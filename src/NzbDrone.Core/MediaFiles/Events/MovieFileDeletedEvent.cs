using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class MovieFileDeletedEvent : IEvent
    {
        public MediaFile MovieFile { get; private set; }
        public DeleteMediaFileReason Reason { get; private set; }

        public MovieFileDeletedEvent(MediaFile movieFile, DeleteMediaFileReason reason)
        {
            MovieFile = movieFile;
            Reason = reason;
        }
    }
}
