using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.StashDB.Favorite
{
    public class StashDBFavoriteRequestGenerator : IImportListRequestGenerator
    {
        public StashDBFavoriteRequestGenerator(int pageSize, int maxResultsPerQuery)
        {
            _pageSize = pageSize;
            _maxResultsPerQuery = maxResultsPerQuery;
        }

        private readonly int _pageSize;
        private readonly int _maxResultsPerQuery;
        public StashDBFavoriteSettings Settings { get; set; }
        public IHttpClient HttpClient { get; set; }
        public IHttpRequestBuilderFactory RequestBuilder { get; set; }
        public Logger Logger { get; set; }
        public virtual ImportListPageableRequestChain GetMovies()
        {
            var pageableRequests = new ImportListPageableRequestChain();

            pageableRequests.Add(GetSceneRequest());

            return pageableRequests;
        }

        private IEnumerable<ImportListRequest> GetSceneRequest()
        {
            var parameterLog = $"Importing StashDB scenes from favorites: {Settings.Filter}";

            var tags = SettingToList(Settings.Tags);
            if (tags.Count > 0)
            {
                parameterLog += $"\r\n Tags: {tags.Join(",")}";
            }

            Logger.Info(parameterLog);

            var querySceneQuery = new QueryFavoriteSceneQuery(1, _pageSize, Settings.Filter, tags, Settings.TagsFilter, Settings.Sort);

            var requestBuilder = RequestBuilder
                                        .Create()
                                        .SetHeader("ApiKey", Settings.ApiKey)
                                        .AddQueryParam("query", querySceneQuery.Query)
                                        .AddQueryParam("variables", querySceneQuery.Variables);

            var jsonResponse = JsonConvert.DeserializeObject<QueryScenesResult>(HttpClient.Execute(requestBuilder.Build()).Content);

            var pagesInResponse = (jsonResponse.Data.QueryScenes.Count / _pageSize) + 1;

            var maxPagesAllowed = _maxResultsPerQuery / _pageSize;

            var pages = Math.Min(pagesInResponse, maxPagesAllowed);

            var requests = new List<ImportListRequest>();

            for (var pageNumber = 1; pageNumber <= pages; pageNumber++)
            {
                querySceneQuery.SetPage(pageNumber);

                requestBuilder.AddQueryParam("variables", querySceneQuery.Variables, true);

                var request = requestBuilder.Build();

                Logger.Debug($"Importing StashDB scenes from {request.Url}");

                requests.Add(new ImportListRequest(request));
            }

            return requests;
        }

        private List<string> SettingToList(string value)
        {
            var list = new List<string>();

            if (!string.IsNullOrEmpty(value?.Trim()))
            {
                list = Array.ConvertAll(value.Split(","), x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToList();
            }

            return list;
        }
    }
}
