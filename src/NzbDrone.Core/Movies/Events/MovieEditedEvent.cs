using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Movies.Events
{
    public class MovieEditedEvent : IEvent
    {
        public Media Movie { get; private set; }
        public Media OldMovie { get; private set; }

        public MovieEditedEvent(Media movie, Media oldMovie)
        {
            Movie = movie;
            OldMovie = oldMovie;
        }
    }
}
