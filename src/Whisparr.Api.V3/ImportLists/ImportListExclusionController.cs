using System;
using System.Collections.Generic;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
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

        public ImportListExclusionController(IImportListExclusionService importListExclusionService,
                                             ImportListExclusionExistsValidator importListExclusionExistsValidator)
        {
            _importListExclusionService = importListExclusionService;

            SharedValidator.RuleFor(c => c.ForeignId).Cascade(CascadeMode.Stop)
                .NotEmpty()
                .SetValidator(importListExclusionExistsValidator);

            SharedValidator.RuleFor(c => c.MovieTitle).NotEmpty();
            SharedValidator.RuleFor(c => c.MovieYear).GreaterThan(0);
        }

        [HttpGet]
        [Produces("application/json")]
        [Obsolete("Deprecated")]
        public List<ImportListExclusionResource> GetImportListExclusions()
        {
            return _importListExclusionService.GetAllExclusions().ToResource();
        }

        protected override ImportListExclusionResource GetResourceById(int id)
        {
            return _importListExclusionService.GetById(id).ToResource();
        }

        [HttpGet("paged")]
        [Produces("application/json")]
        public PagingResource<ImportListExclusionResource> GetImportListExclusionsPaged([FromQuery] PagingRequestResource paging)
        {
            var pagingResource = new PagingResource<ImportListExclusionResource>(paging);
            var pageSpec = pagingResource.MapToPagingSpec<ImportListExclusionResource, ImportListExclusion>();

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
        }

        [HttpDelete("bulk")]
        [Produces("application/json")]
        public object DeleteImportListExclusions([FromBody] ImportListExclusionBulkResource resource)
        {
            foreach (var e in resource?.Ids)
            {
                var exclusion = _importListExclusionService.GetById(e);
                _importListExclusionService.RemoveExclusion(exclusion);
            }

            return new { };
        }
    }
}
