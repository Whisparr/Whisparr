using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Movies.Performers;
using NzbDrone.SignalR;
using Whisparr.Http;
using Whisparr.Http.REST;
using Whisparr.Http.REST.Attributes;

namespace Whisparr.Api.V3.Performers
{
    [V3ApiController]
    public class PerformerController : RestControllerWithSignalR<PerformerResource, Performer>
    {
        private readonly IPerformerService _performerService;

        public PerformerController(IPerformerService performerService,
                                   IBroadcastSignalRMessage signalRBroadcaster)
        : base(signalRBroadcaster)
        {
            _performerService = performerService;
        }

        protected override PerformerResource GetResourceById(int id)
        {
            return _performerService.GetById(id).ToResource();
        }

        [HttpGet]
        public List<PerformerResource> GetPerformers()
        {
            return _performerService.GetAllPerformers().ToResource();
        }

        [RestPutById]
        public ActionResult<PerformerResource> Update(PerformerResource resource)
        {
            var performer = _performerService.GetById(resource.Id);

            var updatedPerformer = _performerService.Update(resource.ToModel(performer));

            BroadcastResourceChange(ModelAction.Updated, updatedPerformer.ToResource());

            return Accepted(updatedPerformer);
        }
    }
}
