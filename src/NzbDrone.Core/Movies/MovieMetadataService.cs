using System.Collections.Generic;

namespace NzbDrone.Core.Movies
{
    public interface IMovieMetadataService
    {
        MovieMetadata Get(int id);
        MovieMetadata FindByTmdbId(int tmdbId);
        MovieMetadata FindByImdbId(string imdbId);
        bool Upsert(MovieMetadata movie);
        bool UpsertMany(List<MovieMetadata> movies);
    }

    public class MovieMetadataService : IMovieMetadataService
    {
        private readonly IMovieMetadataRepository _movieMetadataRepository;

        public MovieMetadataService(IMovieMetadataRepository movieMetadataRepository)
        {
            _movieMetadataRepository = movieMetadataRepository;
        }

        public MovieMetadata FindByTmdbId(int tmdbId)
        {
            return _movieMetadataRepository.FindByTmdbId(tmdbId);
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
            return _movieMetadataRepository.UpsertMany(new List<MovieMetadata> { movie });
        }

        public bool UpsertMany(List<MovieMetadata> movies)
        {
            return _movieMetadataRepository.UpsertMany(movies);
        }
    }
}
