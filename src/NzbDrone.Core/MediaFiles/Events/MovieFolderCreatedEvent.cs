using NzbDrone.Common.Messaging;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class MovieFolderCreatedEvent : IEvent
    {
        public Media Movie { get; private set; }
        public MediaFile MovieFile { get; private set; }
        public string MovieFileFolder { get; set; }
        public string MovieFolder { get; set; }

        public MovieFolderCreatedEvent(Media movie, MediaFile movieFile)
        {
            Movie = movie;
            MovieFile = movieFile;
        }
    }
}
