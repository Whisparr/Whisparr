using Newtonsoft.Json;

namespace NzbDrone.Core.ImportLists.TPDb
{
    public class PerformerScene
    {
        public int SiteId { get; set; }
        public int EpisodeId { get; set; }

        [JsonProperty("site")]
        public string SiteName { get; set; }
    }
}
