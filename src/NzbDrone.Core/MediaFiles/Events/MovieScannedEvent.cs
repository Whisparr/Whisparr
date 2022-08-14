using NzbDrone.Common.Messaging;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class MovieScannedEvent : IEvent
    {
        public Media Movie { get; private set; }

        public MovieScannedEvent(Media movie)
        {
            Movie = movie;
        }
    }
}
