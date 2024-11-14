using System.Collections.Generic;
using Newtonsoft.Json;

namespace NzbDrone.Core.ImportLists.StashDB.Performer
{
    public class QueryPerformerSceneQuery
    {
        private QueryPerformerSceneQueryVariables _variables;
        private string _query;

        public QueryPerformerSceneQuery(int page, int pageSize, List<string> performers, List<string> studios, FilterModifier studiosFilter, List<string> tags, FilterModifier tagsFilter, bool onlyFavoriteStudios, SceneSort sort)
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
            _variables = new QueryPerformerSceneQueryVariables(page, pageSize, performers, studios, studiosFilter, tags, tagsFilter, onlyFavoriteStudios, sort);
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
        public QueryPerformerSceneQueryVariables(int page, int pageSize, List<string> performers, List<string> studios, FilterModifier studiosFilter, List<string> tags, FilterModifier tagsFilter, bool onlyFavoriteStudios, SceneSort sort)
            : base(page, pageSize, sort)
        {
            Input.performers = new FilterType(FilterModifier.INCLUDES, performers);
            if (studios.Count > 0)
            {
                Input.studios = new FilterType(studiosFilter, studios);
            }

            if (tags.Count > 0)
            {
                Input.tags = new FilterType(tagsFilter, tags);
            }

            if (onlyFavoriteStudios)
            {
                Input.favorites = FavoriteFilter.STUDIO;
            }
        }
    }
}
