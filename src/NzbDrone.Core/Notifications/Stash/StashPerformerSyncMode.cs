using System.Collections.Generic;

namespace NzbDrone.Core.Notifications.Stash
{
    public enum StashPerformerSyncMode
    {
        Disabled,
        WhisparrToStash,
        StashToWhisparr,
        Bidirectional
    }

    public class StashPerformerSyncState
    {
        public bool StashFavorite { get; set; }
        public bool WhisparrMonitored { get; set; }
        public string StashId { get; set; }
        public int WhisparrId { get; set; }
    }

    public class StashPerformerSyncResult
    {
        public int Checked { get; set; }
        public int Updated { get; set; }
        public int Created { get; set; }
        public int Warnings { get; set; }
        public List<string> WarningMessages { get; } = new List<string>();
    }
}
