using System.Collections.Generic;
using System.IO;

// using System.IO.Abstractions;
using System.Linq;
using NzbDrone.Common;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Events;

namespace NzbDrone.Core.MediaFiles
{
    public interface IMediaFileService
    {
        MovieFile Add(MovieFile movieFile);
        void AddMany(List<MovieFile> movieFiles);
        void Update(MovieFile movieFile);
        void Update(List<MovieFile> movieFile);
        void Delete(MovieFile movieFile, DeleteMediaFileReason reason);
        void DeleteMany(List<MovieFile> movieFiles, DeleteMediaFileReason reason);
        List<MovieFile> GetFilesByMovie(int movieId);
        List<MovieFile> GetFilesByMovies(IEnumerable<int> movieIds);
        List<MovieFile> GetFilesWithoutMediaInfo();
        List<MovieFile> GetUnmappedFiles();
        List<string> FilterExistingFiles(List<string> files, Movie movie);
        List<string> FilterExistingFiles(List<string> files);
        MovieFile GetMovie(int id);
        List<MovieFile> GetMovies(IEnumerable<int> ids);
        List<MovieFile> GetFilesWithBasePath(string path);
        List<MovieFile> GetFilesWithRelativePath(int movieIds, string relativePath);
    }

    public class MediaFileService : IMediaFileService, IHandleAsync<MoviesDeletedEvent>
    {
        private readonly IMediaFileRepository _mediaFileRepository;
        private readonly IMovieRepository _movieRepository;
        private readonly IEventAggregator _eventAggregator;

        public MediaFileService(IMediaFileRepository mediaFileRepository,
                                IMovieRepository movieRepository,
                                IEventAggregator eventAggregator)
        {
            _mediaFileRepository = mediaFileRepository;
            _movieRepository = movieRepository;
            _eventAggregator = eventAggregator;
        }

        public MovieFile Add(MovieFile movieFile)
        {
            var addedFile = _mediaFileRepository.Insert(movieFile);
            if (addedFile.Movie == null && addedFile.RelativePath != null)
            {
                addedFile.Movie = _movieRepository.Get(movieFile.MovieId);
            }

            _eventAggregator.PublishEvent(new MovieFileAddedEvent(addedFile));

            return addedFile;
        }

        public void AddMany(List<MovieFile> movieFiles)
        {
            _mediaFileRepository.InsertMany(movieFiles);
            foreach (var addedFile in movieFiles)
            {
                _eventAggregator.PublishEvent(new MovieFileAddedEvent(addedFile));
            }
        }

        public void Update(MovieFile movieFile)
        {
            _mediaFileRepository.Update(movieFile);
        }

        public void Update(List<MovieFile> movieFiles)
        {
            _mediaFileRepository.UpdateMany(movieFiles);
        }

        public void Delete(MovieFile movieFile, DeleteMediaFileReason reason)
        {
            // Little hack so we have the movie attached for the event consumers
            if (movieFile.Movie == null && movieFile.RelativePath != null)
            {
                movieFile.Movie = _movieRepository.Get(movieFile.MovieId);
                movieFile.Path = Path.Combine(movieFile.Movie.Path, movieFile.RelativePath);
            }

            _mediaFileRepository.Delete(movieFile);
            _eventAggregator.PublishEvent(new MovieFileDeletedEvent(movieFile, reason));
        }

        public void DeleteMany(List<MovieFile> movieFiles, DeleteMediaFileReason reason)
        {
            _mediaFileRepository.DeleteMany(movieFiles);
            foreach (var movieFile in movieFiles)
            {
                _eventAggregator.PublishEvent(new MovieFileDeletedEvent(movieFile, reason));
            }
        }

        public List<MovieFile> GetFilesByMovie(int movieId)
        {
            return _mediaFileRepository.GetFilesByMovie(movieId);
        }

        public List<MovieFile> GetFilesByMovies(IEnumerable<int> movieIds)
        {
            return _mediaFileRepository.GetFilesByMovies(movieIds);
        }

        public List<MovieFile> GetFilesWithoutMediaInfo()
        {
            return _mediaFileRepository.GetFilesWithoutMediaInfo();
        }

        public List<string> FilterExistingFiles(List<string> files, Movie movie)
        {
            var movieFiles = GetFilesByMovie(movie.Id).Select(f => Path.Combine(movie.Path, f.RelativePath)).ToList();

            if (!movieFiles.Any())
            {
                return files;
            }

            return files.Except(movieFiles, PathEqualityComparer.Instance).ToList();
        }

        public List<string> FilterExistingFiles(List<string> files)
        {
            var allFiles = _mediaFileRepository.GetAllFiles().ToList();

            var movieFiles = new List<string>();
            foreach (var file in allFiles)
            {
                if (file.MovieId > 0)
                {
                    try
                    {
                        var movie = _movieRepository.Get(file.MovieId);
                        movieFiles.Add(Path.Combine(movie.Path, movie.MovieFile.RelativePath));
                    }
                    catch (ModelNotFoundException)
                    {
                        // Movie File record exists but not movie record
                        _movieRepository.Delete(file.MovieId);
                    }
                }
                else
                {
                    movieFiles.Add(file.OriginalFilePath);
                }
            }

            if (!movieFiles.Any())
            {
                return files;
            }

            return files.Except(movieFiles, PathEqualityComparer.Instance).ToList();

            // return files;
        }

        public List<MovieFile> GetMovies(IEnumerable<int> ids)
        {
            return _mediaFileRepository.Get(ids).ToList();
        }

        public MovieFile GetMovie(int id)
        {
            return _mediaFileRepository.Get(id);
        }

        public List<MovieFile> GetFilesWithBasePath(string path)
        {
            return _mediaFileRepository.GetFilesWithBasePath(path);
        }

        public List<MovieFile> GetFilesWithRelativePath(int movieId, string relativePath)
        {
            return _mediaFileRepository.GetFilesWithRelativePath(movieId, relativePath);
        }

        public List<MovieFile> GetUnmappedFiles()
        {
            return _mediaFileRepository.GetUnmappedFiles();
        }

        public void HandleAsync(MoviesDeletedEvent message)
        {
            _mediaFileRepository.DeleteForMovies(message.Movies.Select(m => m.Id).ToList());
        }

        public static List<string> FilterExistingFiles(List<string> files, List<MovieFile> movieFiles, Movie movie)
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
