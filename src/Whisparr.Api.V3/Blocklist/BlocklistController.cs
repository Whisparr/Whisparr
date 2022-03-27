using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Blocklisting;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Datastore;
using Whisparr.Http;
using Whisparr.Http.Extensions;
using Whisparr.Http.REST.Attributes;

namespace Whisparr.Api.V3.Blocklist
{
    [V3ApiController]
    public class BlocklistController : Controller
    {
        private readonly IBlocklistService _blocklistService;
        private readonly ICustomFormatCalculationService _formatCalculator;

        public BlocklistController(IBlocklistService blocklistService,
                               ICustomFormatCalculationService formatCalculator)
        {
            _blocklistService = blocklistService;
            _formatCalculator = formatCalculator;
        }

        [HttpGet]
        public PagingResource<BlocklistResource> GetBlocklist()
        {
            var pagingResource = Request.ReadPagingResourceFromRequest<BlocklistResource>();
            var pagingSpec = pagingResource.MapToPagingSpec<BlocklistResource, NzbDrone.Core.Blocklisting.Blocklist>("date", SortDirection.Descending);

            return pagingSpec.ApplyToPage(_blocklistService.Paged, model => BlocklistResourceMapper.MapToResource(model, _formatCalculator));
        }

        [HttpGet("movie")]
        public List<BlocklistResource> GetMovieBlocklist(int movieId)
        {
            return _blocklistService.GetByMovieId(movieId).Select(h => BlocklistResourceMapper.MapToResource(h, _formatCalculator)).ToList();
        }

        [RestDeleteById]
        public void DeleteBlocklist(int id)
        {
            _blocklistService.Delete(id);
        }

        [HttpDelete("bulk")]
        public object Remove([FromBody] BlocklistBulkResource resource)
        {
            _blocklistService.Delete(resource.Ids);

            return new { };
        }
    }
}
