using System.Collections.Generic;
using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Movies.Studios.Events
{
    public class StudiosAddedEvent : IEvent
    {
        public List<Studio> Studios { get; private set; }

        public StudiosAddedEvent(List<Studio> studios)
        {
            Studios = studios;
        }
    }
}
