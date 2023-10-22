using System;
using System.Collections.Generic;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Profiles.Qualities;

namespace NzbDrone.Core.Tv
{
    public class Series : ModelBase
    {
        public Series()
        {
            Images = new List<MediaCover.MediaCover>();
            Genres = new List<string>();
            Seasons = new List<Season>();
            Tags = new HashSet<int>();
            OriginalLanguage = Language.English;
        }

        public int TvdbId { get; set; }
        public string Title { get; set; }
        public string CleanTitle { get; set; }
        public string SortTitle { get; set; }
        public SeriesStatusType Status { get; set; }
        public string Overview { get; set; }
        public bool Monitored { get; set; }
        public NewItemMonitorTypes MonitorNewItems { get; set; }
        public int QualityProfileId { get; set; }
        public DateTime? LastInfoSync { get; set; }
        public int Runtime { get; set; }
        public List<MediaCover.MediaCover> Images { get; set; }
        public string Network { get; set; }
        public bool UseSceneNumbering { get; set; }
        public string TitleSlug { get; set; }
        public string Path { get; set; }
        public int Year { get; set; }
        public Ratings Ratings { get; set; }
        public List<string> Genres { get; set; }
        public string Certification { get; set; }
        public string RootFolderPath { get; set; }
        public DateTime Added { get; set; }
        public DateTime? FirstAired { get; set; }
        public LazyLoaded<QualityProfile> QualityProfile { get; set; }
        public Language OriginalLanguage { get; set; }

        public List<Season> Seasons { get; set; }
        public HashSet<int> Tags { get; set; }
        public AddSeriesOptions AddOptions { get; set; }

        public override string ToString()
        {
            return string.Format("[{0}][{1}]", TvdbId, Title.NullSafe());
        }

        public void ApplyChanges(Series otherSeries)
        {
            TvdbId = otherSeries.TvdbId;

            Seasons = otherSeries.Seasons;
            Path = otherSeries.Path;
            QualityProfileId = otherSeries.QualityProfileId;

            Monitored = otherSeries.Monitored;
            MonitorNewItems = otherSeries.MonitorNewItems;

            RootFolderPath = otherSeries.RootFolderPath;
            Tags = otherSeries.Tags;
            AddOptions = otherSeries.AddOptions;
        }
    }
}
