using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.ImportLists.ImportExclusions;
using Whisparr.Http;
using Whisparr.Http.Extensions;
using Whisparr.Http.REST;
using Whisparr.Http.REST.Attributes;

namespace Whisparr.Api.V3.ImportLists
{
    [V3ApiController("exclusions")]
    public class ImportListExclusionController : RestController<ImportListExclusionResource>
    {
        private readonly IImportListExclusionService _importListExclusionService;
        private readonly ICached<ImportListExclusionResource> _exclusionResourceCache;
        private readonly bool _useCache;
        private readonly Logger _logger;

        public ImportListExclusionController(IImportListExclusionService importListExclusionService,
                                            ICacheManager cacheManager,
                                            IConfigService configService,
                                            Logger logger,
                                            ImportListExclusionExistsValidator importListExclusionExistsValidator)
        {
            _importListExclusionService = importListExclusionService;
            _exclusionResourceCache = cacheManager.GetCache<ImportListExclusionResource>(typeof(ImportListExclusionResource), "exclusionResources");
            _useCache = configService.WhisparrCacheExclusionAPI;
            _logger = logger;

            SharedValidator.RuleFor(c => c.ForeignId).Cascade(CascadeMode.Stop)
                .NotEmpty()
                .SetValidator(importListExclusionExistsValidator);

            SharedValidator.RuleFor(c => c.MovieTitle).NotEmpty();
            SharedValidator.RuleFor(c => c.MovieYear).GreaterThan(0);
        }

        // Endpoint only used by stasharr to show excluded scenes
        [HttpGet]
        [Produces("application/json")]
        public List<ImportListExclusionResource> GetImportListExclusions(string stashId, ImportExclusionType type = ImportExclusionType.Scene)
        {
            var importListExclusionResources = new List<ImportListExclusionResource>();

            if (stashId.IsNotNullOrWhiteSpace())
            {
                var importListExclusionResource = _importListExclusionService.GetByForeignId(stashId).ToResource();

                if (importListExclusionResource != null)
                {
                    importListExclusionResources.AddIfNotNull(importListExclusionResource);
                }
            }
            else
            {
                if (_useCache)
                {
                    importListExclusionResources = GetExclusionResources();
                    importListExclusionResources = importListExclusionResources.Where(x => x.Type == type).ToList();
                }
                else
                {
                    importListExclusionResources = _importListExclusionService.GetAllByType(type).ToResource();
                }
            }

            return importListExclusionResources;
        }

        protected override ImportListExclusionResource GetResourceById(int id)
        {
            if (_useCache)
            {
                return GetExclusionResource(id);
            }
            else
            {
                return _importListExclusionService.GetById(id).ToResource();
            }
        }

        [HttpGet("paged")]
        [Produces("application/json")]
        public PagingResource<ImportListExclusionResource> GetImportListExclusionsPaged([FromQuery] PagingRequestResource paging)
        {
            var pagingResource = new PagingResource<ImportListExclusionResource>(paging);
            var pageSpec = pagingResource.MapToPagingSpec<ImportListExclusionResource, ImportListExclusion>(
                new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "id",
                    "foreignId",
                    "movieTitle",
                    "movieYear"
                },
                "id",
                SortDirection.Descending);

            return pageSpec.ApplyToPage(_importListExclusionService.Paged, ImportListExclusionResourceMapper.ToResource);
        }

        [RestPostById]
        [Consumes("application/json")]
        public ActionResult<ImportListExclusionResource> AddImportListExclusion([FromBody] ImportListExclusionResource resource)
        {
            var importListExclusion = _importListExclusionService.AddExclusion(resource.ToModel());

            return Created(importListExclusion.Id);
        }

        [RestPutById]
        [Consumes("application/json")]
        public ActionResult<ImportListExclusionResource> UpdateImportListExclusion([FromBody] ImportListExclusionResource resource)
        {
            _importListExclusionService.Update(resource.ToModel());
            _exclusionResourceCache.Remove($"{resource.Id}");
            return Accepted(resource.Id);
        }

