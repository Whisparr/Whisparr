using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Notifications.Stash
{
    public class SyncStashPerformersCommand : Command
    {
        public override bool SendUpdatesToClient => true;
        public override bool IsTypeExclusive => true;
    }
}
