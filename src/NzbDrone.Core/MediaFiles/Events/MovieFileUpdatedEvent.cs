using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class MovieFileUpdatedEvent : IEvent
    {
        public MediaFile MovieFile { get; private set; }

        public MovieFileUpdatedEvent(MediaFile movieFile)
        {
            MovieFile = movieFile;
        }
    }
}
