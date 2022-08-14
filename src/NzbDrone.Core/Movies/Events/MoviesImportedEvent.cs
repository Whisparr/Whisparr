using System.Collections.Generic;
using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Movies.Events
{
    public class MoviesImportedEvent : IEvent
    {
        public List<Media> Movies { get; private set; }

        public MoviesImportedEvent(List<Media> movies)
        {
            Movies = movies;
        }
    }
}
