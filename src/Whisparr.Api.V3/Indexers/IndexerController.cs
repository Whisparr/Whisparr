using NzbDrone.Core.Indexers;
using Whisparr.Http;

namespace Whisparr.Api.V3.Indexers
{
    [V3ApiController]
    public class IndexerController : ProviderControllerBase<IndexerResource, IIndexer, IndexerDefinition>
    {
        public static readonly IndexerResourceMapper ResourceMapper = new IndexerResourceMapper();

        public IndexerController(IndexerFactory indexerFactory)
            : base(indexerFactory, "indexer", ResourceMapper)
        {
        }
    }
}
