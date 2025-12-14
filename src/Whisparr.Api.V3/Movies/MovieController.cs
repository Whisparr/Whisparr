using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DryIoc.ImTools;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Commands;
using NzbDrone.Core.Movies.Events;
using NzbDrone.Core.MovieStats;
using NzbDrone.Core.Parser;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.Validation;
using NzbDrone.Core.Validation.Paths;
using NzbDrone.SignalR;
using Whisparr.Http;
using Whisparr.Http.REST;
using Whisparr.Http.REST.Attributes;

namespace Whisparr.Api.V3.Movies
{
    [V3ApiController]
    public class MovieController : RestControllerWithSignalR<MovieResource, Movie>,
                                IHandle<MovieFileImportedEvent>,
                                IHandle<MovieFileDeletedEvent>,
                                IHandle<MovieUpdatedEvent>,
                                IHandle<MovieEditedEvent>,
                                IHandle<MoviesDeletedEvent>,
                                IHandle<MovieRenamedEvent>,
                                IHandle<MediaCoversUpdatedEvent>
    {
        private readonly IMovieService _moviesService;
        private readonly IAddMovieService _addMovieService;
        private readonly IMovieStatisticsService _movieStatisticsService;
        private readonly IMapCoversToLocal _coverMapper;
        private readonly IManageCommandQueue _commandQueueManager;
        private readonly IRootFolderService _rootFolderService;
        private readonly IUpgradableSpecification _qualityUpgradableSpecification;
        private readonly IConfigService _configService;
        private readonly bool _useCache;
        private readonly ICached<MovieResource> _movieResourcesCache;
        private readonly Logger _logger;

        public MovieController(IBroadcastSignalRMessage signalRBroadcaster,
                           IMovieService moviesService,
                           IAddMovieService addMovieService,
                           IMovieStatisticsService movieStatisticsService,
                           IMapCoversToLocal coverMapper,
                           IManageCommandQueue commandQueueManager,
                           IRootFolderService rootFolderService,
                           IUpgradableSpecification qualityUpgradableSpecification,
                           IConfigService configService,
                           RootFolderValidator rootFolderValidator,
                           MappedNetworkDriveValidator mappedNetworkDriveValidator,
                           MoviePathValidator moviesPathValidator,
                           MovieExistsValidator moviesExistsValidator,
                           MovieAncestorValidator moviesAncestorValidator,
                           RecycleBinValidator recycleBinValidator,
                           SystemFolderValidator systemFolderValidator,
                           QualityProfileExistsValidator qualityProfileExistsValidator,
                           RootFolderExistsValidator rootFolderExistsValidator,
                           MovieFolderAsRootFolderValidator movieFolderAsRootFolderValidator,
                           ICacheManager cacheManager,
                           Logger logger)
            : base(signalRBroadcaster)
        {
            _moviesService = moviesService;
            _addMovieService = addMovieService;
            _movieStatisticsService = movieStatisticsService;
            _qualityUpgradableSpecification = qualityUpgradableSpecification;
            _configService = configService;
            _coverMapper = coverMapper;
            _commandQueueManager = commandQueueManager;
            _useCache = _configService.WhisparrCacheMovieAPI;
            _rootFolderService = rootFolderService;
            _logger = logger;
            _movieResourcesCache = cacheManager.GetCache<MovieResource>(typeof(MovieResource), "movieResources");

            SharedValidator.RuleFor(s => s.Path).Cascade(CascadeMode.Stop)
                .IsValidPath()
                .SetValidator(rootFolderValidator)
                .SetValidator(mappedNetworkDriveValidator)
                .SetValidator(moviesPathValidator)
                .SetValidator(moviesAncestorValidator)
                .SetValidator(recycleBinValidator)
                .SetValidator(systemFolderValidator)
                .When(s => s.Path.IsNotNullOrWhiteSpace());

            PostValidator.RuleFor(s => s.Path).Cascade(CascadeMode.Stop)
                .NotEmpty()
                .IsValidPath()
                .When(s => s.RootFolderPath.IsNullOrWhiteSpace());
            PostValidator.RuleFor(s => s.RootFolderPath).Cascade(CascadeMode.Stop)
                .NotEmpty()
                .IsValidPath()
                .SetValidator(rootFolderExistsValidator)
                .SetValidator(movieFolderAsRootFolderValidator)
                .When(s => s.Path.IsNullOrWhiteSpace());

            PutValidator.RuleFor(s => s.Path).Cascade(CascadeMode.Stop)
                .NotEmpty()
                .IsValidPath();

            SharedValidator.RuleFor(s => s.QualityProfileId).Cascade(CascadeMode.Stop)
                .ValidId()
                .SetValidator(qualityProfileExistsValidator);

            PostValidator.RuleFor(s => s.Title).NotEmpty().When(s => s.TmdbId <= 0);
            PostValidator.RuleFor(s => s.ForeignId).NotNull().NotEmpty().SetValidator(moviesExistsValidator);

            PutValidator.RuleFor(s => s.Path).IsValidPath();
        }

        // Basic search: cleanTitle or foreign ID
        // Added for SelectMovieModalContent performance but will reuse elsewhere
        [HttpGet("search")]
        [Produces("application/json")]
        public List<MovieResource> SearchMovies(string query)
        {
            var moviesResources = new List<MovieResource>();

            if (query.IsNullOrWhiteSpace())
            {
                return moviesResources;
            }

            // Try cache first
            if (_useCache)
            {
                var cleanTitle = query.CleanMovieTitle();
                var ids = _moviesService.AllMovieIds();
                moviesResources = GetMovieResources(ids).Where(m =>
                    (!string.IsNullOrEmpty(m.CleanTitle) && m.CleanTitle.Contains(cleanTitle, StringComparison.OrdinalIgnoreCase)) ||
                    m.ForeignId == query)
                .Take(100)
                .ToList();

                return moviesResources;
            }

            // cache not used, do normal search

            var availDelay = _configService.AvailabilityDelay;
            var movieStats = _movieStatisticsService.MovieStatistics();
            var sdict = movieStats.ToDictionary(x => x.MovieId);

            var movies = _moviesService.SearchMovies(query).Take(100).ToList();

            foreach (var movie in movies)
            {
                moviesResources.AddIfNotNull(movie.ToResource(availDelay, _qualityUpgradableSpecification));
            }

            LinkMovieStatistics(moviesResources, sdict);

            return moviesResources;
        }

        [HttpGet]
        public List<MovieResource> AllMovie(int? tmdbId, string stashId, bool excludeLocalCovers = false)
        {
            var moviesResources = new List<MovieResource>();

            Dictionary<string, FileInfo> coverFileInfos = null;

            if (tmdbId.HasValue)
            {
                var movie = _moviesService.FindByTmdbId(tmdbId.Value);

                if (movie != null)
                {
                    moviesResources.AddIfNotNull(MapToResource(movie));
                }
            }
            else if (stashId.IsNotNullOrWhiteSpace())
            {
                var movie = _moviesService.FindByForeignId(stashId);

                if (movie != null)
                {
                    moviesResources.AddIfNotNull(MapToResource(movie));
                }
            }
            else
            {
                var movieStats = _movieStatisticsService.MovieStatistics();
                var availDelay = _configService.AvailabilityDelay;

                var movieTask = Task.Run(() => _moviesService.GetAllMovies());

                var sdict = movieStats.ToDictionary(x => x.MovieId);

                if (!excludeLocalCovers)
                {
                    coverFileInfos = _coverMapper.GetMovieCoverFileInfos();
                }

                var movies = movieTask.GetAwaiter().GetResult();
                moviesResources = new List<MovieResource>(movies.Count);

                foreach (var movie in movies)
                {
                    moviesResources.Add(movie.ToResource(availDelay, _qualityUpgradableSpecification));
                }

                if (!excludeLocalCovers)
                {
                    MapCoversToLocal(moviesResources, coverFileInfos);
                }

                LinkMovieStatistics(moviesResources, sdict);

                var rootFolders = _rootFolderService.All();

                moviesResources.ForEach(m => m.RootFolderPath = _rootFolderService.GetBestRootFolderPath(m.Path, rootFolders));
            }

            return moviesResources;
        }

        protected override MovieResource GetResourceById(int id)
        {
            if (_useCache)
            {
                return GetMovieResource(id);
            }

            var movie = _moviesService.GetMovie(id);

            return MapToResource(movie);
        }

        [HttpGet("list")]
        public List<int> ListMovies()
        {
            var moviesResources = new List<MovieResource>();

            var movieTask = Task.Run(() => _moviesService.AllMovieIds());

            return movieTask.GetAwaiter().GetResult();
        }

        // Added to support bulk monitor from Studio and eventually Performer detail page groupings
        [HttpPatch("bulk/monitor")]
        public IActionResult SetMoviesMonitored([FromBody] List<int> ids, [FromQuery] bool? monitored)
        {
            if (monitored == null)
            {
                return BadRequest("You must specify ?monitored=true or ?monitored=false.");
            }

            if (ids == null || !ids.Any())
            {
                return BadRequest("No IDs provided.");
            }

            var toUpdate = _moviesService.GetMovies(ids);

            if (toUpdate == null || !toUpdate.Any())
            {
                return NotFound("No movies found for given IDs.");
            }

            foreach (var movie in toUpdate)
            {
                movie.Monitored = monitored.Value;
            }

            var updated = _moviesService.UpdateMovieMonitored(toUpdate, (bool)monitored);
            foreach (var movie in updated)
            {
                _movieResourcesCache.Remove(movie.Id.ToString());
                BroadcastResourceChange(ModelAction.Updated, MapToResource(movie));
            }

            return Ok();
        }

        [HttpPost("bulk")]
        public List<MovieResource> GetResourceByIds([FromBody] List<int> ids)
        {
            if (_useCache)
            {
                return GetMovieResources(ids);
            }

            var moviesResources = new List<MovieResource>();

            var movieStats = _movieStatisticsService.MovieStatistics(ids);
            var coverFileInfos = _coverMapper.GetMovieCoverFileInfos();
            var sdict = movieStats.ToDictionary(x => x.MovieId);
            var availDelay = _configService.AvailabilityDelay;
            var movies = _moviesService.FindByIds(ids);

            foreach (var movie in movies)
            {
                moviesResources.Add(movie.ToResource(availDelay, _qualityUpgradableSpecification));
            }

            LinkMovieStatistics(moviesResources, sdict);
            MapCoversToLocal(moviesResources, coverFileInfos);

            var rootFolders = _rootFolderService.All();

            moviesResources.ForEach(m => m.RootFolderPath = _rootFolderService.GetBestRootFolderPath(m.Path, rootFolders));

            return moviesResources;
        }

        [HttpGet("listByPerformerForeignId")]
        public List<int> ListByPerformerForeignId(string performerForeignId)
        {
            var moviesList = new List<int>();
            if (_useCache)
            {
                var moviesResources = GetMovieResources();
                moviesList = moviesResources.Where(m => m.Credits.Where(c => c.Performer.ForeignId == performerForeignId).Any()).Map(x => x.Id).ToList();
            }
            else
            {
                moviesList = _moviesService.GetByPerformerForeignId(performerForeignId).Map(x => x.Id).ToList();
            }

            return moviesList;
        }

        [HttpGet("listByStudioForeignId")]
        public List<int> ListByStudioForeignId(string studioForeignId)
        {
            return _moviesService.GetByStudioForeignId(studioForeignId).Map(x => x.Id).ToList();
        }

        protected MovieResource MapToResource(Movie movie)
        {
            if (movie == null)
            {
                return null;
            }

            var availDelay = _configService.AvailabilityDelay;

            var resource = movie.ToResource(availDelay, _qualityUpgradableSpecification);
            MapCoversToLocal(resource);
            FetchAndLinkMovieStatistics(resource);

            resource.RootFolderPath = _rootFolderService.GetBestRootFolderPath(resource.Path);

            if (_useCache)
            {
                _movieResourcesCache.Set(resource.Id.ToString(), resource);
            }

            return resource;
        }

        [RestPostById]
        [Consumes("application/json")]
        [Produces("application/json")]
        public ActionResult<MovieResource> AddMovie([FromBody] MovieResource moviesResource)
        {
            var movie = _addMovieService.AddMovie(moviesResource.ToModel());

            return Created(movie.Id);
        }

        [RestPutById]
        [Consumes("application/json")]
        [Produces("application/json")]
        public ActionResult<MovieResource> UpdateMovie([FromBody] MovieResource moviesResource, [FromQuery] bool moveFiles = false)
        {
            var movie = _moviesService.GetMovie(moviesResource.Id);

            if (moveFiles)
            {
                var sourcePath = movie.Path;
                var destinationPath = moviesResource.Path;

                _commandQueueManager.Push(new MoveMovieCommand
                {
                    MovieId = movie.Id,
                    SourcePath = sourcePath,
                    DestinationPath = destinationPath
                }, trigger: CommandTrigger.Manual);
            }

            var model = moviesResource.ToModel(movie);

            var updatedMovie = _moviesService.UpdateMovie(model);

            _movieResourcesCache.Remove(updatedMovie.Id.ToString());
            BroadcastResourceChange(ModelAction.Updated, MapToResource(updatedMovie));

            return Accepted(moviesResource.Id);
        }

        [RestDeleteById]
        public void DeleteMovie(int id, bool deleteFiles = false, bool addImportExclusion = false)
        {
            _moviesService.DeleteMovie(id, deleteFiles, addImportExclusion);
        }

        private void MapCoversToLocal(MovieResource movie)
        {
            _coverMapper.ConvertToLocalUrls(movie.Id, movie.Images);
        }

        private void MapCoversToLocal(IEnumerable<MovieResource> movies, Dictionary<string, FileInfo> coverFileInfos)
        {
            _coverMapper.ConvertToLocalUrls(movies.Select(x => Tuple.Create(x.Id, x.Images.AsEnumerable())), coverFileInfos);
        }

        private void FetchAndLinkMovieStatistics(MovieResource resource)
        {
            LinkMovieStatistics(resource, _movieStatisticsService.MovieStatistics(resource.Id));
        }

        private void LinkMovieStatistics(List<MovieResource> resources, Dictionary<int, MovieStatistics> sDict)
        {
            foreach (var movie in resources)
            {
                if (sDict.TryGetValue(movie.Id, out var stats))
                {
                    LinkMovieStatistics(movie, stats);
                }
            }
        }

        private void LinkMovieStatistics(MovieResource resource, MovieStatistics movieStatistics)
        {
            resource.Statistics = movieStatistics.ToResource();
            resource.HasFile = movieStatistics.MovieFileCount > 0;
            resource.SizeOnDisk = movieStatistics.SizeOnDisk;
        }

        [NonAction]
        public void Handle(MovieFileImportedEvent message)
        {
            _movieResourcesCache.Remove(message.MovieInfo.Movie.Id.ToString());

            var updatedMovie = _moviesService.GetMovie(message.MovieInfo.Movie.Id);
            BroadcastResourceChange(ModelAction.Updated, MapToResource(updatedMovie));
        }

        [NonAction]
        public void Handle(MovieFileDeletedEvent message)
        {
            if (message.Reason == DeleteMediaFileReason.Upgrade
                || message.MovieFile.MovieId == 0)
            {
                return;
            }

            _movieResourcesCache.Remove(message.MovieFile.MovieId.ToString());
            var updatedMovie = _moviesService.GetMovie(message.MovieFile.MovieId);
            BroadcastResourceChange(ModelAction.Updated, MapToResource(updatedMovie));
        }

        [NonAction]
        public void Handle(MovieUpdatedEvent message)
        {
            _movieResourcesCache.Remove(message.Movie.Id.ToString());
            BroadcastResourceChange(ModelAction.Updated, MapToResource(message.Movie));
        }

        [NonAction]
        public void Handle(MovieEditedEvent message)
        {
            _movieResourcesCache.Remove(message.Movie.Id.ToString());
            BroadcastResourceChange(ModelAction.Updated, MapToResource(message.Movie));
        }

        [NonAction]
        public void Handle(MoviesDeletedEvent message)
        {
            foreach (var movie in message.Movies)
            {
                _movieResourcesCache.Remove(movie.Id.ToString());
                BroadcastResourceChange(ModelAction.Deleted, movie.Id);
            }
        }

        [NonAction]
        public void Handle(MovieRenamedEvent message)
        {
            _movieResourcesCache.Remove(message.Movie.Id.ToString());
            BroadcastResourceChange(ModelAction.Updated, MapToResource(message.Movie));
        }

        [NonAction]
        public void Handle(MediaCoversUpdatedEvent message)
        {
            if (message.Updated)
            {
                _movieResourcesCache.Remove(message.Movie.Id.ToString());
                var updatedMovie = _moviesService.GetMovie(message.Movie.Id);
                BroadcastResourceChange(ModelAction.Updated, MapToResource(updatedMovie));
            }
        }

        private MovieResource GetMovieResource(int id)
        {
            return _movieResourcesCache.Get(id.ToString(), () =>
            {
                var ids = new List<int>() { id };

                var moviesResources = new List<MovieResource>();
                var movieStats = _movieStatisticsService.MovieStatistics(ids);

                var coverFileInfos = _coverMapper.GetMovieCoverFileInfos();
                var sdict = movieStats.ToDictionary(x => x.MovieId);
                var availDelay = _configService.AvailabilityDelay;
                var movies = _moviesService.FindByIds(ids);

                foreach (var movie in movies)
                {
                    try
                    {
                        moviesResources.Add(movie.ToResource(availDelay, _qualityUpgradableSpecification));
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "Error Converting  '{0}' to Resource", movie);
                    }
                }

                LinkMovieStatistics(moviesResources, sdict);
                MapCoversToLocal(moviesResources.FirstOrDefault());

                return moviesResources.FirstOrDefault();
            });
        }

