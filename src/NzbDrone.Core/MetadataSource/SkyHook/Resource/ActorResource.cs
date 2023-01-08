using System.Collections.Generic;

namespace NzbDrone.Core.MetadataSource.SkyHook.Resource
{
    public class ActorResource
    {
        public string Name { get; set; }
        public string Character { get; set; }
        public List<ImageResource> Images { get; set; }
    }
}
