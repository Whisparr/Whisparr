using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Movies.Commands
{
    public class SyncPerformerItemsCommand : Command
    {
        public List<int> PerformerIds { get; set; }

        public SyncPerformerItemsCommand()
        {
            PerformerIds = new List<int>();
        }

        public SyncPerformerItemsCommand(List<int> performerIds)
        {
            PerformerIds = performerIds;
        }

        public override bool SendUpdatesToClient => true;

        public override bool UpdateScheduledTask => !PerformerIds.Any();
    }
}
