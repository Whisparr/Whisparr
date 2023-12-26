using System.Collections.Generic;

namespace NzbDrone.Core.MetadataSource.SkyHook.Resource
{
    public class Credits
    {
        public List<CastResource> Cast { get; set; }
    }

    public class CastResource
    {
        public int Order { get; set; }
        public string Character { get; set; }
        public string CreditId { get; set; }
        public PerformerResource Performer { get; set; }
    }
}
