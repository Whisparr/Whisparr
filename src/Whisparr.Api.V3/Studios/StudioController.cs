using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.Messaging.Events;
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
        private readonly IMapCoversToLocal _coverMapper;

        public StudioController(IStudioService studioService,
                                IMapCoversToLocal coverMapper,
                                IBroadcastSignalRMessage signalRBroadcaster)
        : base(signalRBroadcaster)
        {
            _studioService = studioService;
            _coverMapper = coverMapper;
        }

        protected override StudioResource GetResourceById(int id)
        {
            var resource = _studioService.GetById(id).ToResource();

            _coverMapper.ConvertToLocalStudioUrls(resource.Id, resource.Images);

            return resource;
        }

        [HttpGet]
        public List<StudioResource> GetStudios()
        {
            var studioResources = _studioService.GetAllStudios().ToResource();

            var coverFileInfos = _coverMapper.GetStudioCoverFileInfos();

            _coverMapper.ConvertToLocalStudioUrls(studioResources.Select(x => Tuple.Create(x.Id, x.Images.AsEnumerable())), coverFileInfos);

            return studioResources;
        }

        [RestPutById]
        public ActionResult<StudioResource> Update(StudioResource resource)
        {
            var studio = _studioService.GetById(resource.Id);

            var updatedStudio = _studioService.Update(resource.ToModel(studio));

            BroadcastResourceChange(ModelAction.Updated, updatedStudio.ToResource());

            return Accepted(updatedStudio);
        }

        public void Handle(StudioUpdatedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, message.Studio.ToResource());
        }
    }
}
