using System.Collections.Generic;
using Newtonsoft.Json;

namespace NzbDrone.Core.ImportLists.StashDB.Performer
{
    public class QueryPerformerSceneQuery
    {
        private QueryPerformerSceneQueryVariables _variables;
        private string _query;

        public QueryPerformerSceneQuery(int page, int pageSize, List<string> performers, List<string> studios, List<string> tags, bool onlyFavoriteStudios, SceneSort sort)
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
            _variables = new QueryPerformerSceneQueryVariables(page, pageSize, performers, studios, tags, onlyFavoriteStudios, sort);
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

    public class QueryPerformerSceneQueryVariables : QuerySceneQueryVariablesBase
    {
        public QueryPerformerSceneQueryVariables(int page, int pageSize, List<string> performers, List<string> studios, List<string> tags, bool onlyFavoriteStudios, SceneSort sort)
            : base(page, pageSize, sort)
        {
            Input.performers = new FilterType(performers);
            if (studios.Count > 0)
            {
                Input.studios = new FilterType(studios);
            }

            if (tags.Count > 0)
            {
                Input.tags = new FilterType(tags);
            }

            if (onlyFavoriteStudios)
            {
                Input.favorites = FavoriteFilter.STUDIO;
            }
        }
    }
}
