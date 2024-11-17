using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
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
using NzbDrone.Core.Movies.AlternativeTitles;
using NzbDrone.Core.Movies.Commands;
using NzbDrone.Core.Movies.Events;
using NzbDrone.Core.Movies.Performers;
using NzbDrone.Core.Movies.Studios;
using NzbDrone.Core.RootFolders;

namespace NzbDrone.Core.Movies
{
    public class RefreshMovieService : IExecute<RefreshMovieCommand>
    {
        private readonly IProvideMovieInfo _movieInfo;
        private readonly IMovieService _movieService;
        private readonly IMovieMetadataService _movieMetadataService;
        private readonly IRootFolderService _folderService;
        private readonly IDiskProvider _diskProvider;
        private readonly IBuildMoviePaths _buildMoviePaths;
        private readonly IAlternativeTitleService _titleService;
        private readonly IAddPerformerService _performerService;
        private readonly IAddStudioService _studioService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IDiskScanService _diskScanService;
        private readonly ICheckIfMovieShouldBeRefreshed _checkIfMovieShouldBeRefreshed;
        private readonly IConfigService _configService;
        private readonly IAutoTaggingService _autoTaggingService;
        private readonly Logger _logger;

        public RefreshMovieService(IProvideMovieInfo movieInfo,
                                    IMovieService movieService,
                                    IMovieMetadataService movieMetadataService,
                                    IRootFolderService folderService,
                                    IDiskProvider diskProvider,
                                    IBuildMoviePaths buildMoviePaths,
                                    IAlternativeTitleService titleService,
                                    IAddStudioService studioService,
                                    IAddPerformerService performerService,
                                    IEventAggregator eventAggregator,
                                    IDiskScanService diskScanService,
                                    ICheckIfMovieShouldBeRefreshed checkIfMovieShouldBeRefreshed,
                                    IConfigService configService,
                                    IAutoTaggingService autoTaggingService,
                                    Logger logger)
        {
            _movieInfo = movieInfo;
            _movieService = movieService;
            _movieMetadataService = movieMetadataService;
            _folderService = folderService;
            _diskProvider = diskProvider;
            _buildMoviePaths = buildMoviePaths;
            _titleService = titleService;
            _studioService = studioService;
            _performerService = performerService;
            _eventAggregator = eventAggregator;
            _diskScanService = diskScanService;
            _checkIfMovieShouldBeRefreshed = checkIfMovieShouldBeRefreshed;
            _configService = configService;
            _autoTaggingService = autoTaggingService;
            _logger = logger;
        }

