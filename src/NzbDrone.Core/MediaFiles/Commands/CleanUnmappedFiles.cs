using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.MediaFiles.Commands
{
    public class CleanUnmappedFilesCommand : Command
    {
        public override bool SendUpdatesToClient => true;
        public CleanUnmappedFilesCommand()
        {
        }
    }
}
