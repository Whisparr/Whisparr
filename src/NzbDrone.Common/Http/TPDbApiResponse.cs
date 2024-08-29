using System.Collections.Generic;

namespace NzbDrone.Common.Http
{
    public class TPDbAPIResponse<TResource>
    {
        public List<TResource> Data { get; set; }

        // public Dictionary<string, string> Links { get; set; }

        // public Dictionary<string, string> Meta { get; set; }
    }
}
