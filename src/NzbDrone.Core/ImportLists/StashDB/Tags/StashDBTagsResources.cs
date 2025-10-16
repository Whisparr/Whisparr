using System.Collections.Generic;
using Newtonsoft.Json;

namespace NzbDrone.Core.ImportLists.StashDB.Studio
{
    public class QueryTagsSceneQuery
    {
        private QueryTagsSceneQueryVariables _variables;
        private string _query;

        public QueryTagsSceneQuery(int page, int pageSize, List<string> tags, FilterModifier tagsFilter, SceneSort sort, string afterDate)
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
            _variables = new QueryTagsSceneQueryVariables(page, pageSize, tags, tagsFilter, sort, afterDate);
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

    public class QueryTagsSceneQueryVariables : QuerySceneQueryVariablesBase
    {
        public QueryTagsSceneQueryVariables(int page, int pageSize, List<string> tags, FilterModifier tagsFilter, SceneSort sort, string afterDate)
            : base(page, pageSize, sort, afterDate)
        {
            if (tags.Count > 0)
            {
                Input.tags = new FilterType(tagsFilter, tags);
            }

            Input.sort = sort;
        }
    }
}
