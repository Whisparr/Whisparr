using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.WhisparrList
{
    public class WhisparrListRequestGenerator : IImportListRequestGenerator
    {
        public WhisparrListSettings Settings { get; set; }
        public IHttpClient HttpClient { get; set; }
        public Logger Logger { get; set; }

        public virtual ImportListPageableRequestChain GetMovies()
        {
            var pageableRequests = new ImportListPageableRequestChain();

            var request = new ImportListRequest(Settings.Url, HttpAccept.Json);

            request.HttpRequest.SuppressHttpError = true;

            pageableRequests.Add(new List<ImportListRequest> { request });
            return pageableRequests;
        }
    }
}
