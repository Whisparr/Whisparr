using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.MediaCover
{
    public enum MediaCoverTypes
    {
        Unknown = 0,
        Logo = 1,
        Poster = 2,
        Banner = 3,
        Fanart = 4,
        Screenshot = 5,
        Headshot = 6,
        Clearlogo = 7
    }

    public enum MediaCoverEntity
    {
        Series = 0,
        Movie = 1
    }

    public class MediaCover : IEmbeddedDocument
    {
        public MediaCoverTypes CoverType { get; set; }
        public string Url { get; set; }
        public string RemoteUrl { get; set; }

        public MediaCover()
        {
        }

        public MediaCover(MediaCoverTypes coverType, string remoteUrl)
        {
            CoverType = coverType;
            RemoteUrl = remoteUrl;
        }
    }
}
