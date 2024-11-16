using System;
using System.Collections.Generic;
using System.Linq;
using DryIoc.ImTools;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Extensions;
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

        public PerformerController(IPerformerService performerService,
                                   IAddPerformerService addPerformerService,
                                   IMapCoversToLocal coverMapper,
                                   IMovieService moviesService,
                                   IBroadcastSignalRMessage signalRBroadcaster)
        : base(signalRBroadcaster)
        {
            _performerService = performerService;
            _addPerformerService = addPerformerService;
            _coverMapper = coverMapper;
            _moviesService = moviesService;
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
    }
}
