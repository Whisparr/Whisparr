using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.MetadataSource.SkyHook;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;
using NzbDrone.Test.Common.Categories;

namespace NzbDrone.Core.Test.MetadataSource.SkyHook
{
    [TestFixture]
    [IntegrationTest]
    public class SkyHookProxySearchFixture : CoreTest<SkyHookProxy>
    {
        [SetUp]
        public void Setup()
        {
            UseRealHttp();
        }

        [TestCase("Blacked Raw", "Blacked Raw")]
        [TestCase("Brazzers Exxtra", "Brazzers Exxtra")]
        [TestCase("Creampie Angels", "Creampie Angels")]
        [TestCase("CovertJapan", "CovertJapan")]
        [TestCase("Don't Break Me", "Don't Break Me")]
        [TestCase("Gloryhole Secrets", "Gloryhole Secrets")]
        [TestCase("True Amateurs", "True Amateurs")]

        [TestCase("tpdb:2273", "5K Porn")]
        [TestCase("tpdbid:2273", "5K Porn")]
        [TestCase("tpdbid: 2273 ", "5K Porn")]
        public void successful_search(string title, string expected)
        {
            var result = Subject.SearchForNewSeries(title);

            result.Should().NotBeEmpty();

            result[0].Title.Should().Be(expected);

            ExceptionVerification.IgnoreWarns();
        }

        [TestCase("tpdbid:")]
        [TestCase("tpdbid: 99999999999999999999")]
        [TestCase("tpdbid: 0")]
        [TestCase("tpdbid: -12")]
        [TestCase("tpdbid:1")]
        [TestCase("adjalkwdjkalwdjklawjdlKAJD")]
        public void no_search_result(string term)
        {
            var result = Subject.SearchForNewSeries(term);
            result.Should().BeEmpty();

            ExceptionVerification.IgnoreWarns();
        }

        [TestCase("tpdbid:2273")]
        [TestCase("5K Porn")]
        public void should_return_existing_series_if_found(string term)
        {
            const int tvdbId = 2273;
            var existingSeries = new Series
            {
                TvdbId = tvdbId
            };

            Mocker.GetMock<ISeriesService>().Setup(c => c.FindByTvdbId(tvdbId)).Returns(existingSeries);

            var result = Subject.SearchForNewSeries("tpdbid: " + tvdbId);

            result.Should().Contain(existingSeries);
            result.Should().ContainSingle(c => c.TvdbId == tvdbId);
        }
    }
}
