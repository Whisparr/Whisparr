using System.Collections.Generic;
using Newtonsoft.Json;

namespace NzbDrone.Core.Notifications.Stash
{
    public class StashPerformer
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool Favorite { get; set; }

        [JsonProperty("stash_ids")]
        public List<StashPerformerId> StashIds { get; set; } = new List<StashPerformerId>();
    }

    public class StashPerformerId
    {
        public string Endpoint { get; set; }

        [JsonProperty("stash_id")]
        public string StashId { get; set; }
    }

    public class StashPerformerPage
    {
        public int Count { get; set; }
        public List<StashPerformer> Performers { get; set; } = new List<StashPerformer>();
    }

    public class StashFindPerformersData
    {
        public StashPerformerPage FindPerformers { get; set; }
    }

    public class StashPerformerMutationData
    {
        public StashPerformer PerformerUpdate { get; set; }
        public StashPerformer PerformerCreate { get; set; }
    }

    public class StashGraphQlResponse<T>
    {
        public T Data { get; set; }
        public List<StashGraphQlError> Errors { get; set; }
    }

    public class StashGraphQlError
    {
        public string Message { get; set; }
    }
}
