using NzbDrone.Common.Messaging;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class SeriesScanSkippedEvent : IEvent
    {
        public Series Series { get; private set; }
        public EntityScanSkippedReason Reason { get; set; }

        public SeriesScanSkippedEvent(Series series, EntityScanSkippedReason reason)
        {
            Series = series;
            Reason = reason;
        }
    }

    public enum EntityScanSkippedReason
    {
        RootFolderDoesNotExist,
        RootFolderIsEmpty,
        NeverRescanAfterRefresh,
        RescanAfterManualRefreshOnly
    }
}
