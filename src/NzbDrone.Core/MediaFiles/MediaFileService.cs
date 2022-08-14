using System.Collections.Generic;
using System.IO;
using System.Linq;
using NzbDrone.Common;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Events;

namespace NzbDrone.Core.MediaFiles
{
    public interface IMediaFileService
    {
        MediaFile Add(MediaFile movieFile);
        void Update(MediaFile movieFile);
        void Update(List<MediaFile> movieFile);
        void Delete(MediaFile movieFile, DeleteMediaFileReason reason);
        List<MediaFile> GetFilesByMovie(int movieId);
        List<MediaFile> GetFilesWithoutMediaInfo();
        List<string> FilterExistingFiles(List<string> files, Media movie);
        MediaFile GetMovie(int id);
        List<MediaFile> GetMovies(IEnumerable<int> ids);
        List<MediaFile> GetFilesWithRelativePath(int movieIds, string relativePath);
    }

    public class MediaFileService : IMediaFileService, IHandleAsync<MoviesDeletedEvent>
    {
        private readonly IMediaFileRepository _mediaFileRepository;
        private readonly IMediaRepository _movieRepository;
        private readonly IEventAggregator _eventAggregator;

        public MediaFileService(IMediaFileRepository mediaFileRepository,
                                IMediaRepository movieRepository,
                                IEventAggregator eventAggregator)
        {
            _mediaFileRepository = mediaFileRepository;
            _movieRepository = movieRepository;
            _eventAggregator = eventAggregator;
        }

        public MediaFile Add(MediaFile movieFile)
        {
            var addedFile = _mediaFileRepository.Insert(movieFile);
            if (addedFile.Movie == null)
            {
                addedFile.Movie = _movieRepository.Get(movieFile.MovieId);
            }

            _eventAggregator.PublishEvent(new MovieFileAddedEvent(addedFile));

            return addedFile;
        }

        public void Update(MediaFile movieFile)
        {
            _mediaFileRepository.Update(movieFile);
        }

        public void Update(List<MediaFile> movieFiles)
        {
            _mediaFileRepository.UpdateMany(movieFiles);
        }

        public void Delete(MediaFile movieFile, DeleteMediaFileReason reason)
        {
            //Little hack so we have the movie attached for the event consumers
            if (movieFile.Movie == null)
            {
                movieFile.Movie = _movieRepository.Get(movieFile.MovieId);
            }

            movieFile.Path = Path.Combine(movieFile.Movie.Path, movieFile.RelativePath);

            _mediaFileRepository.Delete(movieFile);
            _eventAggregator.PublishEvent(new MovieFileDeletedEvent(movieFile, reason));
        }

        public List<MediaFile> GetFilesByMovie(int movieId)
        {
            return _mediaFileRepository.GetFilesByMovie(movieId);
        }

        public List<MediaFile> GetFilesWithoutMediaInfo()
        {
            return _mediaFileRepository.GetFilesWithoutMediaInfo();
        }

        public List<string> FilterExistingFiles(List<string> files, Media movie)
        {
            var movieFiles = GetFilesByMovie(movie.Id).Select(f => Path.Combine(movie.Path, f.RelativePath)).ToList();

            if (!movieFiles.Any())
            {
                return files;
            }

            return files.Except(movieFiles, PathEqualityComparer.Instance).ToList();
        }

        public List<MediaFile> GetMovies(IEnumerable<int> ids)
        {
            return _mediaFileRepository.Get(ids).ToList();
        }

        public MediaFile GetMovie(int id)
        {
            return _mediaFileRepository.Get(id);
        }

        public List<MediaFile> GetFilesWithRelativePath(int movieId, string relativePath)
        {
            return _mediaFileRepository.GetFilesWithRelativePath(movieId, relativePath);
        }

        public void HandleAsync(MoviesDeletedEvent message)
        {
            _mediaFileRepository.DeleteForMovies(message.Movies.Select(m => m.Id).ToList());
        }

        public static List<string> FilterExistingFiles(List<string> files, List<MediaFile> movieFiles, Media movie)
        {
            var seriesFilePaths = movieFiles.Select(f => Path.Combine(movie.Path, f.RelativePath)).ToList();

            if (!seriesFilePaths.Any())
            {
                return files;
            }

            return files.Except(seriesFilePaths, PathEqualityComparer.Instance).ToList();
        }
    }
}
