using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DryIoc.ImTools;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.ImportLists.ImportExclusions;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Performers;
using NzbDrone.Core.Movies.Performers.Events;
using NzbDrone.Core.MovieStats;
using NzbDrone.SignalR;
using Whisparr.Http;
using Whisparr.Http.REST;
using Whisparr.Http.REST.Attributes;

namespace Whisparr.Api.V3.Performers
{
    /// <summary>Controller for managing performers in Whisparr</summary>
    [V3ApiController]
    public class PerformerController : RestControllerWithSignalR<PerformerResource, Performer>, IHandle<PerformerUpdatedEvent>
    {
        private readonly IPerformerService _performerService;
        private readonly IAddPerformerService _addPerformerService;
        private readonly IMapCoversToLocal _coverMapper;
        private readonly IMovieService _moviesService;
        private readonly IMovieStatisticsService _movieStatisticsService;
        private readonly IImportListExclusionService _exclusionService;
        private readonly IConfigService _configService;
        private readonly bool _useCache;
        private readonly ICached<PerformerResource> _performerResourceCache;
        private readonly Logger _logger;

        public PerformerController(IPerformerService performerService,
                                   IAddPerformerService addPerformerService,
                                   IMapCoversToLocal coverMapper,
                                   IMovieService moviesService,
                                   IMovieStatisticsService movieStatisticsService,
                                   IImportListExclusionService exclusionService,
                                   ICacheManager cacheManager,
                                   IConfigService configService,
                                   Logger logger,
                                   IBroadcastSignalRMessage signalRBroadcaster)
        : base(signalRBroadcaster)
        {
            _performerService = performerService;
            _addPerformerService = addPerformerService;
            _configService = configService;
            _coverMapper = coverMapper;
            _moviesService = moviesService;
            _movieStatisticsService = movieStatisticsService;
            _exclusionService = exclusionService;
            _useCache = _configService.WhisparrCachePerformerAPI;
            _performerResourceCache = cacheManager.GetCache<PerformerResource>(typeof(PerformerResource), "performerResources");
            _logger = logger;

            if (configService.WhisparrMovieMetadataSource == MovieMetadataType.TMDB)
            {
                SharedValidator.RuleFor(s => s.MoviesMonitored)
                    .Must((s, monitored) => !monitored || s.TmdbId > 0)
                    .WithMessage("Requires a TMDB link to be added to the Performer within StashDB");
            }

            if (configService.WhisparrMovieMetadataSource == MovieMetadataType.TPDB)
            {
                SharedValidator.RuleFor(s => s.MoviesMonitored)
                    .Must((s, monitored) => !monitored || !string.IsNullOrWhiteSpace(s.TpdbId))
                    .WithMessage("Requires a TPDB link to be added to the Performer within StashDB");
            }
        }

        /// <summary>Retrieves a performer by their Whisparr (local)internal ID</summary>
        /// <param name="id">The internal ID of the performer</param>
        /// <returns>Performer details with associated movies and local cover URLs</returns>
        /// <response code="200">Performer found and returned</response>
        /// <response code="404">Performer with the specified ID not found</response>
        [HttpGet("{id:int}")]
        [Produces("application/json")]
        protected override PerformerResource GetResourceById(int id)
        {
            var resource = _performerService.GetById(id).ToResource();

            _coverMapper.ConvertToLocalPerformerUrls(resource.Id, resource.Images);

            FetchAndLinkMovies(resource);

            return resource;
        }

        /// <summary>Retrieves full list of performers, or a single performer by their external foreign ID (e.g., from StashDb)</summary>
        /// <param name="stashId">The external foreign ID (StashDb ID) of the performer</param>
        /// <returns>Performer details with associated movies and local cover URLs</returns>
        /// <response code="200">Performer found and returned</response>
        /// <response code="404">Performer with the specified foreign ID not found</response>
        [HttpGet]
        [Produces("application/json")]
        public List<PerformerResource> GetPerformers(string stashId)
        {
            var performerResources = new List<PerformerResource>();

            if (_useCache)
            {
                if (stashId.IsNotNullOrWhiteSpace())
                {
                    performerResources.AddIfNotNull(GetPerformerResource(stashId));
                }
                else
                {
                    performerResources = GetPerformerResources();
                }

                return performerResources;
            }
            else
            {
                if (stashId.IsNotNullOrWhiteSpace())
                {
                    var performer = _performerService.FindByForeignId(stashId);

                    if (performer != null)
                    {
                        performerResources.AddIfNotNull(performer.ToResource());
                    }
                }
                else
                {
                    performerResources = _performerService.GetAllPerformers().ToResource();
                }
            }

            var coverFileInfos = _coverMapper.GetPerformerCoverFileInfos();

            _coverMapper.ConvertToLocalPerformerUrls(performerResources.Select(x => Tuple.Create(x.Id, x.Images.AsEnumerable())), coverFileInfos);

            LinkMovies(performerResources);

            return performerResources;
        }

