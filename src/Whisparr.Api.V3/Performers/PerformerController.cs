using System;
using System.Collections.Generic;
using System.Linq;
using DryIoc.ImTools;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.ImportLists.ImportExclusions;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Performers;
using NzbDrone.Core.Movies.Performers.Events;
using NzbDrone.Core.MovieStats;
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
        private readonly IMovieStatisticsService _movieStatisticsService;
        private readonly IImportListExclusionService _exclusionService;
        private readonly IConfigService _configService;
        private readonly bool _useCache;
        private readonly ICached<PerformerResource> _performerResourceCache;

        public PerformerController(IPerformerService performerService,
                                   IAddPerformerService addPerformerService,
                                   IMapCoversToLocal coverMapper,
                                   IMovieService moviesService,
                                   IMovieStatisticsService movieStatisticsService,
                                   IImportListExclusionService exclusionService,
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
            _movieStatisticsService = movieStatisticsService;
            _exclusionService = exclusionService;
            _useCache = _configService.WhisparrCachePerformerAPI;
            _performerResourceCache = cacheManager.GetCache<PerformerResource>(typeof(PerformerResource), "performerResources");
        }

        protected override PerformerResource GetResourceById(int id)
        {
            var resource = _performerService.GetById(id).ToResource();

            _coverMapper.ConvertToLocalPerformerUrls(resource.Id, resource.Images);

            FetchAndLinkMovies(resource);

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

                return performerResources;
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
            }

            var coverFileInfos = _coverMapper.GetPerformerCoverFileInfos();

            _coverMapper.ConvertToLocalPerformerUrls(performerResources.Select(x => Tuple.Create(x.Id, x.Images.AsEnumerable())), coverFileInfos);

            LinkMovies(performerResources);

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
            _moviesService.DeleteMovies(sceneIds, deleteFiles);

            if (addImportExclusion)
            {
                var exclusion = new ImportListExclusion();
                exclusion.ForeignId = performer.ForeignId;
                exclusion.MovieTitle = performer.Name;
                exclusion.Type = ImportExclusionType.Performer;

                _exclusionService.AddExclusion(exclusion);
            }

            // Remove the performer now that the associated scenes have been removed
            _performerService.RemovePerformer(performer);
        }

        [NonAction]
        public void Handle(PerformerUpdatedEvent message)
        {
            var resource = message.Performer.ToResource();

            FetchAndLinkMovies(resource);
            BroadcastResourceChange(ModelAction.Updated, resource);
        }

        private void FetchAndLinkMovies(PerformerResource resource)
        {
            LinkMovies(resource, _moviesService.GetByPerformerForeignId(resource.ForeignId));
        }

        private void LinkMovies(List<PerformerResource> resources)
        {
            foreach (var performer in resources)
            {
                FetchAndLinkMovies(performer);
            }
        }

        private void LinkMovies(PerformerResource resource, List<Movie> movies)
        {
            var scenes = movies.Where(x => x.MovieMetadata.Value.ItemType == ItemType.Scene);
            resource.HasScenes = scenes.Any();
            resource.HasMovies = movies.Where(x => x.MovieMetadata.Value.ItemType == ItemType.Movie).Any();

            resource.Studios = scenes.Map(x => new PerformerStudioResource() { ForeignId = x.MovieMetadata.Value.StudioForeignId, Title = x.MovieMetadata.Value.StudioTitle }).DistinctBy(x => x.ForeignId).OrderBy(x => x.Title).ToList();

            resource.SceneCount = movies.Where(x => x.HasFile).Count();
            resource.TotalSceneCount = movies.Count;
            var ids = movies.Map(x => x.Id).ToList();
            var movieStats = _movieStatisticsService.MovieStatistics(ids);
            resource.SizeOnDisk = movieStats.Sum(x => x.SizeOnDisk);
        }

        private PerformerResource GetPerformerResource(string performerForeignId)
        {
            var performerForeignIds = new List<string> { performerForeignId };
            return GetPerformerResources(performerForeignIds).FirstOrDefault();
        }

        private List<PerformerResource> GetPerformerResources()
        {
            var allPerformerForeignIds = _performerService.AllPerformerForeignIds();
            return GetPerformerResources(allPerformerForeignIds);
        }

        private List<PerformerResource> GetPerformerResources(List<string> performerForeignIds)
        {
            var performerResources = new List<PerformerResource>();

            var missingIds = new List<string>();
            foreach (var id in performerForeignIds)
            {
                var performerResource = _performerResourceCache.Find(id);
                if (performerResource == null)
                {
                    missingIds.Add(id);
                }
                else
                {
                    performerResources.AddIfNotNull(performerResource);
                }
            }

            if (missingIds.Count > 0)
            {
                try
                {
                    _performerResourceCache.Lock.Wait();

                    // recheck after acquiring the lock
                    var getIds = new List<string>();
                    foreach (var id in missingIds)
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

                        LinkMovies(performerResources);

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