        private Movie RefreshMovieInfo(int movieId)
        {
            // Get the movie before updating, that way any changes made to the movie after the refresh started,
            // but before this movie was refreshed won't be lost.
            var movie = _movieService.GetMovie(movieId);
            var movieMetadata = _movieMetadataService.Get(movie.MovieMetadataId);

            _logger.ProgressInfo("Updating info for {0}", movie.Title);

            MovieMetadata movieInfo;
            Studio studioInfo;
            List<Performer> performerInfo;

            try
            {
                var tupleInfo = movieMetadata.ItemType == ItemType.Movie ? _movieInfo.GetMovieInfo(movie.TmdbId) : _movieInfo.GetSceneInfo(movie.ForeignId);
                movieInfo = tupleInfo.Item1;
                studioInfo = tupleInfo.Item2;
                performerInfo = tupleInfo.Item3;
            }
            catch (MovieNotFoundException)
            {
                if (movieMetadata.Status != MovieStatusType.Deleted)
                {
                    movieMetadata.Status = MovieStatusType.Deleted;
                    _movieMetadataService.Upsert(movieMetadata);
                    _logger.Debug("Movie marked as deleted on TMDb for {0}", movie.Title);
                    _eventAggregator.PublishEvent(new MovieUpdatedEvent(movie));
                }

                throw;
            }

            if (movieMetadata.ForeignId != movieInfo.ForeignId)
            {
                _logger.Warn("Movie '{0}' (TMDb: {1}) was replaced with '{2}' (TMDb: {3}), because the original was a duplicate.", movie.Title, movie.ForeignId, movieInfo.Title, movieInfo.ForeignId);
                movieMetadata.ForeignId = movieInfo.ForeignId;
            }

            movieMetadata.Title = movieInfo.Title;
            movieMetadata.ImdbId = movieInfo.ImdbId;
            movieMetadata.Overview = movieInfo.Overview;
            movieMetadata.Status = movieInfo.Status;
            movieMetadata.Images = movieInfo.Images;
            movieMetadata.CleanTitle = movieInfo.CleanTitle;
            movieMetadata.SortTitle = movieInfo.SortTitle;
            movieMetadata.LastInfoSync = DateTime.UtcNow;
            movieMetadata.Runtime = movieInfo.Runtime;
            movieMetadata.Ratings = movieInfo.Ratings;
            movieMetadata.ItemType = movieInfo.ItemType;
            movieMetadata.MetadataSource = movieInfo.MetadataSource;
            movieMetadata.Credits = movieInfo.Credits;

            // movie.Genres = movieInfo.Genres;
            movieMetadata.Website = movieInfo.Website;

            movieMetadata.Year = movieInfo.Year;
            movieMetadata.ReleaseDateUtc = movieInfo.ReleaseDateUtc;
            movieMetadata.ReleaseDate = movieInfo.ReleaseDate;
            movieMetadata.StudioTitle = movieInfo.StudioTitle;
            movieMetadata.OriginalLanguage = movieInfo.OriginalLanguage;

            if (studioInfo != null && studioInfo.ForeignId.IsNotNullOrWhiteSpace())
            {
                var newCollection = _studioService.AddStudio(new Studio
                {
                    ForeignId = studioInfo.ForeignId,
                    Monitored = false,
                    SearchOnAdd = movie.AddOptions?.SearchForMovie ?? false,
                    QualityProfileId = movie.QualityProfileId,
                    RootFolderPath = _folderService.GetBestRootFolderPath(movie.Path),
                    Tags = movie.Tags
                }, true);

                if (newCollection != null)
                {
                    movieMetadata.StudioForeignId = newCollection.ForeignId;
                    movieMetadata.StudioTitle = newCollection.Title;
                }
            }
            else
            {
                movieMetadata.StudioForeignId = null;
                movieMetadata.StudioTitle = null;
            }

            performerInfo.ForEach(p =>
            {
                p.Monitored = false;
                p.RootFolderPath = _folderService.GetBestRootFolderPath(movie.Path);
                p.SearchOnAdd = movie.AddOptions?.SearchForMovie ?? false;
                p.QualityProfileId = movie.QualityProfileId;
                p.Tags = movie.Tags;
            });

            _performerService.AddPerformers(performerInfo, true);

            _movieMetadataService.Upsert(movieMetadata);

            movie.MovieMetadata = movieMetadata;

            var movieFolderExists = _diskProvider.FolderExists(movie.Path);
            if (!movie.HasFile && !movieFolderExists)
            {
                var path = movie.Path;
                movie.Path = _buildMoviePaths.BuildPath(movie, false);
                if (path != movie.Path)
                {
                    _logger.ProgressInfo("Updating path for {0} to {1}", movie.Title, movie.Path);
                    _movieService.UpdateMovie(movie);
                }
            }

            _logger.Debug("Finished movie metadata refresh for {0}", movieMetadata.Title);
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

        private void UpdateTags(Movie movie)
        {
            _logger.Trace("Updating tags for {0}", movie);

            var tagsAdded = new HashSet<int>();
            var tagsRemoved = new HashSet<int>();
            var changes = _autoTaggingService.GetTagChanges(movie);

            foreach (var tag in changes.TagsToRemove)
            {
                if (movie.Tags.Contains(tag))
                {
                    movie.Tags.Remove(tag);
                    tagsRemoved.Add(tag);
                }
            }

            foreach (var tag in changes.TagsToAdd)
            {
                if (!movie.Tags.Contains(tag))
                {
                    movie.Tags.Add(tag);
                    tagsAdded.Add(tag);
                }
            }

            if (tagsAdded.Any() || tagsRemoved.Any())
            {
                _movieService.UpdateMovie(movie);
                _logger.Debug("Updated tags for '{0}'. Added: {1}, Removed: {2}", movie.Title, tagsAdded.Count, tagsRemoved.Count);
            }
        }

        public void Execute(RefreshMovieCommand message)
        {
            var trigger = message.Trigger;
            var isNew = message.IsNewMovie;
            _eventAggregator.PublishEvent(new MovieRefreshStartingEvent(message.Trigger == CommandTrigger.Manual));

            if (message.MovieIds.Any())
            {
                foreach (var movieId in message.MovieIds)
                {
                    var movie = _movieService.GetMovie(movieId);

                    try
                    {
                        movie = RefreshMovieInfo(movieId);
                        UpdateTags(movie);
                        RescanMovie(movie, isNew, trigger);
                    }
                    catch (MovieNotFoundException)
                    {
                        _logger.Error("Movie '{0}' (TMDb {1}) was not found, it may have been removed from The Movie Database.", movie.Title, movie.ForeignId);
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
                var updatedMovies = new HashSet<string>();
                var updatedScenes = new HashSet<string>();

                if (message.LastStartTime.HasValue && message.LastStartTime.Value.AddDays(14) > DateTime.UtcNow)
                {
                    updatedMovies = _movieInfo.GetChangedMovies(message.LastStartTime.Value).Select(x => x.ToString()).ToHashSet();
                    updatedScenes = _movieInfo.GetChangedScenes(message.LastStartTime.Value);
                }

                var movieLists = _movieService.AllMovieIds().Chunk(1000);
                foreach (var movieList in movieLists)
                {
                    var allMovie = _movieService.GetMovies(movieList).ToList();

                    foreach (var movie in allMovie)
                    {
                        var movieLocal = movie;
                        if ((updatedMovies.Count == 0 && _checkIfMovieShouldBeRefreshed.ShouldRefresh(movie.MovieMetadata)) ||
                            updatedMovies.Contains(movie.ForeignId) ||
                            updatedScenes.Contains(movie.ForeignId) ||
                            message.Trigger == CommandTrigger.Manual)
                        {
                            try
                            {
                                movieLocal = RefreshMovieInfo(movieLocal.Id);
                            }
                            catch (MovieNotFoundException)
                            {
                                _logger.Error("Item '{0}' (ForeignId {1}) was not found, it may have been removed from The Movie Database.", movieLocal.Title, movieLocal.ForeignId);
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
                            _logger.Debug("Skipping refresh of movie: {0}", movieLocal.Title);
                            UpdateTags(movie);
                            RescanMovie(movieLocal, false, trigger);
                        }
                    }
                }
            }

            _eventAggregator.PublishEvent(new MovieRefreshCompleteEvent());
        }
    }
}
