using System.Collections.Generic;

namespace NzbDrone.Core.MetadataSource.SkyHook.Resource
{
    public class PerformerResource
    {
        public string Name { get; set; }
        public ExternalIdResource ForeignIds { get; set; }
        public List<ImageResource> Images { get; set; }
        public string Gender { get; set; }
    }
}