        /// <summary>Adds a new performer to Whisparr</summary>
        /// <param name="performerResource">The performer details to add</param>
        /// <returns>The newly added performer details</returns>
        /// <response code="201">Performer successfully added</response>
        /// <response code="400">Invalid performer details provided</response>
        [RestPostById]
        [Consumes("application/json")]
        [Produces("application/json")]
        public ActionResult<PerformerResource> AddPerformer([FromBody] PerformerResource performerResource)
        {
            var performer = _addPerformerService.AddPerformer(performerResource.ToModel());

            return Created(performer.Id);
        }

        /// <summary>Updates an existing performer in Whisparr</summary>
        /// <param name="resource">The performer details to update</param>
        /// <returns>The updated performer details</returns>
        /// <response code="202">Performer successfully updated</response>
        /// <response code="400">Invalid performer details provided</response>
        /// <response code="404">Performer with the specified ID not found</response>
        [RestPutById]
        [Consumes("application/json")]
        [Produces("application/json")]
        public ActionResult<PerformerResource> Update([FromBody] PerformerResource resource)
        {
            var performer = _performerService.GetById(resource.Id);

            var updatedPerformer = _performerService.Update(resource.ToModel(performer));

            _performerResourceCache.Remove(updatedPerformer.ForeignId);
            BroadcastResourceChange(ModelAction.Updated, updatedPerformer.ToResource());

            return Accepted(updatedPerformer);
        }

        /// <summary>Deletes a performer and their associated movies/scenes from Whisparr</summary>
        /// <param name="id">The internal ID of the performer to delete</param>
        /// <param name="deleteFiles">If true, associated movie/scene files will also be deleted from disk</param>
        /// <param name="addImportExclusion">If true, an import exclusion will be added to prevent re-adding the performer in future imports</param>
        [RestDeleteById]
        public void DeletePerformer(int id, bool deleteFiles = false, bool addImportExclusion = false)
        {
            var performer = _performerService.GetById(id);

            if (performer == null)
            {
                return;
            }

            // Get the scenes for the performer
            var scenes = _moviesService.GetByPerformerForeignId(performer.ForeignId);
            var sceneIds = scenes.Map(x => x.Id).ToList();
            _moviesService.DeleteMovies(sceneIds, deleteFiles);

            if (addImportExclusion)
            {
                var exclusion = new ImportListExclusion();
                exclusion.ForeignId = performer.ForeignId;
                exclusion.MovieTitle = performer.Name;
                exclusion.Type = ImportExclusionType.Performer;

                _exclusionService.AddExclusion(exclusion);
            }

            // Remove the performer now that the associated scenes have been removed
            _performerService.RemovePerformer(performer);
        }

        /// <summary>Handles performer updated events to update the performer cache and broadcast changes via SignalR</summary>
        [NonAction]
        public void Handle(PerformerUpdatedEvent message)
        {
            var resource = message.Performer.ToResource();

            FetchAndLinkMovies(resource);
            _performerResourceCache.Remove(resource.ForeignId);
            BroadcastResourceChange(ModelAction.Updated, resource);
        }

        private void FetchAndLinkMovies(PerformerResource resource)
        {
            LinkMovies(resource, _moviesService.GetByPerformerForeignId(resource.ForeignId));
        }

        private void LinkMovies(List<PerformerResource> resources)
        {
            foreach (var performer in resources)
            {
                FetchAndLinkMovies(performer);
            }
        }

