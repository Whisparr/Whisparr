using NzbDrone.Common.Messaging;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.MediaCover
{
    public class MediaCoversUpdatedEvent : IEvent
    {
        public Media Movie { get; set; }
        public bool Updated { get; set; }

        public MediaCoversUpdatedEvent(Media movie, bool updated)
        {
            Movie = movie;
            Updated = updated;
        }
    }
}
