using System.Collections.Generic;
using NzbDrone.Common.Cache;
using NzbDrone.Core.ImportLists.ImportListMovies;

namespace NzbDrone.Core.Movies
{
    public interface IMovieMetadataService
    {
        MovieMetadata Get(int id);
        MovieMetadata FindByTmdbId(int tmdbId);
        MovieMetadata FindByForeignId(string foreignId);
        MovieMetadata FindByImdbId(string imdbId);
        bool Upsert(MovieMetadata movie);
        bool UpsertMany(List<MovieMetadata> movies);
        void DeleteMany(List<MovieMetadata> movies);
    }

    public class MovieMetadataService : IMovieMetadataService
    {
        private readonly IMovieMetadataRepository _movieMetadataRepository;
        private readonly IMovieService _movieService;
        private readonly IImportListMovieService _importListMovieService;
        private readonly ICacheManager _cacheManager;
        private readonly string _cacheName;

        public MovieMetadataService(IMovieMetadataRepository movieMetadataRepository, IMovieService movieService, IImportListMovieService importListMovieService, ICacheManager cacheManager)
        {
            _movieMetadataRepository = movieMetadataRepository;
            _movieService = movieService;
            _importListMovieService = importListMovieService;
            _cacheManager = cacheManager;

            _cacheName = "Whisparr.Api.V3.Movies.MovieResource_movieResources";
        }

        public MovieMetadata FindByTmdbId(int tmdbId)
        {
            return _movieMetadataRepository.FindByTmdbId(tmdbId);
        }

        public MovieMetadata FindByForeignId(string foreignId)
        {
            return _movieMetadataRepository.FindByForeignId(foreignId);
        }

        public MovieMetadata FindByImdbId(string imdbId)
        {
            return _movieMetadataRepository.FindByImdbId(imdbId);
        }

        public MovieMetadata Get(int id)
        {
            return _movieMetadataRepository.Get(id);
        }

        public bool Upsert(MovieMetadata movie)
        {
            RemoveMovieResourcesCache(movie.Id.ToString());
            return _movieMetadataRepository.UpsertMany(new List<MovieMetadata> { movie });
        }

        public bool UpsertMany(List<MovieMetadata> movies)
        {
            foreach (var movie in movies)
            {
                RemoveMovieResourcesCache(movie.Id.ToString());
            }

            return _movieMetadataRepository.UpsertMany(movies);
        }

        public void DeleteMany(List<MovieMetadata> movies)
        {
            foreach (var movie in movies)
            {
                RemoveMovieResourcesCache(movie.Id.ToString());

                if (!_importListMovieService.ExistsByMetadataId(movie.Id) && !_movieService.ExistsByMetadataId(movie.Id))
                {
                    _movieMetadataRepository.Delete(movie);
                }
            }
        }

        private void RemoveMovieResourcesCache(string cacheKey)
        {
            var movieResourcesCache = _cacheManager.FindCache(_cacheName);
            if (movieResourcesCache != null)
            {
                movieResourcesCache.Remove(cacheKey);
            }
        }
    }
}
