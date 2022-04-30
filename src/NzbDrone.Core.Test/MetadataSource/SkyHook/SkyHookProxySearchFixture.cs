using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MetadataSource.SkyHook;
using NzbDrone.Core.Test.Framework;
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

        [TestCase("Prometheus", "Prometheus")]

        // TODO: TMDB Doesn't like when we clean periods from this
        // [TestCase("The Man from U.N.C.L.E.", "The Man from U.N.C.L.E.")]
        [Ignore("Testing TPDB")]
        [TestCase("imdb:tt0081593", "Taboo")]
        [TestCase("imdb:tt0084755", "Taboo II")]
        public void successful_search(string title, string expected)
        {
            var result = Subject.SearchForNewMovie(title);

            result.Should().NotBeEmpty();

            result[0].Title.Should().Be(expected);

            ExceptionVerification.IgnoreWarns();
        }

        [TestCase("tmdbid:")]
        [TestCase("tmdbid: 99999999999999999999")]
        [TestCase("tmdbid: 0")]
        [TestCase("tmdbid: -12")]
        [TestCase("adjalkwdjkalwdjklawjdlKAJD;EF")]
        [TestCase("imdb: tt9805708")]
        public void no_search_result(string term)
        {
            var result = Subject.SearchForNewMovie(term);
            result.Should().BeEmpty();

            ExceptionVerification.IgnoreWarns();
        }
    }
}
