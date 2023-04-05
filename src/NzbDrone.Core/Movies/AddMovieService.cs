using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Tv;

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
        private readonly IProvideMovieInfo _movieInfo;
        private readonly IBuildFileNames _fileNameBuilder;
        private readonly IAddMovieValidator _addMovieValidator;
        private readonly Logger _logger;

        public AddMovieService(IMovieService movieService,
                                IProvideMovieInfo movieInfo,
                                IBuildFileNames fileNameBuilder,
                                IAddMovieValidator addMovieValidator,
                                Logger logger)
        {
            _movieService = movieService;
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
            _movieService.AddMovie(newMovie);

            return newMovie;
        }

        public List<Movie> AddMovies(List<Movie> newSeries, bool ignoreErrors = false)
        {
            var added = DateTime.UtcNow;
            var moviesToAdd = new List<Movie>();
            var existingSeries = _movieService.GetAllMovies();

            foreach (var s in newSeries)
            {
                if (s.Path.IsNullOrWhiteSpace())
                {
                    _logger.Info("Adding Movie {0} Root Folder Path: [{1}]", s, s.RootFolderPath);
                }
                else
                {
                    _logger.Info("Adding Movie {0} Path: [{1}]", s, s.Path);
                }

                try
                {
                    var movie = AddSkyhookData(s);
                    movie = SetPropertiesAndValidate(movie);
                    movie.Added = added;
                    if (existingSeries.Any(f => f.TmdbId == movie.TmdbId))
                    {
                        _logger.Debug("TMDB ID {0} was not added due to validation failure: Movie already exists in database", s.TmdbId);
                        continue;
                    }

                    if (moviesToAdd.Any(f => f.TmdbId == movie.TmdbId))
                    {
                        _logger.Debug("TMDB ID {0} was not added due to validation failure: Movie already exists on list", s.TmdbId);
                        continue;
                    }

                    var duplicateSlug = moviesToAdd.FirstOrDefault(f => f.TitleSlug == movie.TitleSlug);
                    if (duplicateSlug != null)
                    {
                        _logger.Debug("TMDB ID {0} was not added due to validation failure: Duplicate Slug {1} used by movie {2}", s.TmdbId, s.TitleSlug, duplicateSlug.TmdbId);
                        continue;
                    }

                    moviesToAdd.Add(movie);
                }
                catch (ValidationException ex)
                {
                    if (!ignoreErrors)
                    {
                        throw;
                    }

                    _logger.Debug("TMDB ID {0} was not added due to validation failures. {1}", s.TmdbId, ex.Message);
                }
            }

            return _movieService.AddMovies(moviesToAdd);
        }

        private Movie AddSkyhookData(Movie newMovie)
        {
            Movie movie;

            try
            {
                movie = _movieInfo.GetMovieInfo(newMovie.TmdbId);
            }
            catch (SeriesNotFoundException)
            {
                _logger.Error("TMDB ID {0} was not found, it may have been removed from TheMovieDb.  Path: {1}", newMovie.TmdbId, newMovie.Path);

                throw new ValidationException(new List<ValidationFailure>
                                              {
                                                  new ValidationFailure("TmdbId", $"A movie with this ID was not found. Path: {newMovie.Path}", newMovie.TmdbId)
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

            newMovie.CleanTitle = newMovie.Title.CleanSeriesTitle();
            newMovie.SortTitle = SeriesTitleNormalizer.Normalize(newMovie.Title, newMovie.TmdbId);
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
