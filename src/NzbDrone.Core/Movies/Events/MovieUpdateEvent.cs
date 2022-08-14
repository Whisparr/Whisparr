using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Movies.Events
{
    public class MovieUpdatedEvent : IEvent
    {
        public Media Movie { get; private set; }

        public MovieUpdatedEvent(Media movie)
        {
            Movie = movie;
        }
    }
}
