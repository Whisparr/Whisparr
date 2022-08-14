using NzbDrone.Common.Messaging;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class MovieScanSkippedEvent : IEvent
    {
        public Media Movie { get; private set; }
        public MovieScanSkippedReason Reason { get; set; }

        public MovieScanSkippedEvent(Media movie, MovieScanSkippedReason reason)
        {
            Movie = movie;
            Reason = reason;
        }
    }

    public enum MovieScanSkippedReason
    {
        RootFolderDoesNotExist,
        RootFolderIsEmpty,
        MovieFolderDoesNotExist
    }
}
