using System.Collections.Generic;
using System.Dynamic;
using Newtonsoft.Json;

namespace NzbDrone.Core.ImportLists.StashDB
{
    public class QueryScenesResult
    {
        [JsonProperty("data")]
        public QuerySceneData Data { get; set; }
    }

    public class QuerySceneData
    {
        [JsonProperty("queryScenes")]
        public QueryScene QueryScenes { get; set; }
    }

    public class QueryScene
    {
        [JsonProperty("scenes")]
        public List<Scene> Scenes { get; set; }
        [JsonProperty("count")]
        public int Count { get; set; }
    }

    public class Scene
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("release_date")]
        public string ReleaseDate { get; set; }
    }

    public class QuerySceneQueryVariablesBase
    {
        [JsonProperty("input")]
        public dynamic Input { get; set; }

        public QuerySceneQueryVariablesBase(int page, int pageSize, SceneSort sort)
        {
            Input = new ExpandoObject();
            Input.page = page;
            Input.per_page = pageSize;
            Input.sort = sort;
        }
    }
}
