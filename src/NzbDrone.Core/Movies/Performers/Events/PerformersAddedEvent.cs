using System.Collections.Generic;
using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Movies.Performers.Events
{
    public class PerformersAddedEvent : IEvent
    {
        public List<Performer> Performers { get; private set; }

        public PerformersAddedEvent(List<Performer> performers)
        {
            Performers = performers;
        }
    }
}
