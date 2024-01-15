using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Movies.Performers.Events
{
    public class PerformerAddedEvent : IEvent
    {
        public Performer Performer { get; private set; }

        public PerformerAddedEvent(Performer performer)
        {
            Performer = performer;
        }
    }
}
