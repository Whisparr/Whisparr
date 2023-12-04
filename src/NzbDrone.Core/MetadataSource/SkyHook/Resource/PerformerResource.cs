using System.Collections.Generic;
using NzbDrone.Core.Movies.Performers;

namespace NzbDrone.Core.MetadataSource.SkyHook.Resource
{
    public class PerformerResource
    {
        public string Name { get; set; }
        public ExternalIdResource ForeignIds { get; set; }
        public List<ImageResource> Images { get; set; }
        public string Gender { get; set; }
        public string Ethnicity { get; set; }
        public PerformerStatus Status { get; set; }
        public int? Age { get; set; }
        public int? CareerStart { get; set; }
        public int? CareerEnd { get; set; }
        public string HairColor { get; set; }
    }
}
