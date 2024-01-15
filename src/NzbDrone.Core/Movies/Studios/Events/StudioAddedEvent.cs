using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Movies.Studios.Events
{
    public class StudioAddedEvent : IEvent
    {
        public Studio Studio { get; private set; }

        public StudioAddedEvent(Studio studio)
        {
            Studio = studio;
        }
    }
}
