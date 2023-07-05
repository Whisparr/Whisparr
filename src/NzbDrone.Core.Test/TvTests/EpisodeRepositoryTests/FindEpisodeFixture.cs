using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.TvTests.EpisodeRepositoryTests
{
    [TestFixture]
    public class FindEpisodeFixture : DbTest<EpisodeRepository, Episode>
    {
        private Episode _episode1;
        private Episode _episode2;

        [SetUp]
        public void Setup()
        {
            _episode1 = Builder<Episode>.CreateNew()
                                       .With(e => e.SeriesId = 1)
                                       .With(e => e.SeasonNumber = 1)
                                       .With(e => e.AbsoluteEpisodeNumber = 3)
                                       .BuildNew();

            _episode2 = Builder<Episode>.CreateNew()
                                        .With(e => e.SeriesId = 1)
                                        .With(e => e.SeasonNumber = 1)
                                        .BuildNew();

            _episode1 = Db.Insert(_episode1);
        }

        [Test]
        public void should_find_episode_by_absolute_numbering()
        {
            Subject.Find(_episode1.SeriesId, _episode1.AbsoluteEpisodeNumber.Value)
                .Id
                .Should()
                .Be(_episode1.Id);
        }
    }
}
