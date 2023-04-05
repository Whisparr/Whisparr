using System.Collections.Generic;

namespace NzbDrone.Core.MetadataSource.SkyHook.Resource
{
    public class MovieResource
    {
        public int ForeignId { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; }
        public string Overview { get; set; }
        public string Studio { get; set; }
        public int? Duration { get; set; }
        public List<ImageResource> Images { get; set; }
    }
}
