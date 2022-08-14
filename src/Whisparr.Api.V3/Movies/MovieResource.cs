using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Translations;
using NzbDrone.Core.Parser;
using Whisparr.Api.V3.MovieFiles;
using Whisparr.Http.REST;

namespace Whisparr.Api.V3.Movies
{
    public class MovieResource : RestResource
    {
        public MovieResource()
        {
            Monitored = true;
            MinimumAvailability = MovieStatusType.Released;
        }

        //Todo: Sorters should be done completely on the client
        //Todo: Is there an easy way to keep IgnoreArticlesWhenSorting in sync between, Series, History, Missing?
        //Todo: We should get the entire Profile instead of ID and Name separately

        //View Only
        public string Title { get; set; }
        public string OriginalTitle { get; set; }
        public Language OriginalLanguage { get; set; }
        public List<AlternativeTitleResource> AlternateTitles { get; set; }
        public int? SecondaryYear { get; set; }
        public int SecondaryYearSourceId { get; set; }
        public string SortTitle { get; set; }
        public long? SizeOnDisk { get; set; }
        public MovieStatusType Status { get; set; }
        public string Overview { get; set; }
        public DateTime? InCinemas { get; set; }
        public DateTime? PhysicalRelease { get; set; }
        public DateTime? DigitalRelease { get; set; }
        public string PhysicalReleaseNote { get; set; }
        public List<MediaCover> Images { get; set; }
        public string Website { get; set; }

        //public bool Downloaded { get; set; }
        public string RemotePoster { get; set; }
        public int Year { get; set; }
        public bool HasFile { get; set; }
        public string YouTubeTrailerId { get; set; }
        public string Studio { get; set; }

        //View & Edit
        public string Path { get; set; }
        public int QualityProfileId { get; set; }

        //Editing Only
        public bool Monitored { get; set; }
        public MovieStatusType MinimumAvailability { get; set; }
        public bool IsAvailable { get; set; }
        public string FolderName { get; set; }

        public int Runtime { get; set; }
        public string CleanTitle { get; set; }
        public string ImdbId { get; set; }
        public int TmdbId { get; set; }
        public string TitleSlug { get; set; }
        public string RootFolderPath { get; set; }
        public string Folder { get; set; }
        public string Certification { get; set; }
        public List<string> Genres { get; set; }
        public HashSet<int> Tags { get; set; }
        public DateTime Added { get; set; }
        public AddMovieOptions AddOptions { get; set; }
        public Ratings Ratings { get; set; }
        public MovieFileResource MovieFile { get; set; }
        public MovieCollection Collection { get; set; }
        public float Popularity { get; set; }
    }

    public static class MovieResourceMapper
    {
        public static MovieResource ToResource(this Media model, int availDelay, MovieTranslation movieTranslation = null, IUpgradableSpecification upgradableSpecification = null)
        {
            if (model == null)
            {
                return null;
            }

            long size = model.MovieFile?.Size ?? 0;

            MovieFileResource movieFile = model.MovieFile?.ToResource(model, upgradableSpecification);

            var translatedTitle = movieTranslation?.Title ?? model.Title;
            var translatedOverview = movieTranslation?.Overview ?? model.MediaMetadata.Value.Overview;

            return new MovieResource
            {
                Id = model.Id,
                TmdbId = model.ForiegnId,
                Title = translatedTitle,
                OriginalTitle = model.MediaMetadata.Value.OriginalTitle,
                OriginalLanguage = model.MediaMetadata.Value.OriginalLanguage,
                SortTitle = translatedTitle.NormalizeTitle(),
                DigitalRelease = model.MediaMetadata.Value.DigitalRelease,
                HasFile = model.HasFile,

                SizeOnDisk = size,
                Status = model.MediaMetadata.Value.Status,
                Overview = translatedOverview,

                Images = model.MediaMetadata.Value.Images,

                Year = model.Year,
                SecondaryYear = model.MediaMetadata.Value.SecondaryYear,

                Path = model.Path,
                QualityProfileId = model.ProfileId,

                Monitored = model.Monitored,
                MinimumAvailability = model.MinimumAvailability,

                IsAvailable = model.IsAvailable(availDelay),
                FolderName = model.FolderName(),

                Runtime = model.MediaMetadata.Value.Runtime,
                CleanTitle = model.MediaMetadata.Value.CleanTitle,
                TitleSlug = model.MediaMetadata.Value.ForiegnId.ToString(),
                RootFolderPath = model.RootFolderPath,
                Certification = model.MediaMetadata.Value.Certification,
                Website = model.MediaMetadata.Value.Website,
                Genres = model.MediaMetadata.Value.Genres,
                Tags = model.Tags,
                Added = model.Added,
                AddOptions = model.AddOptions,
                AlternateTitles = model.MediaMetadata.Value.AlternativeTitles.ToResource(),
                Ratings = model.MediaMetadata.Value.Ratings,
                MovieFile = movieFile,
                YouTubeTrailerId = model.MediaMetadata.Value.YouTubeTrailerId,
                Studio = model.MediaMetadata.Value.Studio,
                Collection = model.MediaMetadata.Value.Collection,
                Popularity = model.MediaMetadata.Value.Popularity
            };
        }

        public static Media ToModel(this MovieResource resource)
        {
            if (resource == null)
            {
                return null;
            }

            return new Media
            {
                Id = resource.Id,

                MediaMetadata = new MediaMetadata
                {
                    ForiegnId = resource.TmdbId,
                    Title = resource.Title,
                    Genres = resource.Genres,
                    Images = resource.Images,
                    OriginalTitle = resource.OriginalTitle,
                    SortTitle = resource.SortTitle,
                    Year = resource.Year,
                    SecondaryYear = resource.SecondaryYear,
                    Overview = resource.Overview,
                    Certification = resource.Certification,
                    Website = resource.Website,
                    Ratings = resource.Ratings,
                    YouTubeTrailerId = resource.YouTubeTrailerId,
                    Studio = resource.Studio,
                    Runtime = resource.Runtime,
                    CleanTitle = resource.CleanTitle
                },

                Path = resource.Path,
                ProfileId = resource.QualityProfileId,

                Monitored = resource.Monitored,
                MinimumAvailability = resource.MinimumAvailability,

                RootFolderPath = resource.RootFolderPath,

                Tags = resource.Tags,
                Added = resource.Added,
                AddOptions = resource.AddOptions
            };
        }

        public static Media ToModel(this MovieResource resource, Media movie)
        {
            var updatedmovie = resource.ToModel();

            movie.ApplyChanges(updatedmovie);

            return movie;
        }

        public static List<MovieResource> ToResource(this IEnumerable<Media> movies, int availDelay, IUpgradableSpecification upgradableSpecification = null)
        {
            return movies.Select(x => ToResource(x, availDelay, null, upgradableSpecification)).ToList();
        }

        public static List<Media> ToModel(this IEnumerable<MovieResource> resources)
        {
            return resources.Select(ToModel).ToList();
        }
    }
}
