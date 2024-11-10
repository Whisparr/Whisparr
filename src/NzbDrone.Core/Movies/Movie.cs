using System;
using System.Collections.Generic;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Profiles.Qualities;

namespace NzbDrone.Core.Movies
{
    public class Movie : ModelBase
    {
        public Movie()
        {
            Tags = new HashSet<int>();
            MovieMetadata = new MovieMetadata();
        }

        public const string RELEASE_DATE_FORMAT = "yyyy-MM-dd";

        public int MovieMetadataId { get; set; }

        public bool Monitored { get; set; }
        public int QualityProfileId { get; set; }

        public string Path { get; set; }

        public LazyLoaded<MovieMetadata> MovieMetadata { get; set; }

        public string RootFolderPath { get; set; }
        public DateTime Added { get; set; }
        public QualityProfile QualityProfile { get; set; }
        public HashSet<int> Tags { get; set; }
        public AddMovieOptions AddOptions { get; set; }
        public DateTime? LastSearchTime { get; set; }
        public MovieFile MovieFile { get; set; }
        public int MovieFileId { get; set; }

        public bool HasFile => MovieFileId > 0;

        // compatibility properties
        public string Title
        {
            get { return MovieMetadata.Value.Title; }
            set { MovieMetadata.Value.Title = value; }
        }

        public string ForeignId
        {
            get { return MovieMetadata.Value.ForeignId; }
            set { MovieMetadata.Value.ForeignId = value; }
        }

        public int TmdbId
        {
            get { return MovieMetadata.Value.TmdbId; }
            set { MovieMetadata.Value.TmdbId = value; }
        }

        public string ImdbId
        {
            get { return MovieMetadata.Value.ImdbId; }
            set { MovieMetadata.Value.ImdbId = value; }
        }

        public int Year
        {
            get { return MovieMetadata.Value.Year; }
            set { MovieMetadata.Value.Year = value; }
        }

        public string FolderName()
        {
            if (Path.IsNullOrWhiteSpace())
            {
                return "";
            }

            // Well what about Path = Null?
            // return new DirectoryInfo(Path).Name;
            return Path;
        }

        public bool IsAvailable(int delay = 0)
        {
            // the below line is what was used before delay was implemented, could still be used for cases when delay==0
            return MovieMetadata.Value.Status == MovieStatusType.Released;
        }

        public override string ToString()
        {
            var result = string.Empty;

            if (MovieMetadata.Value.ItemType == ItemType.Scene)
            {
                if (MovieMetadata.Value.Title != null)
                {
                    result += string.Format("[{0} - {1} - {2}]", MovieMetadata.Value.StudioTitle.NullSafe(), MovieMetadata.Value.ReleaseDate.NullSafe(), MovieMetadata.Value.Title.NullSafe());
                }
            }
            else
            {
                if (MovieMetadata.Value.Title != null)
                {
                    result += string.Format("[{0} ({1})]", MovieMetadata.Value.Title.NullSafe(), MovieMetadata.Value.Year.NullSafe());
                }
            }

            if (MovieMetadata.Value.ImdbId != null)
            {
                result += $"[{MovieMetadata.Value.ImdbId}]";
            }

            if (MovieMetadata.Value.ForeignId != null)
            {
                result += $"[{MovieMetadata.Value.ForeignId}]";
            }

            return result;
        }

        public void ApplyChanges(Movie otherMovie)
        {
            Path = otherMovie.Path;
            QualityProfileId = otherMovie.QualityProfileId;

            Monitored = otherMovie.Monitored;

            RootFolderPath = otherMovie.RootFolderPath;
            Tags = otherMovie.Tags;
            AddOptions = otherMovie.AddOptions;
        }
    }
}
