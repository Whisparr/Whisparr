using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Movies.Studios.Commands
{
    public class RefreshStudiosCommand : Command
    {
        public List<int> StudioIds { get; set; }

        public RefreshStudiosCommand()
        {
            StudioIds = new List<int>();
        }

        public RefreshStudiosCommand(List<int> studioIds)
        {
            StudioIds = studioIds;
        }

        public override bool SendUpdatesToClient => true;

        public override bool UpdateScheduledTask => !StudioIds.Any();
    }
}
