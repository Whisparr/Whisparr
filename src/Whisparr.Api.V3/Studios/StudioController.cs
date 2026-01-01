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
using NzbDrone.Core.Movies.Studios;
using NzbDrone.Core.Movies.Studios.Events;
using NzbDrone.Core.MovieStats;
using NzbDrone.SignalR;
using Whisparr.Http;
using Whisparr.Http.REST;
using Whisparr.Http.REST.Attributes;

namespace Whisparr.Api.V3.Studios
{
    [V3ApiController]
    public class StudioController : RestControllerWithSignalR<StudioResource, Studio>, IHandle<StudioUpdatedEvent>
    {
        private readonly IStudioService _studioService;
        private readonly IAddStudioService _addStudioService;
        private readonly IMapCoversToLocal _coverMapper;
        private readonly IMovieService _moviesService;
        private readonly IMovieStatisticsService _movieStatisticsService;
        private readonly IImportListExclusionService _exclusionService;
        private readonly ICached<StudioResource> _studioResourceCache;
        private readonly bool _useCache;
        private readonly Logger _logger;

        public StudioController(IStudioService studioService,
                                IAddStudioService addStudioService,
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
            _studioService = studioService;
            _addStudioService = addStudioService;
            _coverMapper = coverMapper;
            _moviesService = moviesService;
            _movieStatisticsService = movieStatisticsService;
            _exclusionService = exclusionService;
            _useCache = configService.WhisparrCacheStudioAPI;
            _studioResourceCache = cacheManager.GetCache<StudioResource>(typeof(StudioResource), "studioResources");
            _logger = logger;
        }

        protected override StudioResource GetResourceById(int id)
        {
            var resource = _studioService.GetById(id).ToResource();

            _coverMapper.ConvertToLocalStudioUrls(resource.Id, resource.Images);

            FetchAndLinkMovies(resource);

            return resource;
        }

        [HttpGet]
        public List<StudioResource> GetStudios(string stashId)
        {
            var studioResources = new List<StudioResource>();

            if (_useCache)
            {
                if (stashId.IsNotNullOrWhiteSpace())
                {
                    studioResources.AddIfNotNull(GetStudioResource(stashId));
                }
                else
                {
                    studioResources = GetStudioResources();
                }
            }
            else
            {
                if (stashId.IsNotNullOrWhiteSpace())
                {
                    var studio = _studioService.FindByForeignId(stashId);

                    if (studio != null)
                    {
                        studioResources.AddIfNotNull(studio.ToResource());
                    }
                }
                else
                {
                    studioResources = _studioService.GetAllStudios().ToResource();
                }

                var coverFileInfos = _coverMapper.GetStudioCoverFileInfos();

                _coverMapper.ConvertToLocalStudioUrls(studioResources.Select(x => Tuple.Create(x.Id, x.Images.AsEnumerable())), coverFileInfos);

                LinkMovies(studioResources);
            }

            return studioResources;
        }

