using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class MovieFileAddedEvent : IEvent
    {
        public MediaFile MovieFile { get; private set; }

        public MovieFileAddedEvent(MediaFile movieFile)
        {
            MovieFile = movieFile;
        }
    }
}
