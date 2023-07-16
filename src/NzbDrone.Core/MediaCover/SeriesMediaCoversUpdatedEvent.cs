using NzbDrone.Common.Messaging;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MediaCover
{
    public class SeriesMediaCoversUpdatedEvent : IEvent
    {
        public Series Series { get; set; }
        public bool Updated { get; set; }

        public SeriesMediaCoversUpdatedEvent(Series series, bool updated)
        {
            Series = series;
            Updated = updated;
        }
    }
}
