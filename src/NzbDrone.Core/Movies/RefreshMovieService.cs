using System;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.AutoTagging;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Movies.Commands;
using NzbDrone.Core.Movies.Events;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.Movies
{
    public class RefreshMovieService : IExecute<RefreshMovieCommand>
    {
        private readonly IProvideMovieInfo _movieInfo;
        private readonly IMovieService _movieService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IDiskScanService _diskScanService;
        private readonly ICheckIfMovieShouldBeRefreshed _checkIfSeriesShouldBeRefreshed;
        private readonly IConfigService _configService;
        private readonly IAutoTaggingService _autoTaggingService;
        private readonly Logger _logger;

        public RefreshMovieService(IProvideMovieInfo movieInfo,
                                   IMovieService movieService,
                                   IEventAggregator eventAggregator,
                                   IDiskScanService diskScanService,
                                   ICheckIfMovieShouldBeRefreshed checkIfSeriesShouldBeRefreshed,
                                   IConfigService configService,
                                   IAutoTaggingService autoTaggingService,
                                   Logger logger)
        {
            _movieInfo = movieInfo;
            _movieService = movieService;
            _eventAggregator = eventAggregator;
            _diskScanService = diskScanService;
            _checkIfSeriesShouldBeRefreshed = checkIfSeriesShouldBeRefreshed;
            _configService = configService;
            _autoTaggingService = autoTaggingService;
            _logger = logger;
        }

        private Movie RefreshSeriesInfo(int seriesId)
        {
            // Get the series before updating, that way any changes made to the series after the refresh started,
            // but before this series was refreshed won't be lost.
            var movie = _movieService.GetMovie(seriesId);

            _logger.ProgressInfo("Updating {0}", movie.Title);

            Movie movieInfo;

            try
            {
                movieInfo = _movieInfo.GetMovieInfo(movie.TmdbId);
            }
            catch (SeriesNotFoundException)
            {
                if (movie.Status != MovieStatusType.Deleted)
                {
                    movie.Status = MovieStatusType.Deleted;
                    _movieService.UpdateMovie(movie, publishUpdatedEvent: false);
                    _logger.Debug("Series marked as deleted on tvdb for {0}", movie.Title);
                    _eventAggregator.PublishEvent(new MovieUpdatedEvent(movie));
                }

                throw;
            }

            if (movie.TmdbId != movieInfo.TmdbId)
            {
                _logger.Warn("Movie '{0}' (tmdbId {1}) was replaced with '{2}' (tmdbId {3}), because the original was a duplicate.", movie.Title, movie.TmdbId, movieInfo.Title, movieInfo.TmdbId);
                movie.TmdbId = movieInfo.TmdbId;
            }

            movie.Title = movieInfo.Title;
            movie.Year = movieInfo.Year;
            movie.TitleSlug = movieInfo.TitleSlug;
            movie.Overview = movieInfo.Overview;
            movie.OriginalLanguage = movieInfo.OriginalLanguage;
            movie.Status = movieInfo.Status;
            movie.CleanTitle = movieInfo.CleanTitle;
            movie.SortTitle = movieInfo.SortTitle;
            movie.LastInfoSync = DateTime.UtcNow;
            movie.Runtime = movieInfo.Runtime;
            movie.Images = movieInfo.Images;
            movie.Studio = movieInfo.Studio;
            movie.Ratings = movieInfo.Ratings;
            movie.Genres = movieInfo.Genres;

            try
            {
                movie.Path = new DirectoryInfo(movie.Path).FullName;
                movie.Path = movie.Path.GetActualCasing();
            }
            catch (Exception e)
            {
                _logger.Warn(e, "Couldn't update movie path for " + movie.Path);
            }

            _movieService.UpdateMovie(movie, publishUpdatedEvent: false);

            _logger.Debug("Finished series refresh for {0}", movie.Title);
            _eventAggregator.PublishEvent(new MovieUpdatedEvent(movie));

            return movie;
        }

        private void RescanMovie(Movie movie, bool isNew, CommandTrigger trigger)
        {
            var rescanAfterRefresh = _configService.RescanAfterRefresh;

            if (isNew)
            {
                _logger.Trace("Forcing rescan of {0}. Reason: New movie", movie);
            }
            else if (rescanAfterRefresh == RescanAfterRefreshType.Never)
            {
                _logger.Trace("Skipping rescan of {0}. Reason: never rescan after refresh", movie);
                _eventAggregator.PublishEvent(new MovieScanSkippedEvent(movie, EntityScanSkippedReason.NeverRescanAfterRefresh));

                return;
            }
            else if (rescanAfterRefresh == RescanAfterRefreshType.AfterManual && trigger != CommandTrigger.Manual)
            {
                _logger.Trace("Skipping rescan of {0}. Reason: not after automatic scans", movie);
                _eventAggregator.PublishEvent(new MovieScanSkippedEvent(movie, EntityScanSkippedReason.RescanAfterManualRefreshOnly));

                return;
            }

            try
            {
                _diskScanService.Scan(movie);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Couldn't rescan series {0}", movie);
            }
        }

        private void UpdateTags(Movie movie)
        {
            // TODO: Add AutoTagging for Movies
            _logger.Trace("Updating tags for {0}", movie);
        }

        public void Execute(RefreshMovieCommand message)
        {
            var trigger = message.Trigger;
            var isNew = message.IsNewMovie;
            _eventAggregator.PublishEvent(new SeriesRefreshStartingEvent(trigger == CommandTrigger.Manual));

            if (message.MovieIds.Any())
            {
                foreach (var movieId in message.MovieIds)
                {
                    var movie = _movieService.GetMovie(movieId);

                    try
                    {
                        movie = RefreshSeriesInfo(movieId);
                        UpdateTags(movie);
                        RescanMovie(movie, isNew, trigger);
                    }
                    catch (SeriesNotFoundException)
                    {
                        _logger.Error("Series '{0}' (tvdbid {1}) was not found, it may have been removed from TheTVDB.", movie.Title, movie.TmdbId);
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "Couldn't refresh info for {0}", movie);
                        UpdateTags(movie);
                        RescanMovie(movie, isNew, trigger);
                        throw;
                    }
                }
            }
            else
            {
                var allMovies = _movieService.GetAllMovies().OrderBy(c => c.SortTitle).ToList();

                foreach (var movie in allMovies)
                {
                    var movieLocal = movie;
                    if (trigger == CommandTrigger.Manual || _checkIfSeriesShouldBeRefreshed.ShouldRefresh(movieLocal))
                    {
                        try
                        {
                            movieLocal = RefreshSeriesInfo(movieLocal.Id);
                        }
                        catch (SeriesNotFoundException)
                        {
                            _logger.Error("Movie '{0}' (tmdbId {1}) was not found, it may have been removed from TheTVDB.", movieLocal.Title, movieLocal.TmdbId);
                            continue;
                        }
                        catch (Exception e)
                        {
                            _logger.Error(e, "Couldn't refresh info for {0}", movieLocal);
                        }

                        UpdateTags(movie);
                        RescanMovie(movieLocal, false, trigger);
                    }
                    else
                    {
                        _logger.Info("Skipping refresh of movie: {0}", movieLocal.Title);
                        UpdateTags(movie);
                        RescanMovie(movieLocal, false, trigger);
                    }
                }
            }

            _eventAggregator.PublishEvent(new SeriesRefreshCompleteEvent());
        }
    }
}
