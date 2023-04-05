using NzbDrone.Common.Messaging;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class MovieScanSkippedEvent : IEvent
    {
        public Movie Movie { get; private set; }
        public EntityScanSkippedReason Reason { get; set; }

        public MovieScanSkippedEvent(Movie movie, EntityScanSkippedReason reason)
        {
            Movie = movie;
            Reason = reason;
        }
    }
}
