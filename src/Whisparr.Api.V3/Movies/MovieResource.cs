using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.Movies;
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
        }

        // Todo: Sorters should be done completely on the client
        // Todo: Is there an easy way to keep IgnoreArticlesWhenSorting in sync between, Series, History, Missing?
        // Todo: We should get the entire Profile instead of ID and Name separately

        // View Only
        public string Title { get; set; }
        public Language OriginalLanguage { get; set; }
        public string SortTitle { get; set; }
        public long? SizeOnDisk { get; set; }
        public MovieStatusType Status { get; set; }
        public string Overview { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public string PhysicalReleaseNote { get; set; }
        public List<MediaCover> Images { get; set; }
        public string Website { get; set; }

        // public bool Downloaded { get; set; }
        public string RemotePoster { get; set; }
        public int Year { get; set; }
        public bool HasFile { get; set; }
        public string StudioTitle { get; set; }
        public string StudioForeignId { get; set; }

        // View & Edit
        public string Path { get; set; }
        public int QualityProfileId { get; set; }

        // Editing Only
        public bool Monitored { get; set; }
        public bool IsAvailable { get; set; }
        public string FolderName { get; set; }

        public int Runtime { get; set; }
        public string CleanTitle { get; set; }
        public string ImdbId { get; set; }
        public int TmdbId { get; set; }
        public string ForeignId { get; set; }
        public string StashId { get; set; }
        public string TitleSlug { get; set; }
        public string RootFolderPath { get; set; }
        public string Folder { get; set; }
        public List<string> Genres { get; set; }
        public HashSet<int> Tags { get; set; }
        public DateTime Added { get; set; }
        public AddMovieOptions AddOptions { get; set; }
        public Ratings Ratings { get; set; }
        public MovieFileResource MovieFile { get; set; }
        public List<Credit> Credits { get; set; }
        public ItemType ItemType { get; set; }
    }

    public static class MovieResourceMapper
    {
        public static MovieResource ToResource(this Movie model, int availDelay, IUpgradableSpecification upgradableSpecification = null, ICustomFormatCalculationService formatCalculationService = null)
        {
            if (model == null)
            {
                return null;
            }

            var size = model.MovieFile?.Size ?? 0;

            var movieFile = model.MovieFile?.ToResource(model, upgradableSpecification, formatCalculationService);

            return new MovieResource
            {
                Id = model.Id,
                ForeignId = model.ForeignId,
                TmdbId = model.TmdbId,
                StashId = model.MovieMetadata.Value.StashId,
                Title = model.Title,
                OriginalLanguage = model.MovieMetadata.Value.OriginalLanguage,
                SortTitle = model.Title.NormalizeTitle(),
                ReleaseDate = model.MovieMetadata.Value.ReleaseDate,
                HasFile = model.HasFile,

                SizeOnDisk = size,
                Status = model.MovieMetadata.Value.Status,
                Overview = model.MovieMetadata.Value.Overview,

                Images = model.MovieMetadata.Value.Images.JsonClone(),

                Year = model.Year,

                Path = model.Path,
                QualityProfileId = model.QualityProfileId,

                Monitored = model.Monitored,

                IsAvailable = model.IsAvailable(availDelay),
                FolderName = model.FolderName(),

                Runtime = model.MovieMetadata.Value.Runtime,
                CleanTitle = model.MovieMetadata.Value.CleanTitle,
                ImdbId = model.ImdbId,
                TitleSlug = model.MovieMetadata.Value.ForeignId.ToString(),
                RootFolderPath = model.RootFolderPath,
                Website = model.MovieMetadata.Value.Website,
                Genres = model.MovieMetadata.Value.Genres,
                Tags = model.Tags,
                Added = model.Added,
                AddOptions = model.AddOptions,
                Ratings = model.MovieMetadata.Value.Ratings,
                MovieFile = movieFile,
                StudioTitle = model.MovieMetadata.Value.StudioTitle,
                StudioForeignId = model.MovieMetadata.Value.StudioForeignId,
                Credits = model.MovieMetadata.Value.Credits,
                ItemType = model.MovieMetadata.Value.ItemType
            };
        }

        public static Movie ToModel(this MovieResource resource)
        {
            if (resource == null)
            {
                return null;
            }

            return new Movie
            {
                Id = resource.Id,

                MovieMetadata = new MovieMetadata
                {
                    ForeignId = resource.ForeignId,
                    TmdbId = resource.TmdbId,
                    Title = resource.Title,
                    Genres = resource.Genres,
                    Images = resource.Images,
                    SortTitle = resource.SortTitle,
                    ReleaseDate = resource.ReleaseDate,
                    Year = resource.Year,
                    Overview = resource.Overview,
                    Website = resource.Website,
                    Ratings = resource.Ratings,
                    StudioTitle = resource.StudioTitle,
                    Runtime = resource.Runtime,
                    CleanTitle = resource.CleanTitle,
                    ImdbId = resource.ImdbId,
                },

                Path = resource.Path,
                QualityProfileId = resource.QualityProfileId,

                Monitored = resource.Monitored,

                RootFolderPath = resource.RootFolderPath,

                Tags = resource.Tags,
                Added = resource.Added,
                AddOptions = resource.AddOptions
            };
        }

        public static Movie ToModel(this MovieResource resource, Movie movie)
        {
            var updatedmovie = resource.ToModel();

            movie.ApplyChanges(updatedmovie);

            return movie;
        }

        public static List<MovieResource> ToResource(this IEnumerable<Movie> movies, int availDelay, IUpgradableSpecification upgradableSpecification = null, ICustomFormatCalculationService formatCalculationService = null)
        {
            return movies.Select(x => ToResource(x, availDelay, upgradableSpecification, formatCalculationService)).ToList();
        }

        public static List<Movie> ToModel(this IEnumerable<MovieResource> resources)
        {
            return resources.Select(ToModel).ToList();
        }
    }
}
