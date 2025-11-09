using System.Collections.Generic;
using Newtonsoft.Json;
using NLog;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.TMDb.Company
{
    public class TMDbCompanyRequestGenerator : IImportListRequestGenerator
    {
        public TMDbCompanySettings Settings { get; set; }
        public IHttpClient HttpClient { get; set; }
        public IHttpRequestBuilderFactory RequestBuilder { get; set; }
        public Logger Logger { get; set; }

        public virtual ImportListPageableRequestChain GetMovies()
        {
            var pageableRequests = new ImportListPageableRequestChain();

            foreach (var request in GetMoviesRequest())
            {
                pageableRequests.Add(new[] { request }); // Wrap each in its own enumerable (workaround)
            }

            return pageableRequests;
        }

        private IEnumerable<ImportListRequest> GetMoviesRequest()
        {
            Logger.Info($"Importing TMDb movies from company: {Settings.CompanyId}");

            var requestBuilder = RequestBuilder.Create()
                .SetSegment("api", "3")
                .SetSegment("route", "discover")
                .SetSegment("id", $"movie")
                .SetSegment("secondaryRoute", "")
                .AddQueryParam("include_adult", "true")
                .AddQueryParam("include_video", "false")
                .AddQueryParam("language", "en-US")
                .AddQueryParam("sort_by", "popularity.desc")
                .AddQueryParam("with_companies", Settings.CompanyId);

            // Initial request to get total pages
            var initialBuilder = requestBuilder.Clone().AddQueryParam("page", "1");
            var initialRequest = new ImportListRequest(initialBuilder.Accept(HttpAccept.Json).Build());
            var initialResponse = HttpClient.Get(initialRequest.HttpRequest);

            if (initialResponse.HasHttpError)
            {
                Logger.Warn($"TMDb request failed on page 1: {initialResponse.StatusCode}");
                yield break;
            }

            var json = JsonConvert.DeserializeObject<MovieSearchResource>(initialResponse.Content);
            var totalPages = json.TotalPages;
            Logger.Debug($"Total pages to fetch: {totalPages}");

            for (var page = 1; page <= totalPages; page++)
            {
                Logger.Debug($"Building request for page [{page}]");

                var pageBuilder = requestBuilder.Clone().AddQueryParam("page", page.ToString());
                yield return new ImportListRequest(pageBuilder.Accept(HttpAccept.Json).Build());
            }
        }
    }
}
