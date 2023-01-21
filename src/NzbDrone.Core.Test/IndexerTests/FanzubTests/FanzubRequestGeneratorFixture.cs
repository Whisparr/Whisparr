using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Indexers.Fanzub;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.IndexerTests.FanzubTests
{
    public class FanzubRequestGeneratorFixture : CoreTest<FanzubRequestGenerator>
    {
        private SeasonSearchCriteria _seasonSearchCriteria;

        [SetUp]
        public void SetUp()
        {
            Subject.Settings = new FanzubSettings()
            {
                BaseUrl = "http://127.0.0.1:1234/",
            };

            _seasonSearchCriteria = new SeasonSearchCriteria()
            {
                SceneTitles = new List<string>() { "Naruto Shippuuden" },
                Year = 1,
            };
        }

        [Test]
        public void should_not_search_season()
        {
            var results = Subject.GetSearchRequests(_seasonSearchCriteria);

            results.GetAllTiers().Should().HaveCount(0);
        }

        [Test]
        public void should_search_season()
        {
            Subject.Settings.AnimeStandardFormatSearch = true;
            var results = Subject.GetSearchRequests(_seasonSearchCriteria);

            results.GetAllTiers().Should().HaveCount(1);

            var page = results.GetAllTiers().First().First();

            page.Url.FullUri.Should().Contain("q=\"Naruto+Shippuuden%20S01\"|\"Naruto+Shippuuden%20-%20S01\"");
        }
    }
}
