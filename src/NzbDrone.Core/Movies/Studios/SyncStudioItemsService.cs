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

namespace NzbDrone.Core.Movies.Studios
{
    public class SyncStudioItemsService : IExecute<SyncStudioItemsCommand>
    {
        private readonly IProvideMovieInfo _movieInfo;
        private readonly IStudioService _studioService;
        private readonly IMovieService _movieService;
        private readonly IAddMovieService _addMovieService;
        private readonly IConfigService _configService;
        private readonly IDiskScanService _diskScanService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IImportExclusionsService _importExclusionService;

        private readonly Logger _logger;

        public SyncStudioItemsService(IProvideMovieInfo movieInfo,
                                        IStudioService studioService,
                                        IMovieService movieService,
                                        IAddMovieService addMovieService,
                                        IConfigService configService,
                                        IDiskScanService diskScanService,
                                        IEventAggregator eventAggregator,
                                        IImportExclusionsService importExclusionsService,
                                        Logger logger)
        {
            _movieInfo = movieInfo;
            _studioService = studioService;
            _movieService = movieService;
            _addMovieService = addMovieService;
            _configService = configService;
            _diskScanService = diskScanService;
            _eventAggregator = eventAggregator;
            _importExclusionService = importExclusionsService;
            _logger = logger;
        }

        private void SyncStudioItems(Studio studio)
        {
            if (studio.Monitored)
            {
                var existingMovies = _movieService.AllMovieForeignIds();
                var studioScenes = _movieInfo.GetStudioScenes(studio.ForeignId);
                var excludedMovies = _importExclusionService.GetAllExclusions().Select(e => e.ForeignId);
                var moviesToAdd = studioScenes.Where(m => !existingMovies.Contains(m)).Where(m => !excludedMovies.Contains(m));

                if (moviesToAdd.Any())
                {
                    _addMovieService.AddMovies(moviesToAdd.Select(m => new Movie
                    {
                        ForeignId = m,
                        QualityProfileId = studio.QualityProfileId,
                        RootFolderPath = studio.RootFolderPath,
                        AddOptions = new AddMovieOptions
                        {
                            SearchForMovie = studio.SearchOnAdd,
                            AddMethod = AddMovieMethod.Studio
                        },
                        Monitored = true,
                        Tags = studio.Tags
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

        public void Execute(SyncStudioItemsCommand message)
        {
            if (message.StudioIds.Any())
            {
                foreach (var studioId in message.StudioIds)
                {
                    var studio = _studioService.GetById(studioId);
                    var items = _movieService.GetByStudioForeignId(studio.ForeignId);
                    var trigger = message.Trigger;
                    var isNew = false;

                    // Rescan before sync since syncing will add a new movie and scan automatically
                    foreach (var movieItem in items)
                    {
                        var movie = _movieService.GetMovie(movieItem.Id);
                        RescanMovie(movie, isNew, trigger);
                    }

                    SyncStudioItems(studio);
                }
            }
            else
            {
                var allStudios = _studioService.GetAllStudios().OrderBy(c => c.SortTitle).ToList();

                foreach (var studio in allStudios)
                {
                    try
                    {
                        SyncStudioItems(studio);
                    }
                    catch (MovieNotFoundException)
                    {
                        _logger.Error("Studio '{0}' (StashDb {1}) was not found, it may have been removed from The Movie Database.", studio.Title, studio.ForeignId);
                    }
                }
            }
        }
    }
}
