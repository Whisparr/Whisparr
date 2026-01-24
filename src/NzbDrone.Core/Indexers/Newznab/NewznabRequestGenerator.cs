using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Indexers.Newznab
{
    public class NewznabRequestGenerator : IIndexerRequestGenerator
    {
        private readonly Logger _logger;
        private readonly INewznabCapabilitiesProvider _capabilitiesProvider;

        public ProviderDefinition Definition { get; set; }
        public int MaxPages { get; set; }
        public int PageSize { get; set; }
        public NewznabSettings Settings { get; set; }

        public NewznabRequestGenerator(INewznabCapabilitiesProvider capabilitiesProvider)
        {
            _logger = NzbDroneLogger.GetLogger(GetType());
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

        public virtual IndexerPageableRequestChain GetSearchRequests(SingleEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            if (SupportsSearch)
            {
                var queryTitles = TextSearchEngine == "raw" ? searchCriteria.AllSceneTitles : searchCriteria.CleanSceneTitles;

                // Pre-calculate commonly used values to avoid repeated processing
                var dateFormats = GetDateFormats(searchCriteria.ReleaseDate, Settings.DateSearchFormat);
                var episodeTitle = Settings.SearchTitleOnly ? NewsnabifyTitle(searchCriteria.EpisodeTitle) : null;
                var sitePlusEpisodeTitle = Settings.SearchSiteTitleOnly ? NewsnabifyTitle(searchCriteria.EpisodeTitle) : null;

                foreach (var queryTitle in queryTitles)
                {
                    var normalizedQueryTitle = NewsnabifyTitle(queryTitle);

                    // ALWAYS do default search with site + date
                    foreach (var dateFormat in dateFormats)
                    {
                        var searchQuery = $"&q={normalizedQueryTitle}+{dateFormat}";
                        pageableRequests.Add(GetPagedRequests(MaxPages,
                            Settings.Categories,
                            "search",
                            searchQuery));
                    }

                    // Add Site + Title search if selected
                    if (Settings.SearchSiteTitleOnly)
                    {
                        // Use Site name
                        var siteTitle = NewsnabifyTitle(searchCriteria.Series.Title);
                        var searchQuery = $"&q={siteTitle}+{sitePlusEpisodeTitle}";
                        pageableRequests.Add(GetPagedRequests(MaxPages, Settings.Categories, "search", searchQuery));

                        // Add network search if needed
                        AddNetworkSearchIfNeeded(pageableRequests, searchCriteria.Series, sitePlusEpisodeTitle, "Episode");
                    }

                    // Add Title Only search if selected
                    if (Settings.SearchTitleOnly)
                    {
                        var searchQuery = $"&q={episodeTitle}";
                        pageableRequests.Add(GetPagedRequests(MaxPages,
                            Settings.Categories,
                            "search",
                            searchQuery));
                    }

                    // Add External ID search if selected and available
                    if (Settings.SearchExternalIdOnly && !string.IsNullOrWhiteSpace(searchCriteria.ExternalId))
                    {
                        var externalId = NewsnabifyTitle(searchCriteria.ExternalId);
                        var searchQuery = $"&q={externalId}";
                        pageableRequests.Add(GetPagedRequests(MaxPages,
                            Settings.Categories,
                            "search",
                            searchQuery));
                    }
                }
            }

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(SeasonSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();
            if (SupportsSearch)
            {
var queryTitles = TextSearchEngine == "raw" ? searchCriteria.AllSceneTitles : searchCriteria.CleanSceneTitles;

                // Pre-calculate commonly used values to avoid repeated processing
                var twoDigitYear = new DateTime(searchCriteria.Year, 1, 31).ToString("yy");

                foreach (var queryTitle in queryTitles)
                {
                    var normalizedQueryTitle = NewsnabifyTitle(queryTitle);

                    // ALWAYS do default search with site + year
                    var searchQuery1 = $"&q={normalizedQueryTitle}+{twoDigitYear}";
                    var searchQuery2 = $"&q={normalizedQueryTitle}+{searchCriteria.Year}";

                    pageableRequests.Add(GetPagedRequests(MaxPages,
                        Settings.Categories,
                        "search",
                        searchQuery1));

                    pageableRequests.Add(GetPagedRequests(MaxPages,
                        Settings.Categories,
                        "search",
                        searchQuery2));

                    // Add Site + Title search if selected
                    if (Settings.SearchSiteTitleOnly)
                    {
                        // Use Site name
                        var siteTitle = NewsnabifyTitle(searchCriteria.Series.Title);
                        var searchQuery = $"&q={siteTitle}+{normalizedQueryTitle}";
                        pageableRequests.Add(GetPagedRequests(MaxPages, Settings.Categories, "search", searchQuery));

                        // Add network search if needed
                        AddNetworkSearchIfNeeded(pageableRequests, searchCriteria.Series, normalizedQueryTitle, "Season");
                    }

                    // Add Title Only search if selected
                    if (Settings.SearchTitleOnly)
                    {
                        var searchQuery = $"&q={normalizedQueryTitle}";
                        pageableRequests.Add(GetPagedRequests(MaxPages,
                            Settings.Categories,
                            "search",
                            searchQuery));
                    }
                }
            }

            return pageableRequests;
        }

        private IEnumerable<IndexerRequest> GetPagedRequests(int maxPages, IEnumerable<int> categories, string searchType, string parameters)
        {
            if (categories.Empty())
            {
                yield break;
            }

            var categoriesQuery = string.Join(",", categories.Distinct());

            var baseUrl =
                $"{Settings.BaseUrl.TrimEnd('/')}{Settings.ApiPath.TrimEnd('/')}?t={searchType}&cat={categoriesQuery}&extended=1{Settings.AdditionalParameters}";

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

        private static string NewsnabifyTitle(string title)
        {
            title = title.Replace("+", " ");
            return Uri.EscapeDataString(title);
        }

        private static string FormatDate(DateOnly? releaseDate, DateSearchFormat format)
        {
            if (!releaseDate.HasValue)
            {
                return string.Empty;
            }

            return format switch
            {
                DateSearchFormat.YearMonthDay => releaseDate.Value.ToString("yy.MM.dd"),
                DateSearchFormat.DayMonthYear => releaseDate.Value.ToString("dd.MM.yy"),
                _ => releaseDate.Value.ToString("yy.MM.dd")
            };
        }

        private static List<string> GetDateFormats(DateOnly? releaseDate, DateSearchFormat format)
        {
            if (!releaseDate.HasValue)
            {
                return new List<string>();
            }

            return format switch
            {
                DateSearchFormat.YearMonthDay => new List<string> { releaseDate.Value.ToString("yy.MM.dd") },
                DateSearchFormat.DayMonthYear => new List<string> { releaseDate.Value.ToString("dd.MM.yy") },
                DateSearchFormat.Both => new List<string>
                {
                    releaseDate.Value.ToString("yy.MM.dd"),
                    releaseDate.Value.ToString("dd.MM.yy")
                },
                _ => new List<string> { releaseDate.Value.ToString("yy.MM.dd") }
            };
        }

        private void AddNetworkSearchIfNeeded(IndexerPageableRequestChain pageableRequests, Tv.Series series, string additionalTitle, string logPrefix)
        {
            if ((Settings.SeriesNameSource == SeriesNameSource.Network || Settings.SeriesNameSource == SeriesNameSource.Both)
                && !string.IsNullOrWhiteSpace(series.Network))
            {
                var networkTitle = NewsnabifyTitle(series.Network);
                var networkSearchQuery = $"&q={networkTitle}+{additionalTitle}";
                pageableRequests.Add(GetPagedRequests(MaxPages, Settings.Categories, "search", networkSearchQuery));
            }
        }
    }
}
