using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Movies.Events
{
    public class MovieAddedEvent : IEvent
    {
        public Media Movie { get; private set; }

        public MovieAddedEvent(Media movie)
        {
            Movie = movie;
        }
    }
}
