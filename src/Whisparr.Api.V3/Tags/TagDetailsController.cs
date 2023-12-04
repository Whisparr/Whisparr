using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Tags;
using Whisparr.Http;
using Whisparr.Http.REST;

namespace Whisparr.Api.V3.Tags
{
    [V3ApiController("tag/detail")]
    public class TagDetailsController : RestController<TagDetailsResource>
    {
        private readonly ITagService _tagService;

        public TagDetailsController(ITagService tagService)
        {
            _tagService = tagService;
        }

        protected override TagDetailsResource GetResourceById(int id)
        {
            return _tagService.Details(id).ToResource();
        }

        [HttpGet]
        public List<TagDetailsResource> GetAll()
        {
            return _tagService.Details().ToResource();
        }
    }
}
