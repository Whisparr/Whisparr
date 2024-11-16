using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

        private bool SupportsTmdbSearch
        {
            get
            {
                var capabilities = _capabilitiesProvider.GetCapabilities(Settings);

                return capabilities.SupportedMovieSearchParameters != null &&
                       capabilities.SupportedMovieSearchParameters.Contains("tmdbid");
            }
        }

        private bool SupportsAggregatedIdSearch
        {
            get
            {
                var capabilities = _capabilitiesProvider.GetCapabilities(Settings);

                return capabilities.SupportsAggregateIdSearch;
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

        private string MovieTextSearchEngine
        {
            get
            {
                var capabilities = _capabilitiesProvider.GetCapabilities(Settings);

                return capabilities.MovieTextSearchEngine;
            }
        }

        public virtual IndexerPageableRequestChain GetRecentRequests()
        {
            var pageableRequests = new IndexerPageableRequestChain();

            var capabilities = _capabilitiesProvider.GetCapabilities(Settings);

            // Some indexers might forget to enable movie search, but normal search still works fine. Thus we force a normal search.
            if (capabilities.SupportedMovieSearchParameters != null)
            {
                pageableRequests.Add(GetPagedRequests(MaxPages, Settings.Categories, "movie", ""));
            }
            else if (capabilities.SupportedSearchParameters != null)
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

        public virtual IndexerPageableRequestChain GetSearchRequests(SceneSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            if (SupportsSearch)
            {
                var raw = TextSearchEngine == "raw";
                var queryTitles = raw ? searchCriteria.SceneTitles : searchCriteria.CleanSceneTitles;

                var releaseDateStrings = new Dictionary<string, string>();
                if (searchCriteria.ReleaseDate != null)
                {
                    var releaseDateValue = searchCriteria.ReleaseDate?.ToString("yy.MM.dd") ?? string.Empty;
                    var releaseDateKey = raw ? releaseDateValue : releaseDateValue.Replace(".", "");

                    releaseDateStrings.Add(releaseDateKey, releaseDateValue);

                    releaseDateValue = searchCriteria.ReleaseDate?.ToString("dd.MM.yyyy") ?? string.Empty;
                    releaseDateKey = raw ? releaseDateValue : releaseDateValue.Replace(".", "");
                    releaseDateStrings.Add(releaseDateKey, releaseDateValue);
                }

                foreach (var queryTitle in queryTitles)
                {
                    pageableRequests.Add(GetPagedRequests(MaxPages,
                        Settings.Categories,
                        "search",
                        $"&q={NewsnabifyTitle(queryTitle, releaseDateStrings)}"));
                }
            }

            return pageableRequests;
        }

        private void AddMovieIdPageableRequests(IndexerPageableRequestChain chain, int maxPages, IEnumerable<int> categories, SearchCriteriaBase searchCriteria)
        {
            var includeTmdbSearch = SupportsTmdbSearch && searchCriteria.Movie.MovieMetadata.Value.TmdbId > 0;

            if (SupportsAggregatedIdSearch && includeTmdbSearch)
            {
                var ids = "";

                if (includeTmdbSearch)
                {
                    ids += $"&tmdbid={searchCriteria.Movie.MovieMetadata.Value.ForeignId}";
                }

                chain.Add(GetPagedRequests(maxPages, categories, "movie", ids));
            }
            else
            {
                if (includeTmdbSearch)
                {
                    chain.Add(GetPagedRequests(maxPages,
                        categories,
                        "movie",
                        $"&tmdbid={searchCriteria.Movie.MovieMetadata.Value.ForeignId}"));
                }
            }

            if (SupportsSearch)
            {
                chain.AddTier();
                var queryTitles = TextSearchEngine == "raw" ? searchCriteria.SceneTitles : searchCriteria.CleanSceneTitles;
                foreach (var queryTitle in queryTitles)
                {
                    var searchQuery = queryTitle;

                    chain.Add(GetPagedRequests(MaxPages,
                        Settings.Categories,
                        "search",
                        $"&q={NewsnabifyTitle(searchQuery, null)}"));
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

            var baseUrl = $"{Settings.BaseUrl.TrimEnd('/')}{Settings.ApiPath.TrimEnd('/')}?t={searchType}&cat={categoriesQuery}&extended=1{Settings.AdditionalParameters}";

            if (Settings.ApiKey.IsNotNullOrWhiteSpace())
            {
                baseUrl += "&apikey=" + Settings.ApiKey;
            }

            if (PageSize == 0)
            {
                yield return new IndexerRequest($"{baseUrl}{parameters}", HttpAccept.Rss);
            }
            else
            {
                for (var page = 0; page < maxPages; page++)
                {
                    yield return new IndexerRequest($"{baseUrl}&offset={page * PageSize}&limit={PageSize}{parameters}", HttpAccept.Rss);
                }
            }
        }

        private static string NewsnabifyTitle(string title, Dictionary<string, string> releaseDateStrings)
        {
            var newtitle = title.Replace("+", " ");

            // Format the query with the date correctly
            if (releaseDateStrings != null)
            {
                foreach (var releaseDateString in releaseDateStrings)
                {
                    if (releaseDateString.Key.IsNotNullOrWhiteSpace() && releaseDateString.Value.IsNotNullOrWhiteSpace())
                    {
                        newtitle = Regex.Replace(newtitle, $"{releaseDateString.Key}", $"{releaseDateString.Value}");
                    }
                }
            }

            newtitle = Uri.EscapeDataString(newtitle);

            return newtitle;
        }

        public Func<IDictionary<string, string>> GetCookies { get; set; }
        public Action<IDictionary<string, string>, DateTime?> CookiesUpdater { get; set; }
    }
}
