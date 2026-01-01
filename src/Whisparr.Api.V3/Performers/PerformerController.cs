using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using DryIoc.ImTools;
using Microsoft.AspNetCore.Mvc;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Datastore;
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
        }

        protected override PerformerResource GetResourceById(int id)
        {
            var resource = _performerService.GetById(id).ToResource();

            _coverMapper.ConvertToLocalPerformerUrls(resource.Id, resource.Images);

            FetchAndLinkMovies(resource);

            return resource;
        }

        [HttpGet]
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

        [HttpGet("paged")]
        [Produces("application/json")]
        public PagingResource<PerformerResource> GetPagedPerformers([FromQuery] PerformerPagingRequestResource request)
        {
            request ??= new PerformerPagingRequestResource();

            var page = request.ResolvePage();
            var pageSize = request.ResolvePageSize();
            var sortKey = string.IsNullOrWhiteSpace(request.SortKey) ? "fullName" : request.SortKey;
            var resolvedDirection = request.SortDirection.HasValue && request.SortDirection != SortDirection.Default
                ? request.SortDirection.Value
                : SortDirection.Ascending;

            var sortDirection = resolvedDirection == SortDirection.Descending ? "descending" : "ascending";

            var filters = PerformerFilterDefinition.Parse(request.FilterPayload);

            if (filters.Count == 0)
            {
                filters = BuildLegacyFilters();
            }

            var performers = GetPerformerResources();
            var filtered = PerformerFilterEvaluator.ApplyFilters(performers, filters).ToList();
            var ordered = PerformerFilterEvaluator.ApplyOrdering(filtered, sortKey, sortDirection).ToList();

            var totalRecords = filtered.Count;
            var totalPages = Math.Max((int)Math.Ceiling(totalRecords / (double)pageSize), 1);

            if (page > totalPages)
            {
                page = totalPages;
            }

            var skip = (page - 1) * pageSize;
            var records = ordered.Skip(skip).Take(pageSize).ToList();

            return new PagingResource<PerformerResource>
            {
                Page = page,
                PageSize = pageSize,
                SortKey = sortKey,
                SortDirection = resolvedDirection,
                TotalRecords = totalRecords,
                Records = records
            };
        }

        [RestPostById]
        public ActionResult<PerformerResource> AddPerformer(PerformerResource performerResource)
        {
            var performer = _addPerformerService.AddPerformer(performerResource.ToModel());

            return Created(performer.Id);
        }

        [RestPutById]
        public ActionResult<PerformerResource> Update(PerformerResource resource)
        {
            var performer = _performerService.GetById(resource.Id);

            var updatedPerformer = _performerService.Update(resource.ToModel(performer));

            _performerResourceCache.Remove(updatedPerformer.ForeignId);
            BroadcastResourceChange(ModelAction.Updated, updatedPerformer.ToResource());

            return Accepted(updatedPerformer);
        }

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

            resource.SceneCount = movies.Where(x => x.HasFile).Count();
            resource.TotalSceneCount = movies.Count;
            var ids = movies.Map(x => x.Id).ToList();
            var movieStats = _movieStatisticsService.MovieStatistics(ids);
            resource.SizeOnDisk = movieStats.Sum(x => x.SizeOnDisk);
        }

        private IReadOnlyList<PerformerFilterDefinition> BuildLegacyFilters()
        {
            var query = Request?.Query;

            if (query == null || query.Count == 0)
            {
                return Array.Empty<PerformerFilterDefinition>();
            }

            var filters = new List<PerformerFilterDefinition>();

            void AddBoolean(string key)
            {
                if (query.TryGetValue(key, out var values) && bool.TryParse(values.LastOrDefault(), out var parsed))
                {
                    filters.Add(new PerformerFilterDefinition
                    {
                        Key = key,
                        Comparison = "equal",
                        ValueType = "bool",
                        Values = new List<object> { parsed }
                    });
                }
            }

            void AddNumeric(string key)
            {
                if (query.TryGetValue(key, out var values) && double.TryParse(values.LastOrDefault(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
                {
                    filters.Add(new PerformerFilterDefinition
                    {
                        Key = key,
                        Comparison = "equal",
                        ValueType = "number",
                        Values = new List<object> { parsed }
                    });
                }
            }

            void AddString(string key)
            {
                if (query.TryGetValue(key, out var values))
                {
                    var entries = values
                        .SelectMany(value => value.Split(',', StringSplitOptions.RemoveEmptyEntries))
                        .Select(value => value.Trim())
                        .Where(value => value.Length > 0)
                        .ToList();

                    if (entries.Count > 0)
                    {
                        filters.Add(new PerformerFilterDefinition
                        {
                            Key = key,
                            Comparison = "equal",
                            ValueType = "string",
                            Values = entries.Cast<object>().ToList()
                        });
                    }
                }
            }

            void AddTags()
            {
                if (query.TryGetValue("tags", out var tagValues))
                {
                    var tags = tagValues
                        .SelectMany(value => value.Split(',', StringSplitOptions.RemoveEmptyEntries))
                        .Select(value => value.Trim())
                        .Where(value => value.Length > 0)
                        .Cast<object>()
                        .ToList();

                    if (tags.Count > 0)
                    {
                        filters.Add(new PerformerFilterDefinition
                        {
                            Key = "tags",
                            Comparison = "contains",
                            ValueType = "tag",
                            Values = tags
                        });
                    }
                }
            }

            AddBoolean("monitored");
            AddBoolean("moviesMonitored");

            AddNumeric("sceneCount");
            AddNumeric("totalSceneCount");
            AddNumeric("age");
            AddNumeric("careerStart");
            AddNumeric("careerEnd");
            AddNumeric("qualityProfileId");

            AddString("status");
            AddString("fullName");
            AddString("rootFolderPath");
            AddString("monitor");
            AddString("gender");
            AddString("hairColor");
            AddString("ethnicity");

            AddTags();

            return filters;
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
