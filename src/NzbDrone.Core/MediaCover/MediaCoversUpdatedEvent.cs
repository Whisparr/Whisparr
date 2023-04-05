using NzbDrone.Common.Messaging;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MediaCover
{
    public class MediaCoversUpdatedEvent : IEvent
    {
        public Series Series { get; set; }
        public Movie Movie { get; set; }
        public bool Updated { get; set; }

        public MediaCoversUpdatedEvent(Series series, bool updated)
        {
            Series = series;
            Updated = updated;
        }

        public MediaCoversUpdatedEvent(Movie movie, bool updated)
        {
            Movie = movie;
            Updated = updated;
        }
    }
}
