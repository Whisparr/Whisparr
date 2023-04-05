using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Tv;
using Whisparr.Http.REST;

namespace Whisparr.Api.V3.Movies
{
    public class MovieResource : RestResource
    {
        public string Title { get; set; }
        public string SortTitle { get; set; }

        public MovieStatusType Status { get; set; }
        public string Overview { get; set; }
        public List<MediaCover> Images { get; set; }
        public Language OriginalLanguage { get; set; }
        public string RemotePoster { get; set; }
        public int Year { get; set; }

        // View & Edit
        public string Path { get; set; }
        public int QualityProfileId { get; set; }

        // Editing Only
        public bool Monitored { get; set; }

        public int Runtime { get; set; }
        public int TmdbId { get; set; }
        public string CleanTitle { get; set; }
        public string TitleSlug { get; set; }
        public string RootFolderPath { get; set; }
        public string Folder { get; set; }
        public List<string> Genres { get; set; }
        public HashSet<int> Tags { get; set; }
        public DateTime Added { get; set; }
        public AddMovieOptions AddOptions { get; set; }
        public Ratings Ratings { get; set; }

        public MovieStatisticsResource Statistics { get; set; }
    }

    public static class MovieResourceMapper
    {
        public static MovieResource ToResource(this NzbDrone.Core.Movies.Movie model)
        {
            if (model == null)
            {
                return null;
            }

            return new MovieResource
            {
                Id = model.Id,

                Title = model.Title,

                SortTitle = model.SortTitle,

                Status = model.Status,
                Overview = model.Overview,

                Images = model.Images.JsonClone(),

                Year = model.Year,
                OriginalLanguage = model.OriginalLanguage,

                Path = model.Path,
                QualityProfileId = model.QualityProfileId,

                Monitored = model.Monitored,

                Runtime = model.Runtime,
                TmdbId = model.TmdbId,
                CleanTitle = model.CleanTitle,
                TitleSlug = model.TitleSlug,

                // Root folder path needs to be calculated from the series path
                // RootFolderPath = model.RootFolderPath,

                Genres = model.Genres,
                Tags = model.Tags,
                Added = model.Added,
                AddOptions = model.AddOptions,
                Ratings = model.Ratings
            };
        }

        public static NzbDrone.Core.Movies.Movie ToModel(this MovieResource resource)
        {
            if (resource == null)
            {
                return null;
            }

            return new NzbDrone.Core.Movies.Movie
            {
                Id = resource.Id,

                Title = resource.Title,

                SortTitle = resource.SortTitle,

                Status = resource.Status,
                Overview = resource.Overview,

                Images = resource.Images,

                Year = resource.Year,
                OriginalLanguage = resource.OriginalLanguage,

                Path = resource.Path,
                QualityProfileId = resource.QualityProfileId,

                Monitored = resource.Monitored,

                Runtime = resource.Runtime,
                TmdbId = resource.TmdbId,
                CleanTitle = resource.CleanTitle,
                TitleSlug = resource.TitleSlug,
                RootFolderPath = resource.RootFolderPath,
                Genres = resource.Genres,
                Tags = resource.Tags,
                Added = resource.Added,
                AddOptions = resource.AddOptions,
                Ratings = resource.Ratings
            };
        }

        public static NzbDrone.Core.Movies.Movie ToModel(this MovieResource resource, NzbDrone.Core.Movies.Movie movie)
        {
            var updatedSeries = resource.ToModel();

            movie.ApplyChanges(updatedSeries);

            return movie;
        }

        public static List<MovieResource> ToResource(this IEnumerable<NzbDrone.Core.Movies.Movie> movies)
        {
            return movies.Select(s => ToResource(s)).ToList();
        }

        public static List<NzbDrone.Core.Movies.Movie> ToModel(this IEnumerable<MovieResource> resources)
        {
            return resources.Select(ToModel).ToList();
        }
    }
}
