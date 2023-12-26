using System;
using System.Collections.Generic;
using Equ;
using NzbDrone.Core.Languages;

namespace NzbDrone.Core.Movies
{
    public class MovieMetadata : Entity<MovieMetadata>
    {
        public MovieMetadata()
        {
            Credits = new List<Credit>();
            Images = new List<MediaCover.MediaCover>();
            Genres = new List<string>();
            OriginalLanguage = Language.English;
            Recommendations = new List<int>();
            Ratings = new Ratings();
        }

        public string ForeignId { get; set; }

        public List<MediaCover.MediaCover> Images { get; set; }
        public List<string> Genres { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public int Year { get; set; }
        public Ratings Ratings { get; set; }

        public int CollectionTmdbId { get; set; }
        public string CollectionTitle { get; set; }
        public DateTime? LastInfoSync { get; set; }
        public int Runtime { get; set; }
        public string Website { get; set; }
        public string ImdbId { get; set; }
        public int TmdbId { get; set; }
        public string StashId { get; set; }
        public string Title { get; set; }
        public string CleanTitle { get; set; }
        public string SortTitle { get; set; }
        public MovieStatusType Status { get; set; }
        public string Overview { get; set; }
        public string Studio { get; set; }
        public Language OriginalLanguage { get; set; }
        public List<int> Recommendations { get; set; }
        public ItemType ItemType { get; set; }
        public MetadataSource MetadataSource { get; set; }
        public List<Credit> Credits { get; set; }

        [MemberwiseEqualityIgnore]
        public bool IsRecentMovie
        {
            get
            {
                if (ReleaseDate.HasValue && ReleaseDate.Value >= DateTime.UtcNow.AddDays(-21))
                {
                    return true;
                }

                return false;
            }
        }
    }

    public enum ItemType
    {
        Movie,
        Scene
    }

    public enum MetadataSource
    {
        Tmdb,
        Stash
    }
}
