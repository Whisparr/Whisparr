using System.Collections.Generic;
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
        [JsonProperty("performers")]
        public List<Performer> Performers { get; set; }
        [JsonProperty("release_date")]
        public string ReleaseDate { get; set; }
    }

    public class Performer
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class QuerySceneQuery
    {
        private QuerySceneQueryVariables _variables;
        private string _query;
        public string Query
        {
            get
            {
                return _query;
            }
        }

        public string Variables
        {
            get
            {
                return JsonConvert.SerializeObject(_variables);
            }
        }

        public QuerySceneQuery(int page, int pageSize, FavoriteFilter filter, SceneSort sort)
        {
            _query = @"
                        query QueryScenes($sort: SceneSortEnum!, $page: Int!, $perPage: Int!, $favorites: FavoriteFilter!) {
                            queryScenes(
                                input: { sort: $sort, direction: DESC, favorites: $favorites, per_page: $perPage, page: $page }
                            ) {
                                scenes {
                                    id
                                    title
                                    performers {
                                        performer {
                                            name
                                        }
                                    }
                                    release_date
                                }
                                count
                            }
                        }
                        ";
            _variables = new QuerySceneQueryVariables(page, pageSize, filter, sort);
        }

        public void SetPage(int page)
        {
            _variables.Page = page;
        }
    }

    public class QuerySceneQueryVariables
    {
        [JsonProperty("page")]
        public int Page { get; set; }

        [JsonProperty("perPage")]
        public int PageSize { get; set; }

        [JsonProperty("favorites")]
        public FavoriteFilter Filter { get; set; }

        [JsonProperty("sort")]
        public SceneSort Sort { get; set; }

        public QuerySceneQueryVariables(int page, int pageSize, FavoriteFilter filter, SceneSort sort)
        {
            Page = page;
            PageSize = pageSize;
            Filter = filter;
            Sort = sort;
        }
    }
}
