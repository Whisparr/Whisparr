using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Movies.Performers.Events
{
    public class PerformerUpdatedEvent : IEvent
    {
        public Performer Performer { get; private set; }

        public PerformerUpdatedEvent(Performer performer)
        {
            Performer = performer;
        }
    }
}
