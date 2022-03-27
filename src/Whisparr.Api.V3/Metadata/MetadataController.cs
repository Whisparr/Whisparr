using NzbDrone.Core.Extras.Metadata;
using Whisparr.Http;

namespace Whisparr.Api.V3.Metadata
{
    [V3ApiController]
    public class MetadataController : ProviderControllerBase<MetadataResource, IMetadata, MetadataDefinition>
    {
        public static readonly MetadataResourceMapper ResourceMapper = new MetadataResourceMapper();

        public MetadataController(IMetadataFactory metadataFactory)
            : base(metadataFactory, "metadata", ResourceMapper)
        {
        }
    }
}
