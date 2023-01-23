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
            EnsureNoSeries(77, "My Family Pies");

            var result = WantedMissing.GetPaged(0, 15, "airDateUtc", "desc");

            result.Records.Should().BeEmpty();
        }

        [Test]
        [Order(1)]
        public void missing_should_have_monitored_items()
        {
            EnsureSeries(77, "My Family Pies", true);

            var result = WantedMissing.GetPaged(0, 15, "airDateUtc", "desc");

            result.Records.Should().NotBeEmpty();
        }

        [Test]
        [Order(1)]
        public void missing_should_have_series()
        {
            EnsureSeries(77, "My Family Pies", true);

            var result = WantedMissing.GetPaged(0, 15, "airDateUtc", "desc");

            result.Records.First().Series.Should().NotBeNull();
            result.Records.First().Series.Title.Should().Be("My Family Pies");
        }

        [Test]
        [Order(1)]
        public void missing_should_not_have_unmonitored_items()
        {
            EnsureSeries(77, "My Family Pies", false);

            var result = WantedMissing.GetPaged(0, 15, "airDateUtc", "desc");

            result.Records.Should().BeEmpty();
        }

        [Test]
        [Order(2)]
        public void missing_should_have_unmonitored_items()
        {
            EnsureSeries(77, "My Family Pies", false);

            var result = WantedMissing.GetPaged(0, 15, "airDateUtc", "desc", "monitored", "false");

            result.Records.Should().NotBeEmpty();
        }
    }
}
