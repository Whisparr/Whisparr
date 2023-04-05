using System.Collections.Generic;
using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Movies.Events
{
    public class MoviesImportedEvent : IEvent
    {
        public List<int> MoviesIds { get; private set; }

        public MoviesImportedEvent(List<int> movieIds)
        {
            MoviesIds = movieIds;
        }
    }
}
