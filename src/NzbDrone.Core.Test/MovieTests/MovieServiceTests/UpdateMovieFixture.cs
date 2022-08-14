using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MovieTests.MovieServiceTests
{
    [TestFixture]
    public class UpdateMovieFixture : CoreTest<MovieService>
    {
        private Media _fakeMovie;
        private Media _existingMovie;

        [SetUp]
        public void Setup()
        {
            _fakeMovie = Builder<Media>.CreateNew().Build();
            _existingMovie = Builder<Media>.CreateNew().Build();
        }

        private void GivenExistingSeries()
        {
            Mocker.GetMock<IMovieService>()
                  .Setup(s => s.GetMovie(It.IsAny<int>()))
                  .Returns(_existingMovie);
        }

        [Test]
        public void should_update_movie_when_it_changes()
        {
            GivenExistingSeries();

            Subject.UpdateMovie(_fakeMovie);

            Mocker.GetMock<IMediaRepository>()
                  .Verify(v => v.Update(_fakeMovie), Times.Once());
        }
    }
}
