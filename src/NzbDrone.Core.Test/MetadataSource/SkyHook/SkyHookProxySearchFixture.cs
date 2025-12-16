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

        // TODO: Add more movie search test cases
        [TestCase("Consumed by Desire", "Consumed by Desire")]
        [TestCase("https://www.themoviedb.org/movie/699665-consumed-by-desire", "Consumed by Desire")]
        public void successful_movie_search(string title, string expected)
        {
            var result = Subject.SearchForNewMovie(title);

            result.Should().NotBeEmpty();

            result[0].Title.Should().Be(expected);

            ExceptionVerification.IgnoreWarns();
        }

        // TODO: add more scene search test cases
        [TestCase("slick and naughty tiffany tatum", "Slick and Naughty")]
        [TestCase("https://stashdb.org/scenes/019ac9a7-cdd2-7fc5-abca-4c64f97f6e17", "The Dad")]
        public void successful_scene_search(string title, string expected)
        {
            var result = Subject.SearchForNewScene(title);

            result.Should().NotBeEmpty();

            result[0].Title.Should().Be(expected);

            ExceptionVerification.IgnoreWarns();
        }

        [TestCase("whitney wright", "Whitney Wright")]
        [TestCase("stash:155f2559-d1f1-42b1-8cbe-9008542df5ce", "Angela White")]
        [TestCase("stashid:155f2559-d1f1-42b1-8cbe-9008542df5ce", "Angela White")]
        [TestCase("https://stashdb.org/performers/155f2559-d1f1-42b1-8cbe-9008542df5ce", "Angela White")]
        public void successful_performer_search(string title, string expected)
        {
            var result = Subject.SearchForNewPerformer(title);

            result.Should().NotBeEmpty();

            result[0].Name.Should().Be(expected);

            ExceptionVerification.IgnoreWarns();
        }

        [TestCase("brazzers exxtra", "Brazzers Exxtra")]
        [TestCase("https://stashdb.org/studios/39cee498-a9ac-4403-910a-1a0157ad22d8", "Brazzers Exxtra")]
        public void successful_studio_search(string title, string expected)
        {
            var result = Subject.SearchForNewStudio(title);

            result.Should().NotBeEmpty();

            result[0].Title.Should().Be(expected);

            ExceptionVerification.IgnoreWarns();
        }

        [TestCase("tmdbid:")]
        [TestCase("tmdbid: 99999999999999999999")]
        [TestCase("tmdbid: 0")]
        [TestCase("tmdbid: -12")]
        [TestCase("tmdbid:1")]
        [TestCase("adjalkwdjkalwdjklawjdlKAJD;EF")]
        [TestCase("imdb: tt9805708")]
        [TestCase("https://www.UNKNOWN-DOMAIN.com/title/tt0033467/")]
        [TestCase("https://www.themoviedb.org/MALFORMED/775-le-voyage-dans-la-lune")]
        public void no_movie_search_result(string term)
        {
            var result = Subject.SearchForNewMovie(term);
            result.Should().BeEmpty();

            ExceptionVerification.IgnoreWarns();
        }

        [TestCase("stash:")]
        [TestCase("stash: 99999999999999999999")]
        [TestCase("stash: 0")]
        [TestCase("stash: -12")]
        [TestCase("stash:1")]
        [TestCase("adjalkwdjkalwdjklawjdlKAJD;EF")]
        [TestCase("https://www.UNKNOWN-DOMAIN.com/scenes/tt0033467/")]
        [TestCase("https://stashdb.org/scenes/dhhiuooidhoiuhdojhdoduh")]
        public void no_scene_search_result(string term)
        {
            var result = Subject.SearchForNewScene(term);
            result.Should().BeEmpty();

            ExceptionVerification.IgnoreWarns();
        }
    }
}
