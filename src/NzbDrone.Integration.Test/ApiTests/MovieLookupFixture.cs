using FluentAssertions;
using NUnit.Framework;

namespace NzbDrone.Integration.Test.ApiTests
{
    [TestFixture]
    public class MovieLookupFixture : IntegrationTest
    {
        [TestCase("taboo", "Taboo")]
        public void lookup_new_movie_by_title(string term, string title)
        {
            var movie = Movies.Lookup(term);

            movie.Should().NotBeEmpty();
            movie.Should().Contain(c => c.Title == title);
        }

        [Test]
        [Ignore("Not Implemented on Meta")]
        public void lookup_new_movie_by_imdbid()
        {
            var movie = Movies.Lookup("imdb:tt0081593");

            movie.Should().NotBeEmpty();
            movie.Should().Contain(c => c.Title == "Taboo");
        }

        [Test]
        [Ignore("Unreliable")]
        public void lookup_random_movie_using_asterix()
        {
            var movie = Movies.Lookup("*");

            movie.Should().NotBeEmpty();
        }
    }
}
