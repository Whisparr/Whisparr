using NzbDrone.Core.Extras.Metadata;

namespace Whisparr.Api.V3.Metadata
{
    public class MetadataBulkResource : ProviderBulkResource<MetadataBulkResource>
    {
    }

    public class MetadataBulkResourceMapper : ProviderBulkResourceMapper<MetadataBulkResource, MetadataDefinition>
    {
    }
}