        private void LinkMovies(PerformerResource resource, List<Movie> movies)
        {
            var scenes = movies.Where(x => x.MovieMetadata.Value.ItemType == ItemType.Scene);
            resource.HasScenes = scenes.Any();
            resource.HasMovies = movies.Where(x => x.MovieMetadata.Value.ItemType == ItemType.Movie).Any();

            resource.Studios = scenes.Map(x => new PerformerStudioResource() { ForeignId = x.MovieMetadata.Value.StudioForeignId, Title = x.MovieMetadata.Value.StudioTitle }).DistinctBy(x => x.ForeignId).OrderBy(x => x.Title).ToList();

            resource.MovieCount = movies.Where(x => x.HasFile && x.MovieMetadata.Value.ItemType == ItemType.Movie).Count();
            resource.TotalMovieCount = movies.Where(x => x.MovieMetadata.Value.ItemType == ItemType.Movie).Count();
            resource.SceneCount = movies.Where(x => x.HasFile && x.MovieMetadata.Value.ItemType == ItemType.Scene).Count();
            resource.TotalSceneCount = movies.Where(x => x.MovieMetadata.Value.ItemType == ItemType.Scene).Count();

            var ids = movies.Map(x => x.Id).ToList();
            var movieStats = _movieStatisticsService.MovieStatistics(ids);
            resource.SizeOnDisk = movieStats.Sum(x => x.SizeOnDisk);
        }

        private PerformerResource GetPerformerResource(string performerForeignId)
        {
            var performerForeignIds = new List<string> { performerForeignId };
            return GetPerformerResources(performerForeignIds).FirstOrDefault();
        }

        private List<PerformerResource> GetPerformerResources()
        {
            var allPerformerForeignIds = _performerService.AllPerformerForeignIds();
            return GetPerformerResources(allPerformerForeignIds);
        }

        private List<PerformerResource> GetPerformerResources(List<string> performerForeignIds)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            _logger.Trace($"GetPerformerResources: {performerForeignIds.Count} performers");

            var performerResources = new List<PerformerResource>();

            var missingIds = new List<string>();
            foreach (var id in performerForeignIds)
            {
                var performerResource = _performerResourceCache.Find(id);
                if (performerResource == null)
                {
                    missingIds.Add(id);
                }
                else
                {
                    performerResources.AddIfNotNull(performerResource);
                }
            }

            if (missingIds.Count > 0)
            {
                var releaseLock = false;
                var getIds = new List<string>();

                try
                {
                    _logger.Info($"Caching {missingIds.Count} performers with {_performerResourceCache.Lock.CurrentCount} available threads.");

                    // If there are a large number of missing IDs, acquire the lock to prevent cache stampede
                    if (missingIds.Count > 100)
                    {
                        _performerResourceCache.Lock.Wait();
                        releaseLock = true;
                        if (stopwatch.Elapsed.TotalSeconds > 2)
                        {
                            _logger.Warn($"Locked performer cache for {stopwatch.Elapsed.TotalSeconds} seconds");
                        }

                        // recheck after acquiring the lock
                        foreach (var id in missingIds)
                        {
                            var performerResource = _performerResourceCache.Find(id);
                            if (performerResource == null)
                            {
                                getIds.Add(id);
                            }
                            else
                            {
                                performerResources.AddIfNotNull(performerResource);
                            }
                        }
                    }
                    else
                    {
                        getIds = missingIds;
                    }

                    if (getIds.Count > 0)
                    {
                        var performers = _performerService.FindByForeignIds(getIds);

                        foreach (var performer in performers)
                        {
                            performerResources.AddIfNotNull(performer.ToResource());
                        }

                        var coverFileInfos = _coverMapper.GetPerformerCoverFileInfos();

                        _coverMapper.ConvertToLocalPerformerUrls(performerResources.Select(x => Tuple.Create(x.Id, x.Images.AsEnumerable())), coverFileInfos);

                        LinkMovies(performerResources);

                        foreach (var performerResource in performerResources)
                        {
                            _performerResourceCache.Set(performerResource.ForeignId, performerResource);
                        }
                    }
                }
                finally
                {
                    stopwatch.Stop();
                    if (releaseLock)
                    {
                        _performerResourceCache.Lock.Release();
                    }
                }
            }

            if (stopwatch.Elapsed.TotalSeconds > 60)
            {
                _logger.Warn($"Processed performer cache for {performerForeignIds.Count} after {stopwatch.Elapsed.TotalSeconds} seconds");
            }

            return performerResources;
        }
    }
}
