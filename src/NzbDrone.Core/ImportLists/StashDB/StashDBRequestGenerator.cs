using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using NLog;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.StashDB
{
    public class StashDBRequestGenerator : IImportListRequestGenerator
    {
        public StashDBRequestGenerator(int pageSize, int maxResultsPerQuery)
        {
            _pageSize = pageSize;
            _maxResultsPerQuery = maxResultsPerQuery;
        }

        private readonly int _pageSize;
        private readonly int _maxResultsPerQuery;
        public StashDBSettings Settings { get; set; }
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
            Logger.Info($"Importing StashDB scenes from favorites: {Settings.Filter}");

            var querySceneQuery = new QuerySceneQuery(1, _pageSize, Settings.Filter, Settings.Sort);

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
    }
}
