using System.Collections.Generic;

namespace NzbDrone.Core.MetadataSource.SkyHook.Resource
{
    public class StudioResource
    {
        public string Title { get; set; }
        public string Homepage { get; set; }
        public string Network { get; set; }
        public List<ImageResource> Images { get; set; }
        public ExternalIdResource ForeignIds { get; set; }
    }
}