        private List<MovieResource> GetMovieResources()
        {
            var ids = ListMovies();
            return GetMovieResources(ids);
        }

        private List<MovieResource> GetMovieResources(List<int> ids)
        {
            var moviesResources = new List<MovieResource>();

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            _logger.Trace($"GetMovieResources {ids.Count} movies");

            var missingIds = new List<int>();
            foreach (var id in ids)
            {
                var movieResource = _movieResourcesCache.Find(id.ToString());
                if (movieResource == null)
                {
                    missingIds.Add(id);
                }
                else
                {
                    moviesResources.AddIfNotNull(movieResource);
                }
            }

            if (missingIds.Count > 0)
            {
                var releaseLock = false;
                var getIds = new List<int>();

                try
                {
                    // If there are a large number of missing IDs, acquire the lock to prevent cache stampede
                    if (missingIds.Count > 100)
                    {
                        _logger.Info($"Caching {missingIds.Count} movies with {_movieResourcesCache.Lock.CurrentCount} avalible threads");
                        _movieResourcesCache.Lock.Wait();
                        releaseLock = true;
                        if (stopwatch.Elapsed.TotalSeconds > 2)
                        {
                            _logger.Warn($"Locked movie cache for {stopwatch.Elapsed.TotalSeconds} seconds");
                        }

                        // recheck after acquiring the lock
                        foreach (var id in missingIds)
                        {
                            var movieResource = _movieResourcesCache.Find(id.ToString());
                            if (movieResource == null)
                            {
                                getIds.Add(id);
                            }
                            else
                            {
                                moviesResources.AddIfNotNull(movieResource);
                            }
                        }
                    }
                    else
                    {
                        getIds = missingIds;
                    }

                    if (getIds.Count > 0)
                    {
                        var coverFileInfos = _coverMapper.GetMovieCoverFileInfos();
                        var availDelay = _configService.AvailabilityDelay;

                        var movies = _moviesService.FindByIds(getIds);
                        var movieStats = _movieStatisticsService.MovieStatistics(getIds);
                        var sdict = movieStats.ToDictionary(x => x.MovieId);

                        foreach (var movie in movies)
                        {
                            try
                            {
                                moviesResources.Add(movie.ToResource(availDelay, _qualityUpgradableSpecification));
                            }
                            catch (Exception e)
                            {
                                _logger.Error(e, "Error Converting  '{0}' to Resource", movie);
                            }
                        }

                        LinkMovieStatistics(moviesResources, sdict);
                        MapCoversToLocal(moviesResources, coverFileInfos);

                        var rootFolders = _rootFolderService.All();

                        moviesResources.ForEach(m => m.RootFolderPath = _rootFolderService.GetBestRootFolderPath(m.Path, rootFolders));

                        if (_useCache)
                        {
                            foreach (var moviesResource in moviesResources)
                            {
                                _movieResourcesCache.Set(moviesResource.Id.ToString(), moviesResource);
                            }
                        }
                    }
                }
                finally
                {
                    stopwatch.Stop();
                    if (releaseLock)
                    {
                        _movieResourcesCache.Lock.Release();
                    }
                }
            }

            if (stopwatch.Elapsed.TotalSeconds > 60)
            {
                _logger.Warn($"Processed movie cache for {ids.Count} after {stopwatch.Elapsed.TotalSeconds} seconds");
            }

            return moviesResources;
        }
    }
}
