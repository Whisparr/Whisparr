using System;
using System.Collections.Generic;
using NzbDrone.Core.ImportLists.ImportListMovies;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Collections;
using Whisparr.Http.REST;

namespace Whisparr.Api.V3.ImportLists
{
    public class ImportListMoviesResource : RestResource
    {
        public ImportListMoviesResource()
        {
            Lists = new HashSet<int>();
        }

        public string Title { get; set; }
        public string SortTitle { get; set; }
        public Language OriginalLanguage { get; set; }
        public MovieStatusType Status { get; set; }
        public string Overview { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public List<MediaCover> Images { get; set; }
        public string Website { get; set; }
        public string RemotePoster { get; set; }
        public int Year { get; set; }
        public string Studio { get; set; }

        public int Runtime { get; set; }
        public string ImdbId { get; set; }
        public int TmdbId { get; set; }
        public string ForeignId { get; set; }
        public string Folder { get; set; }
        public List<string> Genres { get; set; }
        public Ratings Ratings { get; set; }
        public MovieCollection Collection { get; set; }
        public bool IsExcluded { get; set; }
        public bool IsExisting { get; set; }

        public bool IsRecommendation { get; set; }
        public HashSet<int> Lists { get; set; }
    }

    public static class DiscoverMoviesResourceMapper
    {
        public static ImportListMoviesResource ToResource(this Movie model)
        {
            if (model == null)
            {
                return null;
            }

            return new ImportListMoviesResource
            {
                TmdbId = model.TmdbId,
                ForeignId = model.ForeignId,
                Title = model.Title,
                SortTitle = model.MovieMetadata.Value.SortTitle,
                OriginalLanguage = model.MovieMetadata.Value.OriginalLanguage,
                ReleaseDate = model.MovieMetadata.Value.ReleaseDate,

                Status = model.MovieMetadata.Value.Status,
                Overview = model.MovieMetadata.Value.Overview,

                Images = model.MovieMetadata.Value.Images,

                Year = model.Year,

                Runtime = model.MovieMetadata.Value.Runtime,
                ImdbId = model.ImdbId,
                Website = model.MovieMetadata.Value.Website,
                Genres = model.MovieMetadata.Value.Genres,
                Ratings = model.MovieMetadata.Value.Ratings,
                Collection = new MovieCollection { Title = model.MovieMetadata.Value.CollectionTitle, TmdbId = model.MovieMetadata.Value.CollectionTmdbId },
                Studio = model.MovieMetadata.Value.Studio
            };
        }

        public static ImportListMoviesResource ToResource(this ImportListMovie model)
        {
            if (model == null)
            {
                return null;
            }

            return new ImportListMoviesResource
            {
                TmdbId = model.TmdbId,
                ForeignId = model.ForeignId,
                Title = model.Title,
                SortTitle = model.MovieMetadata.Value.SortTitle,
                OriginalLanguage = model.MovieMetadata.Value.OriginalLanguage,
                ReleaseDate = model.MovieMetadata.Value.ReleaseDate,

                Status = model.MovieMetadata.Value.Status,
                Overview = model.MovieMetadata.Value.Overview,

                Images = model.MovieMetadata.Value.Images,

                Year = model.Year,

                Runtime = model.MovieMetadata.Value.Runtime,
                ImdbId = model.ImdbId,
                Website = model.MovieMetadata.Value.Website,
                Genres = model.MovieMetadata.Value.Genres,
                Ratings = model.MovieMetadata.Value.Ratings,
                Studio = model.MovieMetadata.Value.Studio,
                Collection = new MovieCollection { Title = model.MovieMetadata.Value.CollectionTitle, TmdbId = model.MovieMetadata.Value.CollectionTmdbId },
                Lists = new HashSet<int> { model.ListId }
            };
        }
    }
}
