using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Movies.Performers;
using Whisparr.Http;

namespace Whisparr.Api.V3.Performers
{
    [V3ApiController("performer/editor")]
    public class PerformerEditorController : Controller
    {
        private readonly IPerformerService _performerService;
        private readonly IManageCommandQueue _commandQueueManager;
        private readonly IUpgradableSpecification _upgradableSpecification;

        public PerformerEditorController(IPerformerService performerService, IManageCommandQueue commandQueueManager, IUpgradableSpecification upgradableSpecification)
        {
            _performerService = performerService;
            _commandQueueManager = commandQueueManager;
            _upgradableSpecification = upgradableSpecification;
        }

        [HttpPut]
        public IActionResult SaveAll([FromBody] PerformerEditorResource resource)
        {
            var performersToUpdate = _performerService.GetPerformers(resource.PerformerIds);

            foreach (var performer in performersToUpdate)
            {
                if (resource.Monitored.HasValue)
                {
                    performer.Monitored = resource.Monitored.Value;
                }

                if (resource.QualityProfileId.HasValue)
                {
                    performer.QualityProfileId = resource.QualityProfileId.Value;
                }

                if (resource.RootFolderPath.IsNotNullOrWhiteSpace())
                {
                    performer.RootFolderPath = resource.RootFolderPath;
                }

                if (resource.Tags != null)
                {
                    var newTags = resource.Tags;
                    var applyTags = resource.ApplyTags;

                    switch (applyTags)
                    {
                        case ApplyTags.Add:
                            newTags.ForEach(t => performer.Tags.Add(t));
                            break;
                        case ApplyTags.Remove:
                            newTags.ForEach(t => performer.Tags.Remove(t));
                            break;
                        case ApplyTags.Replace:
                            performer.Tags = new HashSet<int>(newTags);
                            break;
                    }
                }
            }

            return Accepted(_performerService.Update(performersToUpdate).ToResource());
        }
    }
}
