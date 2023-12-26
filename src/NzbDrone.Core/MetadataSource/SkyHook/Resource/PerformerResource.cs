using System.Collections.Generic;
using NzbDrone.Core.Movies.Performers;

namespace NzbDrone.Core.MetadataSource.SkyHook.Resource
{
    public class PerformerResource
    {
        public string Name { get; set; }
        public ExternalIdResource ForeignIds { get; set; }
        public List<ImageResource> Images { get; set; }
        public Gender Gender { get; set; }
    }
}
