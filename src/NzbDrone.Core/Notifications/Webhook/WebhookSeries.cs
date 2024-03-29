using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications.Webhook
{
    public class WebhookSeries
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string TitleSlug { get; set; }
        public string Path { get; set; }
        public int TvdbId { get; set; }

        public WebhookSeries()
        {
        }

        public WebhookSeries(Series series)
        {
            Id = series.Id;
            Title = series.Title;
            TitleSlug = series.TitleSlug;
            Path = series.Path;
            TvdbId = series.TvdbId;
        }
    }
}
