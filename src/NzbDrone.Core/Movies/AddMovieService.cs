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
using NzbDrone.Core.ImportLists.ImportExclusions;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Movies.Commands;
using NzbDrone.Core.Movies.Studios;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser;
using NzbDrone.Core.RootFolders;

namespace NzbDrone.Core.Movies
{
    public interface IAddMovieService
    {
        Movie AddMovie(Movie newMovie);
        List<Movie> AddMovies(List<Movie> newMovies, bool ignoreErrors = false);
    }

    public class AddMovieService :
        IAddMovieService,
        IExecute<AddMoviesCommand>
    {
        private readonly IMovieService _movieService;
        private readonly IStudioService _studioService;
        private readonly IMovieMetadataService _movieMetadataService;
        private readonly IProvideMovieInfo _movieInfo;
        private readonly IBuildFileNames _fileNameBuilder;
        private readonly IAddMovieValidator _addMovieValidator;
        private readonly IImportExclusionsService _importExclusionService;
        private readonly IRootFolderService _rootFolderService;
        private readonly Logger _logger;

        public AddMovieService(IMovieService movieService,
                                IStudioService studioService,
                                IMovieMetadataService movieMetadataService,
                                IProvideMovieInfo movieInfo,
                                IBuildFileNames fileNameBuilder,
                                IAddMovieValidator addMovieValidator,
                                ImportExclusionsService importExclusionsService,
                                IRootFolderService rootFolderService,
                                Logger logger)
        {
            _movieService = movieService;
            _studioService = studioService;
            _movieMetadataService = movieMetadataService;
            _movieInfo = movieInfo;
            _fileNameBuilder = fileNameBuilder;
            _addMovieValidator = addMovieValidator;
            _importExclusionService = importExclusionsService;
            _rootFolderService = rootFolderService;
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

                    if (m.RootFolderPath == null)
                    {
                        var rootFolder = _rootFolderService.GetBestRootFolderPath(m.Path);
                        movie.RootFolderPath = rootFolder;
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

                    _logger.Error("Foreign ID {0} was not added due to validation failures. {1}", m.ForeignId, ex.Message);
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

                    _logger.Error("Foreign ID {0} was not added due to connection failures. {1}", m.ForeignId, ex.Message);
                }
                catch (Exception ex)
                {
                    if (!ignoreErrors)
                    {
                        throw;
                    }

                    _logger.Error("Foreign ID {0} was not added due to failures. {1}", m.ForeignId, ex.Message);
                }
            }

            try
            {
                _movieMetadataService.UpsertMany(moviesToAdd.Select(x => x.MovieMetadata.Value).ToList());
            }
            catch (Exception ex)
            {
                if (!ignoreErrors)
                {
                    throw;
                }

                _logger.Debug("Failures adding metadata.", ex.Message);
            }

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
                var source = string.IsNullOrEmpty(newMovie.ForeignId) ? "TMDb" : "StashDB";
                _logger.Error("{1} was not found, it may have been removed from {0}. Path: {2}", source, newMovie.ForeignId, newMovie.Path);

                throw new ValidationException(new List<ValidationFailure>
                                              {
                                                 new ValidationFailure(source, $"A movie with this ID was not found. Path: {newMovie.Path}", newMovie.ForeignId)
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

            // Check if previosly excluded
            var type = newMovie.MovieMetadata.Value.ItemType == ItemType.Scene ? ImportExclusionType.Scene : ImportExclusionType.Movie;
            if (!_importExclusionService.IsExcluded(newMovie.ForeignId, type))
            {
                var newExclusion = new ImportExclusion { ForeignId = newMovie.ForeignId, Type = type, MovieTitle = newMovie.Title, MovieYear = newMovie.Year };
                if (newMovie.MovieMetadata?.Value?.Studio != null)
                {
                    var stashId = newMovie.MovieMetadata.Value.Studio.ForeignIds.StashId;
                    var excludedStudio = _importExclusionService.IsExcluded(stashId, ImportExclusionType.Studio);
                    if (excludedStudio)
                    {
                        _importExclusionService.AddExclusion(newExclusion);
                        throw new ValidationException($"Studio: [{newMovie.MovieMetadata.Value.Studio.Title}] has been excluded");
                    }
                    else
                    {
                        var studio = _studioService.FindByForeignId(stashId);
                        if (studio?.AfterDate != null)
                        {
                            var dateTime = (DateTime)studio.AfterDate;
                            if (newMovie.MovieMetadata?.Value?.ReleaseDateUtc < dateTime)
                            {
                                _importExclusionService.AddExclusion(newExclusion);
                                throw new ValidationException($"Date: [{newMovie.MovieMetadata?.Value.ReleaseDate}] has been excluded before {dateTime.ToString("yyyy-MM-dd")}");
                            }
                        }
                    }
                }

                var performerForeignIds = newMovie.MovieMetadata.Value.Credits.Select(c => c.PerformerForeignId);
                var excludedItems = _importExclusionService.GetAllByType(ImportExclusionType.Performer);
                if (excludedItems != null)
                {
                    var excludedPerformers = excludedItems.Where(e => performerForeignIds.Contains(e.ForeignId)).ToList();
                    if (excludedPerformers.Any())
                    {
                        _importExclusionService.AddExclusion(newExclusion);
                        throw new ValidationException($"Performer: [{string.Join(",", excludedPerformers.Select(ep => ep.MovieTitle).ToList())}] has been excluded");
                    }
                }

                var tagNames = newMovie.MovieMetadata.Value.Genres;
                var excludedTags = _importExclusionService.GetAllByType(ImportExclusionType.Tag);
                var exclusions = excludedTags.Where(e => tagNames.Contains(e.MovieTitle, StringComparer.OrdinalIgnoreCase)).ToList();

                if (exclusions.Any())
                {
                    _importExclusionService.AddExclusion(newExclusion);
                    throw new ValidationException($"Tag(s): [{string.Join(",", exclusions.Select(et => et.MovieTitle).ToList())}] excluded");
                }
            }
            else
            {
                // Clean up exclusion on manual add
                var exclusion = _importExclusionService.GetByForeignId(newMovie.ForeignId);
                if (exclusion != null && exclusion.Type == type)
                {
                    _importExclusionService.RemoveExclusion(exclusion);
                }
            }

            return newMovie;
        }

        public void Execute(AddMoviesCommand message)
        {
            AddMovies(message.Movies);
        }
    }
}
