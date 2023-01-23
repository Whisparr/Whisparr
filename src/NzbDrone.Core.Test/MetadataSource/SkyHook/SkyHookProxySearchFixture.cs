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

        [TestCase("tvdb:2273", "5K Porn")]
        [TestCase("tvdbid:2273", "5K Porn")]
        [TestCase("tvdbid: 2273 ", "5K Porn")]
        public void successful_search(string title, string expected)
        {
            var result = Subject.SearchForNewSeries(title);

            result.Should().NotBeEmpty();

            result[0].Title.Should().Be(expected);

            ExceptionVerification.IgnoreWarns();
        }

        [TestCase("4565se")]
        public void should_not_search_by_imdb_if_invalid(string title)
        {
            var result = Subject.SearchForNewSeriesByImdbId(title);
            result.Should().BeEmpty();

            Mocker.GetMock<ISearchForNewSeries>()
                  .Verify(v => v.SearchForNewSeries(It.IsAny<string>()), Times.Never());

            ExceptionVerification.IgnoreWarns();
        }

        [TestCase("tvdbid:")]
        [TestCase("tvdbid: 99999999999999999999")]
        [TestCase("tvdbid: 0")]
        [TestCase("tvdbid: -12")]
        [TestCase("tvdbid:1")]
        [TestCase("adjalkwdjkalwdjklawjdlKAJD")]
        public void no_search_result(string term)
        {
            var result = Subject.SearchForNewSeries(term);
            result.Should().BeEmpty();

            ExceptionVerification.IgnoreWarns();
        }

        [TestCase("tvdbid:2273")]
        [TestCase("5K Porn")]
        public void should_return_existing_series_if_found(string term)
        {
            const int tvdbId = 2273;
            var existingSeries = new Series
            {
                TvdbId = tvdbId
            };

            Mocker.GetMock<ISeriesService>().Setup(c => c.FindByTvdbId(tvdbId)).Returns(existingSeries);

            var result = Subject.SearchForNewSeries("tvdbid: " + tvdbId);

            result.Should().Contain(existingSeries);
            result.Should().ContainSingle(c => c.TvdbId == tvdbId);
        }
    }
}
