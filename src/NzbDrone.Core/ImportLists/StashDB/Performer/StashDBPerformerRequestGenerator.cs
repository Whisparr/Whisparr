using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.StashDB.Performer
{
    public class StashDBPerformerRequestGenerator : IImportListRequestGenerator
    {
        public StashDBPerformerRequestGenerator(int pageSize, int maxResultsPerQuery)
        {
            _pageSize = pageSize;
            _maxResultsPerQuery = maxResultsPerQuery;
        }

        private readonly int _pageSize;
        private readonly int _maxResultsPerQuery;
        public StashDBPerformerSettings Settings { get; set; }
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
            var parameterLog = string.Empty;

            var performers = SettingToList(Settings.Performers);
            if (performers.Count > 0)
            {
                parameterLog += $"\r\n Performers: {performers.Join(",")}";
            }

            var studios = SettingToList(Settings.Studios);
            if (studios.Count > 0)
            {
                parameterLog += $"\r\n Studios: {studios.Join(",")}";
            }

            var tags = SettingToList(Settings.Tags);
            if (tags.Count > 0)
            {
                parameterLog += $"\r\n Tags: {tags.Join(",")}";
            }

            parameterLog += $"\r\n OnlyFavoriteStudios: {Settings.OnlyFavoriteStudios}";

            Logger.Info($"Importing StashDB scenes for performers: {parameterLog}");

            var querySceneQuery = new QueryPerformerSceneQuery(1, _pageSize, performers, studios, tags, Settings.OnlyFavoriteStudios, Settings.Sort);

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