        [HttpGet("paged")]
        [Produces("application/json")]
        public PagingResource<StudioResource> GetPagedStudios([FromQuery] StudioPagingRequestResource request)
        {
            request ??= new StudioPagingRequestResource();

            var page = request.ResolvePage();
            var pageSize = request.ResolvePageSize();
            var sortKey = string.IsNullOrWhiteSpace(request.SortKey) ? "sortTitle" : request.SortKey;
            var resolvedDirection = request.SortDirection.HasValue && request.SortDirection != SortDirection.Default
                ? request.SortDirection.Value
                : SortDirection.Ascending;

            var sortDirection = resolvedDirection == SortDirection.Descending ? "descending" : "ascending";

            var filters = StudioFilterDefinition.Parse(request.FilterPayload);

            if (filters.Count == 0)
            {
                filters = BuildLegacyFilters();
            }

            var studios = GetStudioResources();
            var filtered = StudioFilterEvaluator.ApplyFilters(studios, filters).ToList();
            var ordered = StudioFilterEvaluator.ApplyOrdering(filtered, sortKey, sortDirection).ToList();

            var totalRecords = filtered.Count;
            var totalPages = Math.Max((int)Math.Ceiling(totalRecords / (double)pageSize), 1);

            if (page > totalPages)
            {
                page = totalPages;
            }

            var skip = (page - 1) * pageSize;
            var records = ordered.Skip(skip).Take(pageSize).ToList();

            return new PagingResource<StudioResource>
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
        public ActionResult<StudioResource> AddStudio(StudioResource studioResource)
        {
            var studio = _addStudioService.AddStudio(studioResource.ToModel());

            return Created(studio.Id);
        }

        [RestPutById]
        public ActionResult<StudioResource> Update(StudioResource resource)
        {
            var studio = _studioService.GetById(resource.Id);

            var updatedStudio = _studioService.Update(resource.ToModel(studio));

            _studioResourceCache.Remove(updatedStudio.ForeignId);
            BroadcastResourceChange(ModelAction.Updated, updatedStudio.ToResource());

            return Accepted(updatedStudio);
        }

        [RestDeleteById]
        public void DeleteStudio(int id, bool deleteFiles = false, bool addImportExclusion = false)
        {
            var studio = _studioService.GetById(id);

            if (studio == null)
            {
                return;
            }

            // Get the scenes for the studio
            var scenes = _moviesService.GetByStudioForeignId(studio.ForeignId);
            var sceneIds = scenes.Map(x => x.Id).ToList();
            _moviesService.DeleteMovies(sceneIds, deleteFiles);

            if (addImportExclusion)
            {
                var exclusion = new ImportListExclusion();
                exclusion.ForeignId = studio.ForeignId;
                exclusion.MovieTitle = studio.Title;
                exclusion.Type = ImportExclusionType.Studio;

                _exclusionService.AddExclusion(exclusion);
            }

            // Remove the studio now that the associated scenes have been removed
            _studioResourceCache.Remove(studio.ForeignId);
            _studioService.RemoveStudio(studio);
        }

        [NonAction]
        public void Handle(StudioUpdatedEvent message)
        {
            var resource = message.Studio.ToResource();

            _studioResourceCache.Remove(resource.ForeignId);
            FetchAndLinkMovies(resource);
            BroadcastResourceChange(ModelAction.Updated, message.Studio.ToResource());
        }

        private void FetchAndLinkMovies(StudioResource resource)
        {
            LinkMovies(resource, _moviesService.GetByStudioForeignId(resource.ForeignId));
        }

        private void LinkMovies(List<StudioResource> resources)
        {
            foreach (var performer in resources)
            {
                FetchAndLinkMovies(performer);
            }
        }

        private void LinkMovies(StudioResource resource, List<Movie> movies)
        {
            var scenes = movies.Where(x => x.MovieMetadata.Value.ItemType == ItemType.Scene);
            resource.HasScenes = scenes.Any();
            resource.HasMovies = movies.Where(x => x.MovieMetadata.Value.ItemType == ItemType.Movie).Any();

            resource.Years = scenes.OrderBy(x => x.Year).Map(x => x.Year).Distinct().ToList();

            resource.SceneCount = movies.Where(x => x.HasFile).Count();
            resource.TotalSceneCount = movies.Count;
            var ids = movies.Map(x => x.Id).ToList();
            var movieStats = _movieStatisticsService.MovieStatistics(ids);
            resource.SizeOnDisk = movieStats.Sum(x => x.SizeOnDisk);
        }

        private IReadOnlyList<StudioFilterDefinition> BuildLegacyFilters()
        {
            var query = Request?.Query;

            if (query == null || query.Count == 0)
            {
                return Array.Empty<StudioFilterDefinition>();
            }

            var filters = new List<StudioFilterDefinition>();

            void AddBoolean(string key)
            {
                if (query.TryGetValue(key, out var values) && bool.TryParse(values.LastOrDefault(), out var parsed))
                {
                    filters.Add(new StudioFilterDefinition
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
                    filters.Add(new StudioFilterDefinition
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
                        filters.Add(new StudioFilterDefinition
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
                        filters.Add(new StudioFilterDefinition
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

            AddNumeric("qualityProfileId");
            AddNumeric("sceneCount");
            AddNumeric("totalSceneCount");

            AddString("title");
            AddString("sortTitle");
            AddString("status");
            AddString("network");
            AddString("rootFolderPath");
            AddString("monitor");

            AddTags();

            return filters;
        }

        private StudioResource GetStudioResource(string studioForeignId)
        {
            var studioIds = new List<string> { studioForeignId };
            return GetStudioResources(studioIds).FirstOrDefault();
        }

        private List<StudioResource> GetStudioResources()
        {
            var allStudioForeignIds = _studioService.AllStudioForeignIds();
            return GetStudioResources(allStudioForeignIds);
        }

        private List<StudioResource> GetStudioResources(List<string> studioForeignIds)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            _logger.Trace($"GetStudioResources: {studioForeignIds.Count} studios");

            var studioResources = new List<StudioResource>();

            var missingIds = new List<string>();
            foreach (var id in studioForeignIds)
            {
                var studioResource = _studioResourceCache.Find(id);
                if (studioResource == null)
                {
                    missingIds.Add(id);
                }
                else
                {
                    studioResources.AddIfNotNull(studioResource);
                }
            }

            if (missingIds.Count > 0)
            {
                var releaseLock = false;
                var getIds = new List<string>();

                try
                {
                    _logger.Info($"Caching {missingIds.Count} studios with {_studioResourceCache.Lock.CurrentCount} available threads.");

                    // If there are a large number of missing IDs, acquire the lock to prevent cache stampede
                    if (missingIds.Count > 100)
                    {
                        _studioResourceCache.Lock.Wait();
                        releaseLock = true;
                        if (stopwatch.Elapsed.TotalSeconds > 2)
                        {
                            _logger.Warn($"Locked studio cache for {stopwatch.Elapsed.TotalSeconds} seconds");
                        }

                        // Re-check missing IDs after acquiring the lock
                        foreach (var id in missingIds)
                        {
                            var studioResource = _studioResourceCache.Find(id);
                            if (studioResource == null)
                            {
                                getIds.Add(id);
                            }
                            else
                            {
                                studioResources.AddIfNotNull(studioResource);
                            }
                        }
                    }
                    else
                    {
                        getIds = missingIds;
                    }

                    if (getIds.Count > 0)
                    {
                        var studios = _studioService.FindByForeignIds(getIds);

                        foreach (var studio in studios)
                        {
                            studioResources.AddIfNotNull(studio.ToResource());
                        }

                        var coverFileInfos = _coverMapper.GetStudioCoverFileInfos();

                        _coverMapper.ConvertToLocalStudioUrls(studioResources.Select(x => Tuple.Create(x.Id, x.Images.AsEnumerable())), coverFileInfos);

                        LinkMovies(studioResources);

                        foreach (var studioResource in studioResources)
                        {
                            _studioResourceCache.Set(studioResource.ForeignId, studioResource);
                        }
                    }
                }
                finally
                {
                    stopwatch.Stop();
                    if (releaseLock)
                    {
                        _studioResourceCache.Lock.Release();
                    }
                }
            }

            if (stopwatch.Elapsed.TotalSeconds > 60)
            {
                _logger.Warn($"Processed studio cache for {studioForeignIds.Count} after {stopwatch.Elapsed.TotalSeconds} seconds");
            }

            return studioResources;
        }
    }
}
