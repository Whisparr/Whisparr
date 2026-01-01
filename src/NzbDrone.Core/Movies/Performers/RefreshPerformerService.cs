using System;
using System.Collections.Generic;
using System.Linq;
using DryIoc.ImTools;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.ImportLists.ImportExclusions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Movies.Performers.Commands;
using NzbDrone.Core.Movies.Performers.Events;

namespace NzbDrone.Core.Movies.Performers
{
    public class RefreshPerformerService : IExecute<RefreshPerformersCommand>
    {
        private readonly IProvideMovieInfo _movieInfo;
        private readonly IPerformerService _performerService;
        private readonly IAddPerformerService _addPerformerService;
        private readonly IMovieService _movieService;
        private readonly IAddMovieService _addMovieService;
        private readonly IConfigService _configService;
        private readonly IDiskScanService _diskScanService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IImportListExclusionService _importListExclusionService;

        private readonly Logger _logger;

        public RefreshPerformerService(IProvideMovieInfo movieInfo,
                                        IAddPerformerService addPerformerService,
                                        IPerformerService performerService,
                                        IMovieService movieService,
                                        IAddMovieService addMovieService,
                                        IConfigService configService,
                                        IDiskScanService diskScanService,
                                        IEventAggregator eventAggregator,
                                        ImportListExclusionService importListExclusionsService,
                                        Logger logger)
        {
            _movieInfo = movieInfo;
            _performerService = performerService;
            _addPerformerService = addPerformerService;
            _movieService = movieService;
            _addMovieService = addMovieService;
            _configService = configService;
            _diskScanService = diskScanService;
            _eventAggregator = eventAggregator;
            _importListExclusionService = importListExclusionsService;
            _logger = logger;
        }

        private Performer RefreshPerformerInfo(int performerId)
        {
            // Get the movie before updating, that way any changes made to the movie after the refresh started,
            // but before this movie was refreshed won't be lost.
            var performer = _performerService.GetById(performerId);

            _logger.ProgressInfo("Updating info for {0}", performer.Name);

            Performer performerInfo;

            try
            {
                performerInfo = _movieInfo.GetPerformerInfo(performer.ForeignId);
            }
            catch (MovieNotFoundException)
            {
                if (performer.Status != PerformerStatus.Deleted)
                {
                    performer.Status = PerformerStatus.Deleted;
                    _performerService.Update(performer);
                    _logger.Debug("Performer not found on StashDB for {0}", performer);
                    _eventAggregator.PublishEvent(new PerformerUpdatedEvent(performer));
                }

                throw;
            }

            if (performer.ForeignId != performerInfo.ForeignId)
            {
                _logger.Warn("Performer '{0}' (StashDb: {1}) was replaced with '{2}' (StashDb: {3}), because the original was a duplicate.", performer.Name, performer.ForeignId, performer.Name, performer.ForeignId);
                performer.ForeignId = performerInfo.ForeignId;
            }

            performer.TpdbId = performerInfo.TpdbId;
            performer.Age = performerInfo.Age;
            performer.CareerEnd = performerInfo.CareerEnd;
            performer.CareerStart = performerInfo.CareerStart;
            performer.CleanName = performerInfo.CleanName;
            performer.Ethnicity = performerInfo.Ethnicity;
            performer.Gender = performerInfo.Gender;
            performer.HairColor = performerInfo.HairColor;
            performer.Images = performerInfo.Images;
            performer.LastInfoSync = DateTime.UtcNow;
            performer.Name = performerInfo.Name;
            performer.SortName = performerInfo.SortName;
            performer.Status = performerInfo.Status;

            if (performerInfo.MergedIntoId.IsNotNullOrWhiteSpace())
            {
                var mergedPerformer = _performerService.FindByForeignId(performerInfo.MergedIntoId);

                if (mergedPerformer == null)
                {
                    _logger.Info("Performer '{0}' (StashId {1}) was merged into performer (StashId {2}) which is not in the system. Adding performer.", performer.Name, performer.ForeignId, performerInfo.MergedIntoId);

                    var newPerformer = new Performer
                    {
                        ForeignId = performerInfo.MergedIntoId,
                        Monitored = performer.Monitored,
                        QualityProfileId = performer.QualityProfileId,
                        RootFolderPath = performer.RootFolderPath,
                        SearchOnAdd = performer.SearchOnAdd,
                        Tags = performer.Tags
                    };

                    _addPerformerService.AddPerformer(newPerformer, true);
                }
            }

            _performerService.Update(performer);

            _logger.Debug("Finished performer metadata refresh for {0}", performer.Name);
            _eventAggregator.PublishEvent(new PerformerUpdatedEvent(performer));

            return performer;
        }

