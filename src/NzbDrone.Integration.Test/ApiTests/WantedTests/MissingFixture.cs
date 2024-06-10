using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace NzbDrone.Integration.Test.ApiTests.WantedTests
{
    [TestFixture]
    public class MissingFixture : IntegrationTest
    {
        [Test]
        [Order(0)]
        public void missing_should_be_empty()
        {
            EnsureNoMovie(42019, "Taboo");

            var result = WantedMissing.GetPaged(0, 15, "movieMetadata.year", "desc");

            result.Records.Should().BeEmpty();
        }

        [Test]
        [Order(1)]
        public void missing_should_have_monitored_items()
        {
            EnsureMovie(42019, "Taboo", true);

            var result = WantedMissing.GetPaged(0, 15, "movieMetadata.year", "desc");

            result.Records.Should().NotBeEmpty();
        }

        [Test]
        [Order(1)]
        public void missing_should_have_series()
        {
            EnsureMovie(42019, "Taboo", true);

            var result = WantedMissing.GetPaged(0, 15, "movieMetadata.year", "desc");

            result.Records.First().Title.Should().Be("Taboo");
        }

        [Test]
        [Order(1)]
        public void missing_should_not_have_unmonitored_items()
        {
            EnsureMovie(42019, "Taboo", false);

            var result = WantedMissing.GetPaged(0, 15, "movieMetadata.year", "desc");

            result.Records.Should().BeEmpty();
        }

        [Test]
        [Order(2)]
        public void missing_should_have_unmonitored_items()
        {
            EnsureMovie(42019, "Taboo", false);

            var result = WantedMissing.GetPaged(0, 15, "movieMetadata.year", "desc", "monitored", false);

            result.Records.Should().NotBeEmpty();
        }
    }
}
