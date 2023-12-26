using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Movies.Studios;
using NzbDrone.SignalR;
using Whisparr.Http;
using Whisparr.Http.REST;

namespace Whisparr.Api.V3.Studios
{
    [V3ApiController]
    public class StudioController : RestControllerWithSignalR<StudioResource, Studio>
    {
        private readonly IStudioService _studioService;

        public StudioController(IStudioService studioService,
                                   IBroadcastSignalRMessage signalRBroadcaster)
        : base(signalRBroadcaster)
        {
            _studioService = studioService;
        }

        protected override StudioResource GetResourceById(int id)
        {
            return _studioService.GetById(id).ToResource();
        }

        [HttpGet]
        public List<StudioResource> GetStudios()
        {
            return _studioService.GetAllStudios().ToResource();
        }
    }
}
