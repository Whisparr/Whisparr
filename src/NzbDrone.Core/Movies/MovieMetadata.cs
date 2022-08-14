using System;
using System.Collections.Generic;
using Equ;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Movies.AlternativeTitles;
using NzbDrone.Core.Movies.Translations;

namespace NzbDrone.Core.Movies
{
    public class MediaMetadata : Entity<MediaMetadata>
    {
        public MediaMetadata()
        {
            AlternativeTitles = new List<AlternativeTitle>();
            Translations = new List<MovieTranslation>();
            Images = new List<MediaCover.MediaCover>();
            Genres = new List<string>();
            OriginalLanguage = Language.English;
            Recommendations = new List<int>();
            Ratings = new Ratings();
        }

        public int ForiegnId { get; set; }

        public List<MediaCover.MediaCover> Images { get; set; }
        public List<string> Genres { get; set; }
        public DateTime? DigitalRelease { get; set; }
        public string Certification { get; set; }
        public int Year { get; set; }
        public Ratings Ratings { get; set; }

        public MovieCollection Collection { get; set; }
        public DateTime? LastInfoSync { get; set; }
        public int Runtime { get; set; }
        public string Website { get; set; }
        public string Title { get; set; }
        public string CleanTitle { get; set; }
        public string SortTitle { get; set; }
        public MovieStatusType Status { get; set; }
        public ReleaseType ReleaseType { get; set; }
        public string Overview { get; set; }

        //Get Loaded via a Join Query
        public List<AlternativeTitle> AlternativeTitles { get; set; }
        public List<MovieTranslation> Translations { get; set; }

        public int? SecondaryYear { get; set; }
        public string YouTubeTrailerId { get; set; }
        public string Studio { get; set; }
        public string OriginalTitle { get; set; }
        public string CleanOriginalTitle { get; set; }
        public Language OriginalLanguage { get; set; }
        public List<int> Recommendations { get; set; }
        public float Popularity { get; set; }

        [MemberwiseEqualityIgnore]
        public bool IsRecentMovie
        {
            get
            {
                if (DigitalRelease.HasValue)
                {
                    return DigitalRelease.Value >= DateTime.UtcNow.AddDays(-21);
                }

                return true;
            }
        }

        public DateTime PhysicalReleaseDate()
        {
            return DigitalRelease ?? DateTime.MaxValue;
        }
    }

    public enum ReleaseType
    {
        Scene,
        Movie
    }
}
