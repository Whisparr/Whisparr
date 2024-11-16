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
using NzbDrone.Core.Movies.Studios;
using NzbDrone.Core.Movies.Studios.Events;
using NzbDrone.SignalR;
using Whisparr.Http;
using Whisparr.Http.REST;
using Whisparr.Http.REST.Attributes;

namespace Whisparr.Api.V3.Studios
{
    [V3ApiController]
    public class StudioController : RestControllerWithSignalR<StudioResource, Studio>, IHandle<StudioUpdatedEvent>
    {
        private readonly IStudioService _studioService;
        private readonly IAddStudioService _addStudioService;
        private readonly IMapCoversToLocal _coverMapper;
        private readonly IMovieService _moviesService;

        public StudioController(IStudioService studioService,
                                IAddStudioService addStudioService,
                                IMapCoversToLocal coverMapper,
                                IMovieService moviesService,
                                IBroadcastSignalRMessage signalRBroadcaster)
        : base(signalRBroadcaster)
        {
            _studioService = studioService;
            _addStudioService = addStudioService;
            _coverMapper = coverMapper;
            _moviesService = moviesService;
        }

        protected override StudioResource GetResourceById(int id)
        {
            var resource = _studioService.GetById(id).ToResource();

            _coverMapper.ConvertToLocalStudioUrls(resource.Id, resource.Images);

            return resource;
        }

        [HttpGet]
        public List<StudioResource> GetStudios(string stashId)
        {
            var studioResources = new List<StudioResource>();

            if (stashId.IsNotNullOrWhiteSpace())
            {
                var studio = _studioService.FindByForeignId(stashId);

                if (studio != null)
                {
                    studioResources.AddIfNotNull(studio.ToResource());
                }
            }
            else
            {
                studioResources = _studioService.GetAllStudios().ToResource();
            }

            var coverFileInfos = _coverMapper.GetStudioCoverFileInfos();

            _coverMapper.ConvertToLocalStudioUrls(studioResources.Select(x => Tuple.Create(x.Id, x.Images.AsEnumerable())), coverFileInfos);

            return studioResources;
        }

        [RestPostById]
        public ActionResult<StudioResource> AddStudio(StudioResource studioResource)
        {
            var studio = _addStudioService.AddStudio(studioResource.ToModel());

            return Created(studio.Id);
        }

        [RestPutById]
        public ActionResult<StudioResource> Update(StudioResource resource)
        {
            var studio = _studioService.GetById(resource.Id);

            var updatedStudio = _studioService.Update(resource.ToModel(studio));

            BroadcastResourceChange(ModelAction.Updated, updatedStudio.ToResource());

            return Accepted(updatedStudio);
        }

        [RestDeleteById]
        public void DeleteStudio(int id, bool deleteFiles = false, bool addImportExclusion = false)
        {
            var studio = _studioService.GetById(id);

            if (studio == null)
            {
                return;
            }

            // Get the scenes for the studio
            var scenes = _moviesService.GetByStudioForeignId(studio.ForeignId);
            var sceneIds = scenes.Map(x => x.Id).ToList();
            _moviesService.DeleteMovies(sceneIds, deleteFiles, addImportExclusion);

            // Remove the studio now that the associated scenes have been removed
            _studioService.RemoveStudio(studio);
        }

        public void Handle(StudioUpdatedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, message.Studio.ToResource());
        }
    }
}
