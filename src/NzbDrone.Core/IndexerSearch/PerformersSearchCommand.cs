using System.Collections.Generic;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.IndexerSearch
{
    public class PerformersSearchCommand : Command
    {
        public PerformersSearchCommand()
        {
            StudioIds = new List<int>();
        }

        public List<int> PerformerIds { get; set; }
        public List<int> StudioIds { get; set; }

        public override bool SendUpdatesToClient => true;
    }
}
