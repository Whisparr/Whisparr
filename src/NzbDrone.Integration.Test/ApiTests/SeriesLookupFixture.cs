using FluentAssertions;
using NUnit.Framework;

namespace NzbDrone.Integration.Test.ApiTests
{
    [TestFixture]
    public class SeriesLookupFixture : IntegrationTest
    {
        [TestCase("Bratty Sis", "Bratty Sis")]
        [TestCase("5K Porn", "5K Porn")]
        public void lookup_new_series_by_title(string term, string title)
        {
            var series = Series.Lookup(term);

            series.Should().NotBeEmpty();
            series.Should().Contain(c => c.Title == title);
        }

        [Test]
        public void lookup_new_series_by_tvdbid()
        {
            var series = Series.Lookup("tpdb:77");

            series.Should().NotBeEmpty();
            series.Should().Contain(c => c.Title == "My Family Pies");
        }

        [Test]
        [Ignore("Unreliable")]
        public void lookup_random_series_using_asterix()
        {
            var series = Series.Lookup("*");

            series.Should().NotBeEmpty();
        }
    }
}
