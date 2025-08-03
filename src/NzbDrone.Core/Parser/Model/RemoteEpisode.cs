using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Download.Clients;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Parser.Model
{
    public class RemoteEpisode
    {
        public ReleaseInfo Release { get; set; }
        public ParsedEpisodeInfo ParsedEpisodeInfo { get; set; }
        public Series Series { get; set; }
        public List<Episode> Episodes { get; set; }
        public bool EpisodeRequested { get; set; }
        public bool DownloadAllowed { get; set; }
        public TorrentSeedConfiguration SeedConfiguration { get; set; }
        public List<CustomFormat> CustomFormats { get; set; }
        public int CustomFormatScore { get; set; }
        public SeriesMatchType SeriesMatchType { get; set; }
        public List<Language> Languages { get; set; }
        public ReleaseSourceType ReleaseSource { get; set; }
        public bool ShouldOverride { get; set; }

        public RemoteEpisode()
        {
            Episodes = new List<Episode>();
            CustomFormats = new List<CustomFormat>();
            Languages = new List<Language>();
        }

        public bool IsRecentEpisode()
        {
            return Episodes.Any(e => e.AirDateUtc >= DateTime.UtcNow.Date.AddDays(-14));
        }

        public override string ToString()
        {
            return Release.Title;
        }

        public static RemoteEpisode CreateForceOverride(RemoteEpisode source, Series series, List<Episode> episodes, QualityModel quality, List<Language> languages)
        {
            var clone = source.ParsedEpisodeInfo.JsonClone();
            clone.Quality = quality;

            return new RemoteEpisode
            {
                Release = source.Release,
                ParsedEpisodeInfo = clone,
                EpisodeRequested = source.EpisodeRequested,
                DownloadAllowed = source.DownloadAllowed,
                SeedConfiguration = source.SeedConfiguration,
                CustomFormats = source.CustomFormats,
                CustomFormatScore = source.CustomFormatScore,
                SeriesMatchType = source.SeriesMatchType,
                ReleaseSource = source.ReleaseSource,
                Series = series,
                Episodes = episodes,
                Languages = languages,
                ShouldOverride = true
            };
        }
    }

    public enum ReleaseSourceType
    {
        Unknown = 0,
        Rss = 1,
        Search = 2,
        UserInvokedSearch = 3,
        InteractiveSearch = 4,
        ReleasePush = 5
    }
}