        [HttpPost("bulk")]
        public object AddImportListExclusions([FromBody] List<ImportListExclusionResource> resources)
        {
            var importListExclusions = _importListExclusionService.AddExclusions(resources.ToModel());

            return importListExclusions.ToResource();
        }

        [RestDeleteById]
        public void DeleteImportListExclusion(int id)
        {
            var exclusion = _importListExclusionService.GetById(id);
            _importListExclusionService.RemoveExclusion(exclusion);

            _exclusionResourceCache.Remove($"{id}");
        }

        [HttpDelete("bulk")]
        [Produces("application/json")]
        public object DeleteImportListExclusions([FromBody] ImportListExclusionBulkResource resource)
        {
            foreach (var e in resource?.Ids)
            {
                var exclusion = _importListExclusionService.GetById(e);
                _importListExclusionService.RemoveExclusion(exclusion);
                _exclusionResourceCache.Remove($"{e}");
            }

            return new { };
        }

        private ImportListExclusionResource GetExclusionResource(int id)
        {
            var ids = new List<int> { id };
            return GetExclusionResources(ids).FirstOrDefault();
        }

        private List<ImportListExclusionResource> GetExclusionResources()
        {
            var allIds = _importListExclusionService.AllIds();
            return GetExclusionResources(allIds);
        }

        private List<ImportListExclusionResource> GetExclusionResources(List<int> ids)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            _logger.Trace($"ImportListExclusionResource: {ids.Count} exclusions");

            var exclusionResources = new List<ImportListExclusionResource>();

            var missingIds = new List<int>();
            foreach (var id in ids)
            {
                var exclusionResource = _exclusionResourceCache.Find($"{id}");
                if (exclusionResource == null)
                {
                    missingIds.Add(id);
                }
                else
                {
                    exclusionResources.AddIfNotNull(exclusionResource);
                }
            }

            if (missingIds.Count > 0)
            {
                var releaseLock = false;
                var getIds = new List<int>();

                try
                {
                    _logger.Info($"Caching {missingIds.Count} exclusions with {_exclusionResourceCache.Lock.CurrentCount} available threads.");

                    // If there are a large number of missing IDs, acquire the lock to prevent cache stampede
                    if (missingIds.Count > 100)
                    {
                        _exclusionResourceCache.Lock.Wait();
                        releaseLock = true;
                        if (stopwatch.Elapsed.TotalSeconds > 2)
                        {
                            _logger.Warn($"Locked exclusion cache for {stopwatch.Elapsed.TotalSeconds} seconds");
                        }

                        // recheck after acquiring the lock
                        foreach (var id in missingIds)
                        {
                            var exclusionResource = _exclusionResourceCache.Find($"{id}");
                            if (exclusionResource == null)
                            {
                                getIds.Add(id);
                            }
                            else
                            {
                                exclusionResources.AddIfNotNull(exclusionResource);
                            }
                        }
                    }
                    else
                    {
                        getIds = missingIds;
                    }

                    if (getIds.Count > 0)
                    {
                        var exclusions = _importListExclusionService.GetByIds(getIds);

                        foreach (var exclusion in exclusions)
                        {
                            exclusionResources.AddIfNotNull(exclusion.ToResource());
                        }

                        foreach (var exclusionResource in exclusionResources)
                        {
                            _exclusionResourceCache.Set($"{exclusionResource.Id}", exclusionResource);
                        }
                    }
                }
                finally
                {
                    stopwatch.Stop();
                    if (releaseLock)
                    {
                        _exclusionResourceCache.Lock.Release();
                    }
                }
            }

            if (stopwatch.Elapsed.TotalSeconds > 60)
            {
                _logger.Warn($"Processed exclusion cache for {ids.Count} after {stopwatch.Elapsed.TotalSeconds} seconds");
            }

            return exclusionResources;
        }
    }
}
