using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Notifications.Webhook
{
    public class WebhookRemoteMovie
    {
        public int TmdbId { get; set; }
        public string ImdbId { get; set; }
        public string Title { get; set; }
        public int Year { get; set; }

        public WebhookRemoteMovie()
        {
        }

        public WebhookRemoteMovie(RemoteMovie remoteMovie)
        {
            TmdbId = remoteMovie.Movie.MediaMetadata.Value.ForiegnId;
            Title = remoteMovie.Movie.MediaMetadata.Value.Title;
            Year = remoteMovie.Movie.MediaMetadata.Value.Year;
        }

        public WebhookRemoteMovie(Media movie)
        {
            TmdbId = movie.MediaMetadata.Value.ForiegnId;
            Title = movie.MediaMetadata.Value.Title;
            Year = movie.MediaMetadata.Value.Year;
        }
    }
}
