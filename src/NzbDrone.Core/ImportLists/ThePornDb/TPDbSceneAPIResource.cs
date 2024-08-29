using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NzbDrone.Core.ImportLists.TPDb;

namespace NzbDrone.Core.ImportLists.ThePornDb
{
    public class TPDbApiObject
    {
        public List<FeedData> Data { get; set; } = new List<FeedData>();

        public FeedLinks Links { get; set; } = new FeedLinks();

        public FeedMeta Meta { get; set; } = new FeedMeta();

        internal void CombineWith(TPDbApiObject json)
        {
            Data.AddRange(json.Data);
            Links = json.Links;
            Meta = json.Meta;
        }
    }

    public class FeedData
    {
        [JsonProperty("_id")]
        public int SceneId { get; set; }

        [JsonProperty("site_id")]
        public int SiteId { get; set; }
    }

    public class FeedLinks
    {
        public string First { get; set; }
        public string Next { get; set; }
        public string Last { get; set; }
        public string Prev { get; set; }
    }

    public class FeedMeta
    {
        public int Total { get; set; }

        [JsonProperty("last_page")]
        public int LastPage { get; set; }

        [JsonProperty("current_page")]
        public int CurrentPage { get; set; }

        // This is for the links array inside "meta"
        public List<MetaLink> Links { get; set; } = new List<MetaLink>();
    }

    public class MetaLink
    {
        public string Url { get; set; }
        public string Label { get; set; }
        public bool Active { get; set; }
    }

    public static class TPDbApiObjectExtensions
    {
        public static List<PerformerScene> ToPerformerSceneList(this List<FeedData> feedDataList)
        {
            return feedDataList.Select(item => new PerformerScene
            {
                SiteId = item.SiteId,
                EpisodeId = item.SceneId
            }).ToList();
        }

        public static List<PerformerScene> ToPerformerSceneList(this TPDbApiObject feedObject)
        {
            return feedObject.Data.ToPerformerSceneList();
        }
    }
}
