using System.Collections.Generic;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.IndexerSearch
{
    public class StudiosSearchCommand : Command
    {
        public StudiosSearchCommand()
        {
            Years = new List<int>();
        }

        public List<int> StudioIds { get; set; }
        public List<int> Years { get; set; }

        public override bool SendUpdatesToClient => true;
    }
}
