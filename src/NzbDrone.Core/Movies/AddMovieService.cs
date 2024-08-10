using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.Http;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.Movies
{
    public interface IAddMovieService
    {
        Movie AddMovie(Movie newMovie);
        List<Movie> AddMovies(List<Movie> newMovies, bool ignoreErrors = false);
    }

    public class AddMovieService : IAddMovieService
    {
        private readonly IMovieService _movieService;
        private readonly IMovieMetadataService _movieMetadataService;
        private readonly IProvideMovieInfo _movieInfo;
        private readonly IBuildFileNames _fileNameBuilder;
        private readonly IAddMovieValidator _addMovieValidator;
        private readonly Logger _logger;

        public AddMovieService(IMovieService movieService,
                                IMovieMetadataService movieMetadataService,
                                IProvideMovieInfo movieInfo,
                                IBuildFileNames fileNameBuilder,
                                IAddMovieValidator addMovieValidator,
                                Logger logger)
        {
            _movieService = movieService;
            _movieMetadataService = movieMetadataService;
            _movieInfo = movieInfo;
            _fileNameBuilder = fileNameBuilder;
            _addMovieValidator = addMovieValidator;
            _logger = logger;
        }

        public Movie AddMovie(Movie newMovie)
        {
            Ensure.That(newMovie, () => newMovie).IsNotNull();

            newMovie = AddSkyhookData(newMovie);
            newMovie = SetPropertiesAndValidate(newMovie);

            _logger.Info("Adding Movie {0} Path: [{1}]", newMovie, newMovie.Path);

            _movieMetadataService.Upsert(newMovie.MovieMetadata.Value);
            newMovie.MovieMetadataId = newMovie.MovieMetadata.Value.Id;

            _movieService.AddMovie(newMovie);

            return newMovie;
        }

        public List<Movie> AddMovies(List<Movie> newMovies, bool ignoreErrors = false)
        {
            var httpExceptionCount = 0;
            var added = DateTime.UtcNow;
            var moviesToAdd = new List<Movie>();
            var existingMovieForeignIds = _movieService.AllMovieForeignIds();

            foreach (var m in newMovies)
            {
                _logger.Info("Adding Movie {0} Path: [{1}]", m, m.Path);

                try
                {
                    var movie = AddSkyhookData(m);
                    movie = SetPropertiesAndValidate(movie);

                    movie.Added = added;

                    if (existingMovieForeignIds.Any(f => f == movie.ForeignId))
                    {
                        _logger.Debug("Foreign ID {0} was not added due to validation failure: Movie already exists in database", m.ForeignId);
                        continue;
                    }

                    if (moviesToAdd.Any(f => f.ForeignId == movie.ForeignId))
                    {
                        _logger.Debug("Foreign ID {0} was not added due to validation failure: Movie already exists on list", m.ForeignId);
                        continue;
                    }

                    moviesToAdd.Add(movie);
                    httpExceptionCount = 0;
                }
                catch (ValidationException ex)
                {
                    if (!ignoreErrors)
                    {
                        throw;
                    }

                    _logger.Debug("Foreign ID {0} was not added due to validation failures. {1}", m.ForeignId, ex.Message);
                }
                catch (HttpException ex)
                {
                    if (!ignoreErrors)
                    {
                        throw;
                    }

                    httpExceptionCount++;

                    // Throw exception on the two successive exception
                    if (httpExceptionCount > 2)
                    {
                        throw;
                    }

                    _logger.Debug("Foreign ID {0} was not added due to connection failures. {1}", m.ForeignId, ex.Message);
                }
            }

            _movieMetadataService.UpsertMany(moviesToAdd.Select(x => x.MovieMetadata.Value).ToList());
            moviesToAdd.ForEach(x => x.MovieMetadataId = x.MovieMetadata.Value.Id);

            return _movieService.AddMovies(moviesToAdd);
        }

        private Movie AddSkyhookData(Movie newMovie)
        {
            var movie = new Movie();

            try
            {
                movie.MovieMetadata = int.TryParse(newMovie.ForeignId, out var tmdbId) ? _movieInfo.GetMovieInfo(tmdbId).Item1 : _movieInfo.GetSceneInfo(newMovie.ForeignId).Item1;
            }
            catch (MovieNotFoundException)
            {
                _logger.Error("TmdbId {0} was not found, it may have been removed from TMDb. Path: {1}", newMovie.ForeignId, newMovie.Path);

                throw new ValidationException(new List<ValidationFailure>
                                              {
                                                  new ValidationFailure("TmdbId", $"A movie with this ID was not found. Path: {newMovie.Path}", newMovie.ForeignId)
                                              });
            }

            movie.ApplyChanges(newMovie);

            return movie;
        }

        private Movie SetPropertiesAndValidate(Movie newMovie)
        {
            if (string.IsNullOrWhiteSpace(newMovie.Path))
            {
                var folderName = _fileNameBuilder.GetMovieFolder(newMovie);
                newMovie.Path = Path.Combine(newMovie.RootFolderPath, folderName);
            }

            newMovie.MovieMetadata.Value.CleanTitle = newMovie.Title.CleanMovieTitle();
            newMovie.MovieMetadata.Value.SortTitle = MovieTitleNormalizer.Normalize(newMovie.Title, newMovie.ForeignId);
            newMovie.Added = DateTime.UtcNow;

            var validationResult = _addMovieValidator.Validate(newMovie);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            return newMovie;
        }
    }
}
