using System;
using System.Collections.Generic;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Movies
{
    public class Movie : ModelBase
    {
        public Movie()
        {
            Images = new List<MediaCover.MediaCover>();
            Genres = new List<string>();
            Tags = new HashSet<int>();
            OriginalLanguage = Language.English;
        }

        public string Title { get; set; }
        public string CleanTitle { get; set; }
        public string SortTitle { get; set; }
        public MovieStatusType Status { get; set; }
        public string Overview { get; set; }
        public bool Monitored { get; set; }
        public int QualityProfileId { get; set; }
        public DateTime? LastInfoSync { get; set; }
        public int Runtime { get; set; }
        public List<MediaCover.MediaCover> Images { get; set; }
        public int TmdbId { get; set; }
        public string RootFolderPath { get; set; }
        public LazyLoaded<QualityProfile> QualityProfile { get; set; }
        public string TitleSlug { get; set; }
        public string Path { get; set; }
        public int Year { get; set; }
        public Ratings Ratings { get; set; }
        public List<string> Genres { get; set; }
        public string Studio { get; set; }
        public DateTime Added { get; set; }
        public Language OriginalLanguage { get; set; }
        public HashSet<int> Tags { get; set; }
        public AddMovieOptions AddOptions { get; set; }

        public override string ToString()
        {
            return string.Format("[{0}][{1}]", TmdbId, Title.NullSafe());
        }

        public void ApplyChanges(Movie otherMovie)
        {
            TmdbId = otherMovie.TmdbId;

            Path = otherMovie.Path;
            QualityProfileId = otherMovie.QualityProfileId;

            Monitored = otherMovie.Monitored;

            RootFolderPath = otherMovie.RootFolderPath;
            Tags = otherMovie.Tags;
            AddOptions = otherMovie.AddOptions;
        }
    }
}
