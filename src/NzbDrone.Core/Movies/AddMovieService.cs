using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser;
using NzbDrone.Core.RootFolders;

namespace NzbDrone.Core.Movies
{
    public interface IAddMovieService
    {
        Media AddMovie(Media newMovie);
        List<Media> AddMovies(List<Media> newMovies, bool ignoreErrors = false);
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

        public Media AddMovie(Media newMovie)
        {
            Ensure.That(newMovie, () => newMovie).IsNotNull();

            newMovie = AddSkyhookData(newMovie);
            newMovie = SetPropertiesAndValidate(newMovie);

            _logger.Info("Adding Movie {0} Path: [{1}]", newMovie, newMovie.Path);

            _movieMetadataService.Upsert(newMovie.MediaMetadata.Value);
            newMovie.MovieMetadataId = newMovie.MediaMetadata.Value.Id;

            _movieService.AddMovie(newMovie);

            return newMovie;
        }

        public List<Media> AddMovies(List<Media> newMovies, bool ignoreErrors = false)
        {
            var added = DateTime.UtcNow;
            var moviesToAdd = new List<Media>();

            foreach (var m in newMovies)
            {
                _logger.Info("Adding Movie {0} Path: [{1}]", m, m.Path);

                try
                {
                    var movie = AddSkyhookData(m);
                    movie = SetPropertiesAndValidate(movie);

                    movie.Added = added;
                    moviesToAdd.Add(movie);
                }
                catch (ValidationException ex)
                {
                    if (!ignoreErrors)
                    {
                        throw;
                    }

                    _logger.Debug("TmdbId {0} was not added due to validation failures. {1}", m.ForiegnId, ex.Message);
                }
            }

            _movieMetadataService.UpsertMany(moviesToAdd.Select(x => x.MediaMetadata.Value).ToList());
            moviesToAdd.ForEach(x => x.MovieMetadataId = x.MediaMetadata.Value.Id);

            return _movieService.AddMovies(moviesToAdd);
        }

        private Media AddSkyhookData(Media newMovie)
        {
            var movie = new Media();

            try
            {
                movie.MediaMetadata = _movieInfo.GetMovieInfo(newMovie.ForiegnId).Item1;
            }
            catch (MovieNotFoundException)
            {
                _logger.Error("TmdbId {0} was not found, it may have been removed from TMDb. Path: {1}", newMovie.ForiegnId, newMovie.Path);

                throw new ValidationException(new List<ValidationFailure>
                                              {
                                                  new ValidationFailure("TmdbId", $"A movie with this ID was not found. Path: {newMovie.Path}", newMovie.ForiegnId)
                                              });
            }

            movie.ApplyChanges(newMovie);

            return movie;
        }

        private Media SetPropertiesAndValidate(Media newMovie)
        {
            if (string.IsNullOrWhiteSpace(newMovie.Path))
            {
                var folderName = _fileNameBuilder.GetMovieFolder(newMovie);
                newMovie.Path = Path.Combine(newMovie.RootFolderPath, folderName);
            }

            newMovie.MediaMetadata.Value.CleanTitle = newMovie.Title.CleanMovieTitle();
            newMovie.MediaMetadata.Value.SortTitle = MovieTitleNormalizer.Normalize(newMovie.Title, newMovie.ForiegnId);
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