        private void SyncPerformerItems(Performer performer)
        {
            if (!performer.Monitored && !performer.MoviesMonitored)
            {
                return;
            }

            // Chunk the into smaller lists
            var chunkSize = 10;
            var performerWork = _movieInfo.GetPerformerWorks(performer.ForeignId);

            if (performer.Monitored)
            {
                var existingScenes = _movieService.AllMovieStashIds();
                var excludedScenes = _importListExclusionService.GetAllExclusions().Select(e => e.ForeignId);
                var scenesToAdd = performerWork.Scenes.Where(m => !existingScenes.Contains(m)).Where(m => !excludedScenes.Contains(m));
                var scenesAdded = 0;

                if (scenesToAdd.Any())
                {
                    var sceneLists = scenesToAdd.Select(m => new Movie
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
                    }).Chunk(chunkSize);

                    foreach (var sceneList in sceneLists)
                    {
                        scenesAdded += _addMovieService.AddMovies(sceneList.ToList(), true).Count;
                    }
                }

                _logger.Info("Synced performer {0} has {1} scenes adding {2} and added {3}", performer.Name, performerWork.Scenes.Count, scenesToAdd.Count(), scenesAdded);
            }

            if (performer.MoviesMonitored)
            {
                var moviesAdded = 0;

                if (_configService.WhisparrMovieMetadataSource == MovieMetadataType.TMDB)
                {
                    var tmbdId = 0;
                    var existingMovies = _movieService.AllMovieTmdbIds();
                    var excludedMovies = _importListExclusionService.GetAllExclusions().Select(e => int.TryParse(e.ForeignId, out tmbdId)).Select(e => tmbdId).Where(e => e != 0).ToList();
                    var moviesToAdd = performerWork.Movies.Where(m => !existingMovies.Contains(m)).Where(m => !excludedMovies.Contains(m));

                    if (moviesToAdd.Any())
                    {
                        var movieLists = moviesToAdd.Select(m => new Movie
                        {
                            ForeignId = m.ToString(),
                            TmdbId = m,
                            QualityProfileId = performer.QualityProfileId,
                            RootFolderPath = performer.RootFolderPath,
                            AddOptions = new AddMovieOptions
                            {
                                SearchForMovie = performer.SearchOnAdd,
                                AddMethod = AddMovieMethod.Performer
                            },
                            Monitored = true,
                            Tags = performer.Tags
                        }).Chunk(chunkSize);

                        foreach (var movieList in movieLists)
                        {
                            moviesAdded += _addMovieService.AddMovies(movieList.ToList(), true).Count;
                        }
                    }

                    _logger.Info("Synced performer {0} has {1} movies adding {2} and added {3}", performer.Name, performerWork.Movies.Count, moviesToAdd.Count(), moviesAdded);
                }

                if (_configService.WhisparrMovieMetadataSource == MovieMetadataType.TPDB)
                {
                    var existingMovies = _movieService.AllMovieTpdbIds();
                    var excludedMovies = _importListExclusionService.GetAllExclusions().Where(e => e.Type == ImportExclusionType.Movie).Select(e => e.ForeignId).ToList();
                    var moviesToAdd = performerWork.TpdbMovies.Where(m => !existingMovies.Contains(m)).Where(m => !excludedMovies.Contains(m));

                    if (moviesToAdd.Any())
                    {
                        var movieLists = moviesToAdd.Select(m => new Movie
                        {
                            ForeignId = $"tpdbid:{m}",
                            TpdbId = m,
                            QualityProfileId = performer.QualityProfileId,
                            RootFolderPath = performer.RootFolderPath,
                            AddOptions = new AddMovieOptions
                            {
                                SearchForMovie = performer.SearchOnAdd,
                                AddMethod = AddMovieMethod.Performer
                            },
                            Monitored = true,
                            Tags = performer.Tags
                        }).Chunk(chunkSize);

                        foreach (var movieList in movieLists)
                        {
                            moviesAdded += _addMovieService.AddMovies(movieList.ToList(), true).Count;
                        }
                    }

                    _logger.Info("Synced performer {0} has {1} movies adding {2} and added {3}", performer.Name, performerWork.TpdbMovies.Count, moviesToAdd.Count(), moviesAdded);
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
                _diskScanService.Scan(movie, trigger == CommandTrigger.Manual);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Couldn't rescan movie {0}", movie);
            }
        }

        public void Execute(RefreshPerformersCommand message)
        {
            if (message.PerformerIds.Any())
            {
                foreach (var performerId in message.PerformerIds)
                {
                    var performer = _performerService.GetById(performerId);
                    var items = _movieService.GetByPerformerForeignId(performer.ForeignId);
                    var trigger = message.Trigger;
                    var isNew = false;

                    try
                    {
                        performer = RefreshPerformerInfo(performerId);
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "Couldn't refresh info for {0}", performer.Name);
                    }

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
                var allPerformers = _performerService.GetAllPerformers().OrderBy(c => c.LastInfoSync).ToList();

                var updatePerformers = new HashSet<string>();

                if (message.LastStartTime.HasValue && message.LastStartTime.Value.AddDays(14) > DateTime.UtcNow)
                {
                    updatePerformers = _movieInfo.GetChangedPerformers(message.LastStartTime.Value);
                }

                foreach (var performer in allPerformers)
                {
                    var performerLocal = performer;

                    try
                    {
                        if ((updatePerformers.Count == 0 && performer.LastInfoSync < DateTime.UtcNow.AddDays(-14)) ||
                            updatePerformers.Contains(performer.ForeignId) ||
                            message.Trigger == CommandTrigger.Manual)
                        {
                            performerLocal = RefreshPerformerInfo(performerLocal.Id);
                        }

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
