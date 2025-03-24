using System.Collections.Generic;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.MediaFiles.Commands
{
    public class RescanScenesCommand : Command
    {
        public override bool SendUpdatesToClient => true;

        public List<string> Folders { get; set; }
        public RescanScenesCommand()
        {
        }
    }
}
