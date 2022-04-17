using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.IndexerSearch.Definitions;

namespace NzbDrone.Core.Indexers.Newznab
{
    public class NewznabRequestGenerator : IIndexerRequestGenerator
    {
        private readonly INewznabCapabilitiesProvider _capabilitiesProvider;
        public int MaxPages { get; set; }
        public int PageSize { get; set; }
        public NewznabSettings Settings { get; set; }

        public NewznabRequestGenerator(INewznabCapabilitiesProvider capabilitiesProvider)
        {
            _capabilitiesProvider = capabilitiesProvider;

            MaxPages = 30;
            PageSize = 100;
        }

        private bool SupportsSearch
        {
            get
            {
                var capabilities = _capabilitiesProvider.GetCapabilities(Settings);

                return capabilities.SupportedSearchParameters != null &&
                       capabilities.SupportedSearchParameters.Contains("q");
            }
        }

        private string TextSearchEngine
        {
            get
            {
                var capabilities = _capabilitiesProvider.GetCapabilities(Settings);

                return capabilities.TextSearchEngine;
            }
        }

        public virtual IndexerPageableRequestChain GetRecentRequests()
        {
            var pageableRequests = new IndexerPageableRequestChain();

            var capabilities = _capabilitiesProvider.GetCapabilities(Settings);

            if (capabilities.SupportedSearchParameters != null)
            {
                pageableRequests.Add(GetPagedRequests(MaxPages, Settings.Categories, "search", ""));
            }

            return pageableRequests;
        }

        public IndexerPageableRequestChain GetSearchRequests(MovieSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            AddMovieIdPageableRequests(pageableRequests, MaxPages, Settings.Categories, searchCriteria);

            return pageableRequests;
        }

        private void AddMovieIdPageableRequests(IndexerPageableRequestChain chain, int maxPages, IEnumerable<int> categories, SearchCriteriaBase searchCriteria)
        {
            if (SupportsSearch)
            {
                chain.AddTier();
                var queryTitles = TextSearchEngine == "raw" ? searchCriteria.SceneTitles : searchCriteria.CleanSceneTitles;
                foreach (var queryTitle in queryTitles)
                {
                    var searchQuery = queryTitle;

                    searchQuery = string.Format("{0} {1}", searchQuery, searchCriteria.Movie.Year);

                    chain.Add(GetPagedRequests(MaxPages,
                        Settings.Categories,
                        "search",
                        string.Format("&q={0}", NewsnabifyTitle(searchQuery))));
                }

                chain.AddTier();
                foreach (var queryTitle in queryTitles)
                {
                    var searchQuery = queryTitle;

                    chain.Add(GetPagedRequests(MaxPages,
                        Settings.Categories,
                        "search",
                        string.Format("&q={0}", NewsnabifyTitle(searchQuery))));
                }
            }
        }

        private IEnumerable<IndexerRequest> GetPagedRequests(int maxPages, IEnumerable<int> categories, string searchType, string parameters)
        {
            if (categories.Empty())
            {
                yield break;
            }

            var categoriesQuery = string.Join(",", categories.Distinct());

            var baseUrl = string.Format("{0}{1}?t={2}&cat={3}&extended=1{4}", Settings.BaseUrl.TrimEnd('/'), Settings.ApiPath.TrimEnd('/'), searchType, categoriesQuery, Settings.AdditionalParameters);

            if (Settings.ApiKey.IsNotNullOrWhiteSpace())
            {
                baseUrl += "&apikey=" + Settings.ApiKey;
            }

            if (PageSize == 0)
            {
                yield return new IndexerRequest(string.Format("{0}{1}", baseUrl, parameters), HttpAccept.Rss);
            }
            else
            {
                for (var page = 0; page < maxPages; page++)
                {
                    yield return new IndexerRequest(string.Format("{0}&offset={1}&limit={2}{3}", baseUrl, page * PageSize, PageSize, parameters), HttpAccept.Rss);
                }
            }
        }

        private static string NewsnabifyTitle(string title)
        {
            var newtitle = title.Replace("+", " ");
            return Uri.EscapeDataString(newtitle);
        }

        public Func<IDictionary<string, string>> GetCookies { get; set; }
        public Action<IDictionary<string, string>, DateTime?> CookiesUpdater { get; set; }
    }
}
