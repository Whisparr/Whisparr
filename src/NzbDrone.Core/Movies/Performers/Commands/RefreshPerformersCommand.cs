using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Movies.Performers.Commands
{
    public class RefreshPerformersCommand : Command
    {
        public List<int> PerformerIds { get; set; }

        public RefreshPerformersCommand()
        {
            PerformerIds = new List<int>();
        }

        public RefreshPerformersCommand(List<int> performerIds)
        {
            PerformerIds = performerIds;
        }

        public override bool SendUpdatesToClient => true;

        public override bool UpdateScheduledTask => !PerformerIds.Any();
    }
}
