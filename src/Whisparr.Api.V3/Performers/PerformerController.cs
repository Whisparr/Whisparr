using System;
using System.Collections.Generic;
using System.Linq;
using DryIoc.ImTools;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Performers;
using NzbDrone.Core.Movies.Performers.Events;
using NzbDrone.SignalR;
using Whisparr.Http;
using Whisparr.Http.REST;
using Whisparr.Http.REST.Attributes;

namespace Whisparr.Api.V3.Performers
{
    [V3ApiController]
    public class PerformerController : RestControllerWithSignalR<PerformerResource, Performer>, IHandle<PerformerUpdatedEvent>
    {
        private readonly IPerformerService _performerService;
        private readonly IAddPerformerService _addPerformerService;
        private readonly IMapCoversToLocal _coverMapper;
        private readonly IMovieService _moviesService;
        private readonly IConfigService _configService;
        private readonly bool _useCache;
        private readonly ICached<PerformerResource> _performerResourceCache;

        public PerformerController(IPerformerService performerService,
                                   IAddPerformerService addPerformerService,
                                   IMapCoversToLocal coverMapper,
                                   IMovieService moviesService,
                                   ICacheManager cacheManager,
                                   IConfigService configService,
                                   IBroadcastSignalRMessage signalRBroadcaster)
        : base(signalRBroadcaster)
        {
            _performerService = performerService;
            _addPerformerService = addPerformerService;
            _configService = configService;
            _coverMapper = coverMapper;
            _moviesService = moviesService;
            _useCache = _configService.WhisparrCacheAPI;
            _performerResourceCache = cacheManager.GetCache<PerformerResource>(typeof(PerformerResource), "performerResources");
        }

        protected override PerformerResource GetResourceById(int id)
        {
            var resource = _performerService.GetById(id).ToResource();

            _coverMapper.ConvertToLocalPerformerUrls(resource.Id, resource.Images);

            return resource;
        }

        [HttpGet]
        public List<PerformerResource> GetPerformers(string stashId)
        {
            var performerResources = new List<PerformerResource>();

            if (_useCache)
            {
                if (stashId.IsNotNullOrWhiteSpace())
                {
                    performerResources.AddIfNotNull(GetPerformerResource(stashId));
                }
                else
                {
                    performerResources = GetPerformerResources();
                }
            }
            else
            {
                if (stashId.IsNotNullOrWhiteSpace())
                {
                    var performer = _performerService.FindByForeignId(stashId);

                    if (performer != null)
                    {
                        performerResources.AddIfNotNull(performer.ToResource());
                    }
                }
                else
                {
                    performerResources = _performerService.GetAllPerformers().ToResource();
                }

                var coverFileInfos = _coverMapper.GetPerformerCoverFileInfos();

                _coverMapper.ConvertToLocalPerformerUrls(performerResources.Select(x => Tuple.Create(x.Id, x.Images.AsEnumerable())), coverFileInfos);
            }

            return performerResources;
        }

        [RestPostById]
        public ActionResult<PerformerResource> AddPerformer(PerformerResource performerResource)
        {
            var performer = _addPerformerService.AddPerformer(performerResource.ToModel());

            return Created(performer.Id);
        }

        [RestPutById]
        public ActionResult<PerformerResource> Update(PerformerResource resource)
        {
            var performer = _performerService.GetById(resource.Id);

            var updatedPerformer = _performerService.Update(resource.ToModel(performer));

            BroadcastResourceChange(ModelAction.Updated, updatedPerformer.ToResource());

            return Accepted(updatedPerformer);
        }

        [RestDeleteById]
        public void DeletePerformer(int id, bool deleteFiles = false, bool addImportExclusion = false)
        {
            var performer = _performerService.GetById(id);

            if (performer == null)
            {
                return;
            }

            // Get the scenes for the performer
            var scenes = _moviesService.GetByPerformerForeignId(performer.ForeignId);
            var sceneIds = scenes.Map(x => x.Id).ToList();
            _moviesService.DeleteMovies(sceneIds, deleteFiles, addImportExclusion);

            // Remove the performer now that the associated scenes have been removed
            _performerService.RemovePerformer(performer);
        }

        public void Handle(PerformerUpdatedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, message.Performer.ToResource());
        }

        private PerformerResource GetPerformerResource(string performerForeignId)
        {
            var performerForeignIds = new List<string> { performerForeignId };
            return GetMovieResources(performerForeignIds).FirstOrDefault();
        }

        private List<PerformerResource> GetPerformerResources()
        {
            var allPerformerForeignIds = _performerService.AllPerformerForeignIds();
            return GetMovieResources(allPerformerForeignIds);
        }

        private List<PerformerResource> GetMovieResources(List<string> performerForeignIds)
        {
            var performerResources = new List<PerformerResource>();

            var getIds = new List<string>();
            foreach (var id in performerForeignIds)
            {
                var performerResource = _performerResourceCache.Find(id);
                if (performerResource == null)
                {
                    getIds.Add(id);
                }
                else
                {
                    performerResources.AddIfNotNull(performerResource);
                }
            }

            if (getIds.Count > 0)
            {
                try
                {
                    _performerResourceCache.Lock.Wait();

                    // Recheck outstanding Ids
                    getIds.Clear();
                    foreach (var id in performerForeignIds)
                    {
                        var performerResource = _performerResourceCache.Find(id);
                        if (performerResource == null)
                        {
                            getIds.Add(id);
                        }
                        else
                        {
                            performerResources.AddIfNotNull(performerResource);
                        }
                    }

                    if (getIds.Count > 0)
                    {
                        var performers = _performerService.FindByForeignIds(getIds);

                        foreach (var performer in performers)
                        {
                            performerResources.AddIfNotNull(performer.ToResource());
                        }

                        var coverFileInfos = _coverMapper.GetPerformerCoverFileInfos();

                        _coverMapper.ConvertToLocalPerformerUrls(performerResources.Select(x => Tuple.Create(x.Id, x.Images.AsEnumerable())), coverFileInfos);

                        foreach (var performerResource in performerResources)
                        {
                            _performerResourceCache.Set(performerResource.ForeignId, performerResource);
                        }
                    }
                }
                finally
                {
                    _performerResourceCache.Lock.Release();
                }
            }

            return performerResources;
        }
    }
}
