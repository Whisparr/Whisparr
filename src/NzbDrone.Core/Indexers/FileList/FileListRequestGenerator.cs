using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.IndexerSearch.Definitions;

namespace NzbDrone.Core.Indexers.FileList
{
    public class FileListRequestGenerator : IIndexerRequestGenerator
    {
        public FileListSettings Settings { get; set; }

        public virtual IndexerPageableRequestChain GetRecentRequests()
        {
            var pageableRequests = new IndexerPageableRequestChain();

            pageableRequests.Add(GetRequest("latest-torrents", Settings.Categories, ""));

            return pageableRequests;
        }

        public IndexerPageableRequestChain GetSearchRequests(SingleEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            AddNameRequests(pageableRequests, searchCriteria, "search-torrents", Settings.Categories, string.Empty);

            return pageableRequests;
        }

        public IndexerPageableRequestChain GetSearchRequests(SeasonSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            AddNameRequests(pageableRequests, searchCriteria, "search-torrents", Settings.Categories, $"&season={searchCriteria.SeasonNumber}");

            return pageableRequests;
        }

        private void AddImdbRequests(IndexerPageableRequestChain chain, SearchCriteriaBase searchCriteria, string searchType, IEnumerable<int> categories, string parameters)
        {
            if (searchCriteria.Series.ImdbId.IsNotNullOrWhiteSpace())
            {
                chain.Add(GetRequest(searchType, categories, string.Format("&type=imdb&query={0}{1}", searchCriteria.Series.ImdbId, parameters)));
            }
        }

        private void AddNameRequests(IndexerPageableRequestChain chain, SearchCriteriaBase searchCriteria, string searchType, IEnumerable<int> categories, string parameters)
        {
            foreach (var sceneTitle in searchCriteria.SceneTitles)
            {
                chain.Add(GetRequest(searchType, categories, string.Format("&type=name&query={0}{1}", Uri.EscapeDataString(sceneTitle.Trim()), parameters)));
            }
        }

        private IEnumerable<IndexerRequest> GetRequest(string searchType, IEnumerable<int> categories, string parameters)
        {
            var categoriesQuery = string.Join(",", categories.Distinct());

            var baseUrl = string.Format("{0}/api.php?action={1}&category={2}{3}", Settings.BaseUrl.TrimEnd('/'), searchType, categoriesQuery, parameters);

            var request = new IndexerRequest(baseUrl, HttpAccept.Json);
            request.HttpRequest.Credentials = new BasicNetworkCredential(Settings.Username.Trim(), Settings.Passkey.Trim());

            yield return request;
        }
    }
}
