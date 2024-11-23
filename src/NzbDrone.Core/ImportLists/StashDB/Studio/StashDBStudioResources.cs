using System.Collections.Generic;
using Newtonsoft.Json;

namespace NzbDrone.Core.ImportLists.StashDB.Studio
{
    public class QueryStudioSceneQuery
    {
        private QueryStudioSceneQueryVariables _variables;
        private string _query;

        public QueryStudioSceneQuery(int page, int pageSize, List<string> studios, List<string> tags, FilterModifier tagsFilter, bool onlyFavoriteStudios, SceneSort sort, string afterDate)
        {
            _query = @"query Scenes($input: SceneQueryInput!) {
                         queryScenes(input: $input) {
                           scenes {
                             id
                             title
                             release_date
                           }
                           count
                         }
                        }";
            _variables = new QueryStudioSceneQueryVariables(page, pageSize, studios, tags, tagsFilter, onlyFavoriteStudios, sort, afterDate);
        }

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

        public void SetPage(int page)
        {
            _variables.Input.page = page;
        }
    }

    public class QueryStudioSceneQueryVariables : QuerySceneQueryVariablesBase
    {
        public QueryStudioSceneQueryVariables(int page, int pageSize, List<string> studios, List<string> tags, FilterModifier tagsFilter, bool onlyFavoritePerformers, SceneSort sort, string afterDate)
            : base(page, pageSize, sort, afterDate)
        {
            Input.studios = new FilterType(FilterModifier.INCLUDES, studios);

            if (tags.Count > 0)
            {
                Input.tags = new FilterType(tagsFilter, tags);
            }

            if (onlyFavoritePerformers)
            {
                Input.favorites = FavoriteFilter.PERFORMER;
            }

            Input.sort = sort;
        }
    }
}
