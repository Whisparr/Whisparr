using NzbDrone.Common.Messaging;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class MovieFileRenamedEvent : IEvent
    {
        public Media Movie { get; private set; }
        public MediaFile MovieFile { get; private set; }
        public string OriginalPath { get; private set; }

        public MovieFileRenamedEvent(Media movie, MediaFile movieFile, string originalPath)
        {
            Movie = movie;
            MovieFile = movieFile;
            OriginalPath = originalPath;
        }
    }
}
