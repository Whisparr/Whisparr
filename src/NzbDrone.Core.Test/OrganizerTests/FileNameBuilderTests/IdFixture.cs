using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NUnit.Framework.Internal;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.OrganizerTests.FileNameBuilderTests
{
    [TestFixture]
    public class IdFixture : CoreTest<FileNameBuilder>
    {
        private Media _movie;
        private NamingConfig _namingConfig;

        [SetUp]
        public void Setup()
        {
            _movie = Builder<Media>
                      .CreateNew()
                      .With(s => s.Title = "Movie Title")
                      .With(s => s.ForiegnId = 123456)
                      .Build();

            _namingConfig = NamingConfig.Default;

            Mocker.GetMock<INamingConfigService>()
                  .Setup(c => c.GetConfig()).Returns(_namingConfig);
        }

        [Test]
        public void should_add_tmdb_id()
        {
            _namingConfig.MovieFolderFormat = "{Movie Title} ({TmdbId})";

            Subject.GetMovieFolder(_movie)
                   .Should().Be($"Movie Title ({_movie.ForiegnId})");
        }
    }
}
