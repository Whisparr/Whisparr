using Newtonsoft.Json;

namespace NzbDrone.Core.ImportLists.TPDb
{
    public class PerformerScene
    {
        [JsonProperty("SiteId")]
        public int SiteId { get; set; }

        [JsonProperty("EpisodeId")]
        public int EpisodeId { get; set; }

        [JsonProperty("SiteName")]
        public string SiteName { get; set; }
    }
}
