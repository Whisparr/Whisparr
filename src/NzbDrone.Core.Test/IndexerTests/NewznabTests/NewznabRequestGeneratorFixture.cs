using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.Newznab;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.IndexerTests.NewznabTests
{
    public class NewznabRequestGeneratorFixture : CoreTest<NewznabRequestGenerator>
    {
        private SingleEpisodeSearchCriteria _singleEpisodeSearchCriteria;
        private SeasonSearchCriteria _seasonSearchCriteria;
        private NewznabCapabilities _capabilities;

        [SetUp]
        public void SetUp()
        {
            Subject.Definition = new IndexerDefinition
            {
                Name = "Newznab"
            };

            Subject.Settings = new NewznabSettings()
            {
                BaseUrl = "http://127.0.0.1:1234/",
                Categories = new[] { 1, 2 },
                ApiKey = "abcd",
            };

            _singleEpisodeSearchCriteria = new SingleEpisodeSearchCriteria
            {
                Series = new Tv.Series { TvdbId = 20, Title = "Monkey Island", Network = "HBO" },
                SceneTitles = new List<string> { "Monkey Island" },
                EpisodeTitle = "Pilot Episode",
                ReleaseDate = new DateOnly(2021, 3, 15)
            };

            _seasonSearchCriteria = new SeasonSearchCriteria
            {
                Series = new Tv.Series { TvdbId = 20, Title = "Monkey Island", Network = "HBO" },
                SceneTitles = new List<string> { "Monkey Island" },
                Year = 2021
            };

            _capabilities = new NewznabCapabilities();

            Mocker.GetMock<INewznabCapabilitiesProvider>()
                .Setup(v => v.GetCapabilities(It.IsAny<NewznabSettings>()))
                .Returns(_capabilities);
        }

        [Test]
        public void should_use_all_categories_for_feed()
        {
            var results = Subject.GetRecentRequests();

            results.GetAllTiers().Should().HaveCount(1);

            var page = results.GetAllTiers().First().First();

            page.Url.Query.Should().Contain("&cat=1,2&");
        }

        [Test]
        public void should_not_have_duplicate_categories()
        {
            Subject.Settings.Categories = new[] { 1, 2, 2, 3 };

            var results = Subject.GetRecentRequests();

            results.GetAllTiers().Should().HaveCount(1);

            var page = results.GetAllTiers().First().First();

            page.Url.FullUri.Should().Contain("&cat=1,2,3&");
        }

        [Test]
        public void should_not_search_by_rid_if_not_supported()
        {
            _capabilities.SupportedSearchParameters = new[] { "q" };

            var results = Subject.GetSearchRequests(_singleEpisodeSearchCriteria);

            results.GetAllTiers().Should().HaveCount(1);

            var page = results.GetAllTiers().First().First();

            page.Url.Query.Should().NotContain("rid=10");
            page.Url.Query.Should().Contain("q=Monkey");
        }

        [Test]
        public void should_not_search_by_tvdbid_if_not_supported()
        {
            _capabilities.SupportedSearchParameters = new[] { "q" };

            var results = Subject.GetSearchRequests(_singleEpisodeSearchCriteria);
            results.GetTier(0).Should().HaveCount(1);

            var page = results.GetAllTiers().First().First();

            page.Url.Query.Should().NotContain("rid=10");
            page.Url.Query.Should().Contain("q=Monkey");
        }

        [Test]
        public void should_not_use_aggregrated_id_search_if_no_ids_supported()
        {
            _capabilities.SupportedSearchParameters = new[] { "q" };
            _capabilities.SupportsAggregateIdSearch = true; // Turns true if indexer supplies supportedParams.

            var results = Subject.GetSearchRequests(_singleEpisodeSearchCriteria);
            results.Tiers.Should().Be(1);
            results.GetTier(0).Should().HaveCount(1);

            var page = results.GetTier(0).First().First();

            page.Url.Query.Should().Contain("q=");
        }

        [Test]
        public void should_encode_raw_title()
        {
            _capabilities.SupportedSearchParameters = new[] { "q" };
            _capabilities.TextSearchEngine = "raw";
            _singleEpisodeSearchCriteria.SceneTitles[0] = "Edith & Little";

            var results = Subject.GetSearchRequests(_singleEpisodeSearchCriteria);
            results.Tiers.Should().Be(1);

            var pageTier = results.GetTier(0).First().First();

            pageTier.Url.Query.Should().Contain("q=Edith%20%26%20Little");
            pageTier.Url.Query.Should().NotContain(" & ");
            pageTier.Url.Query.Should().Contain("%26");
        }

        [Test]
        public void should_use_clean_title_and_encode()
        {
            _capabilities.SupportedSearchParameters = new[] { "q" };
            _capabilities.TextSearchEngine = "sphinx";
            _singleEpisodeSearchCriteria.SceneTitles[0] = "Edith & Little";

            var results = Subject.GetSearchRequests(_singleEpisodeSearchCriteria);
            results.Tiers.Should().Be(1);

            var pageTier = results.GetTier(0).First().First();

            pageTier.Url.Query.Should().Contain("q=Edith%20and%20Little");
            pageTier.Url.Query.Should().Contain("and");
            pageTier.Url.Query.Should().NotContain(" & ");
            pageTier.Url.Query.Should().NotContain("%26");
        }

        [Test]
        public void should_search_title_only_when_enabled()
        {
            _capabilities.SupportedSearchParameters = new[] { "q" };
            Subject.Settings.SearchTitleOnly = true;

            var results = Subject.GetSearchRequests(_singleEpisodeSearchCriteria);
            var requests = results.GetAllTiers().SelectMany(x => x).ToList();

            // Should have default search requests plus title-only search
            requests.Should().HaveCountGreaterThan(0);
            requests.Should().Contain(x => x.Url.Query.Contains("Pilot") && x.Url.Query.Contains("Episode"));
        }

        [Test]
        public void should_search_site_plus_title_when_enabled()
        {
            _capabilities.SupportedSearchParameters = new[] { "q" };
            Subject.Settings.SearchSiteTitleOnly = true;

            var results = Subject.GetSearchRequests(_singleEpisodeSearchCriteria);
            var requests = results.GetAllTiers().SelectMany(x => x).ToList();

            requests.Should().HaveCountGreaterThan(0);

            // Should have some request containing both series and episode info

            requests.Should().Contain(x => x.Url.Query.Contains("Monkey") && x.Url.Query.Contains("Pilot"));
        }

        [Test]
        public void should_use_both_date_formats_when_configured()
        {
            _capabilities.SupportedSearchParameters = new[] { "q" };
            Subject.Settings.DateSearchFormat = DateSearchFormat.Both;

            var results = Subject.GetSearchRequests(_singleEpisodeSearchCriteria);
            var requests = results.GetAllTiers().SelectMany(x => x).ToList();

            requests.Should().HaveCountGreaterThan(0);

            // Should have requests with both YY.MM.DD (21.03.15) and DD.MM.YY (15.03.21) formats
            // The dots might be URL encoded as %2E
            var allQueries = string.Join(" ", requests.Select(x => x.Url.Query));
            allQueries.Should().Match("*21*03*15*"); // YY.MM.DD format
            allQueries.Should().Match("*15*03*21*"); // DD.MM.YY format
        }

        [Test]
        public void should_use_day_month_year_format_when_configured()
        {
            _capabilities.SupportedSearchParameters = new[] { "q" };
            Subject.Settings.DateSearchFormat = DateSearchFormat.DayMonthYear;

            var results = Subject.GetSearchRequests(_singleEpisodeSearchCriteria);
            var requests = results.GetAllTiers().SelectMany(x => x).ToList();

            requests.Should().HaveCountGreaterThan(0);

            // When DateSearchFormat.DayMonthYear is set, should have DD.MM.YY format
            var allQueries = string.Join(" ", requests.Select(x => x.Url.Query));
            allQueries.Should().Match("*15*03*21*"); // Should have DD.MM.YY format
        }

        [Test]
        public void should_use_network_name_when_series_name_source_is_network()
        {
            _capabilities.SupportedSearchParameters = new[] { "q" };
            Subject.Settings.SearchSiteTitleOnly = true;
            Subject.Settings.SeriesNameSource = SeriesNameSource.Network;

            var results = Subject.GetSearchRequests(_singleEpisodeSearchCriteria);
            var requests = results.GetAllTiers().SelectMany(x => x).ToList();

            requests.Should().HaveCountGreaterThan(0);
            requests.Should().Contain(x => x.Url.Query.Contains("HBO"));
        }

        [Test]
        public void should_use_both_network_and_site_when_series_name_source_is_both()
        {
            _capabilities.SupportedSearchParameters = new[] { "q" };
            Subject.Settings.SearchSiteTitleOnly = true;
            Subject.Settings.SeriesNameSource = SeriesNameSource.Both;

            var results = Subject.GetSearchRequests(_singleEpisodeSearchCriteria);
            var requests = results.GetAllTiers().SelectMany(x => x).ToList();

            requests.Should().HaveCountGreaterThan(0);

            // Should have requests with both site name and network name variations
            requests.Should().Contain(x => x.Url.Query.Contains("Monkey"));
            requests.Should().Contain(x => x.Url.Query.Contains("HBO"));
        }

        [Test]
        public void should_search_season_with_title_only_when_enabled()
        {
            _capabilities.SupportedSearchParameters = new[] { "q" };
            Subject.Settings.SearchTitleOnly = true;

            var results = Subject.GetSearchRequests(_seasonSearchCriteria);
            var requests = results.GetAllTiers().SelectMany(x => x).ToList();

            requests.Should().HaveCountGreaterThan(0);

            // For season searches, SearchTitleOnly should include requests with just the series title
            requests.Should().Contain(x => x.Url.Query.Contains("Monkey") && x.Url.Query.Contains("Island"));
        }

        [Test]
        public void should_search_season_with_site_plus_title_when_enabled()
        {
            _capabilities.SupportedSearchParameters = new[] { "q" };
            Subject.Settings.SearchSiteTitleOnly = true;

            var results = Subject.GetSearchRequests(_seasonSearchCriteria);
            var requests = results.GetAllTiers().SelectMany(x => x).ToList();

            requests.Should().HaveCountGreaterThan(0);
            requests.Should().Contain(x => x.Url.Query.Contains("Monkey") && x.Url.Query.Contains("Island"));
        }

        [Test]
        public void should_search_season_with_network_name_when_series_name_source_is_network()
        {
            _capabilities.SupportedSearchParameters = new[] { "q" };
            Subject.Settings.SearchSiteTitleOnly = true;
            Subject.Settings.SeriesNameSource = SeriesNameSource.Network;

            var results = Subject.GetSearchRequests(_seasonSearchCriteria);
            var requests = results.GetAllTiers().SelectMany(x => x).ToList();

            requests.Should().HaveCountGreaterThan(0);
            requests.Should().Contain(x => x.Url.Query.Contains("HBO"));
        }

        [Test]
        public void should_search_season_with_both_years()
        {
            _capabilities.SupportedSearchParameters = new[] { "q" };

            var results = Subject.GetSearchRequests(_seasonSearchCriteria);
            var requests = results.GetAllTiers().SelectMany(x => x).ToList();

            requests.Should().Contain(x => x.Url.Query.Contains("21"));
            requests.Should().Contain(x => x.Url.Query.Contains("2021"));
        }
    }
}
