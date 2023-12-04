using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Movies.Commands
{
    public class SyncStudioItemsCommand : Command
    {
        public List<int> StudioIds { get; set; }

        public SyncStudioItemsCommand()
        {
            StudioIds = new List<int>();
        }

        public SyncStudioItemsCommand(List<int> studioIds)
        {
            StudioIds = studioIds;
        }

        public override bool SendUpdatesToClient => true;

        public override bool UpdateScheduledTask => !StudioIds.Any();
    }
}
