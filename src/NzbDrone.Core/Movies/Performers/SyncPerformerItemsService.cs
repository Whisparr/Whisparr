using System;
using System.Linq;
using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.ImportLists.ImportExclusions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Movies.Commands;

namespace NzbDrone.Core.Movies.Performers
{
    public class SyncPerformerItemsService : IExecute<SyncPerformerItemsCommand>
    {
        private readonly IProvideMovieInfo _movieInfo;
        private readonly IPerformerService _performerService;
        private readonly IMovieService _movieService;
        private readonly IAddMovieService _addMovieService;
        private readonly IConfigService _configService;
        private readonly IDiskScanService _diskScanService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IImportExclusionsService _importExclusionService;

        private readonly RefreshMovieService _refreshMovieService;
        private readonly Logger _logger;

        public SyncPerformerItemsService(IProvideMovieInfo movieInfo,
                                        IPerformerService performerService,
                                        IMovieService movieService,
                                        IAddMovieService addMovieService,
                                        IConfigService configService,
                                        IDiskScanService diskScanService,
                                        IEventAggregator eventAggregator,
                                        RefreshMovieService refreshMovieService,
                                        ImportExclusionsService importExclusionsService,
                                        Logger logger)
        {
            _movieInfo = movieInfo;
            _performerService = performerService;
            _movieService = movieService;
            _addMovieService = addMovieService;
            _configService = configService;
            _diskScanService = diskScanService;
            _eventAggregator = eventAggregator;
            _refreshMovieService = refreshMovieService;
            _importExclusionService = importExclusionsService;
            _logger = logger;
        }

        private void SyncPerformerItems(Performer performer)
        {
            if (performer.Monitored)
            {
                var existingMovies = _movieService.AllMovieForeignIds();
                var performerScenes = _movieInfo.GetPerformerScenes(performer.ForeignId);
                var excludedMovies = _importExclusionService.GetAllExclusions().Select(e => e.ForeignId);
                var moviesToAdd = performerScenes.Where(m => !existingMovies.Contains(m)).Where(m => !excludedMovies.Contains(m));

                if (moviesToAdd.Any())
                {
                    _addMovieService.AddMovies(moviesToAdd.Select(m => new Movie
                    {
                        ForeignId = m,
                        QualityProfileId = performer.QualityProfileId,
                        RootFolderPath = performer.RootFolderPath,
                        AddOptions = new AddMovieOptions
                        {
                            SearchForMovie = performer.SearchOnAdd,
                            AddMethod = AddMovieMethod.Performer
                        },
                        Monitored = true,
                        Tags = performer.Tags
                    }).ToList(), true);
                }
            }
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
                _logger.Trace("Skipping rescan of {0}. Reason: Never rescan after refresh", movie);
                _eventAggregator.PublishEvent(new MovieScanSkippedEvent(movie, MovieScanSkippedReason.NeverRescanAfterRefresh));

                return;
            }
            else if (rescanAfterRefresh == RescanAfterRefreshType.AfterManual && trigger != CommandTrigger.Manual)
            {
                _logger.Trace("Skipping rescan of {0}. Reason: Not after automatic scans", movie);
                _eventAggregator.PublishEvent(new MovieScanSkippedEvent(movie, MovieScanSkippedReason.RescanAfterManualRefreshOnly));

                return;
            }

            try
            {
                _diskScanService.Scan(movie);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Couldn't rescan movie {0}", movie);
            }
        }

        public void Execute(SyncPerformerItemsCommand message)
        {
            if (message.PerformerIds.Any())
            {
                foreach (var performerId in message.PerformerIds)
                {
                    var performer = _performerService.GetById(performerId);
                    var items = _movieService.GetByPerformerForeignId(performer.ForeignId);
                    var trigger = message.Trigger;
                    var isNew = false;

                    // Rescan before sync since syncing will add a new movie and scan automatically
                    foreach (var movieItem in items)
                    {
                        var movie = _movieService.GetMovie(movieItem.Id);
                        RescanMovie(movie, isNew, trigger);
                    }

                    SyncPerformerItems(performer);
                }
            }
            else
            {
                var allPerformers = _performerService.GetAllPerformers().OrderBy(c => c.SortName).ToList();

                foreach (var performer in allPerformers)
                {
                    try
                    {
                        SyncPerformerItems(performer);
                    }
                    catch (MovieNotFoundException)
                    {
                        _logger.Error("Performer '{0}' (StashDb {1}) was not found, it may have been removed from The Movie Database.", performer.Name, performer.ForeignId);
                    }
                }
            }
        }
    }
}
