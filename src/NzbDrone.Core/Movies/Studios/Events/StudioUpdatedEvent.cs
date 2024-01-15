using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Movies.Studios.Events
{
    public class StudioUpdatedEvent : IEvent
    {
        public Studio Studio { get; private set; }

        public StudioUpdatedEvent(Studio studio)
        {
            Studio = studio;
        }
    }
}
