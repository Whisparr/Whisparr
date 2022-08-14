using System.Collections.Generic;

namespace NzbDrone.Core.Movies
{
    public interface IMovieMetadataService
    {
        MediaMetadata Get(int id);
        MediaMetadata FindByTmdbId(int tmdbId);
        bool Upsert(MediaMetadata movie);
        bool UpsertMany(List<MediaMetadata> movies);
    }

    public class MovieMetadataService : IMovieMetadataService
    {
        private readonly IMovieMetadataRepository _movieMetadataRepository;

        public MovieMetadataService(IMovieMetadataRepository movieMetadataRepository)
        {
            _movieMetadataRepository = movieMetadataRepository;
        }

        public MediaMetadata FindByTmdbId(int tmdbId)
        {
            return _movieMetadataRepository.FindByTmdbId(tmdbId);
        }

        public MediaMetadata Get(int id)
        {
            return _movieMetadataRepository.Get(id);
        }

        public bool Upsert(MediaMetadata movie)
        {
            return _movieMetadataRepository.UpsertMany(new List<MediaMetadata> { movie });
        }

        public bool UpsertMany(List<MediaMetadata> movies)
        {
            return _movieMetadataRepository.UpsertMany(movies);
        }
    }
}
