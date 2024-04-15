using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.TvTests.EpisodeServiceTests
{
    [TestFixture]
    public class ByAirDateFixture : CoreTest<EpisodeService>
    {
        private const int SERIES_ID = 1;
        private const string AIR_DATE = "2014-04-02";

        private Episode CreateEpisode(int seasonNumber, int episodeNumber, string performer = null, string title = null, string aridate = AIR_DATE)
        {
            var episode = Builder<Episode>.CreateNew()
                                          .With(e => e.SeriesId = SERIES_ID)
                                          .With(e => e.SeasonNumber = seasonNumber)
                                          .With(e => e.Title = title)
                                          .With(e => e.Actors = new List<Actor>
                                          {
                                              new Actor
                                              {
                                                  Name = performer,
                                                  Gender = Gender.Female,
                                              }
                                          })
                                          .With(e => e.AirDate = aridate)
                                          .BuildNew();

            return episode;
        }

        private void GivenEpisodes(params Episode[] episodes)
        {
            Mocker.GetMock<IEpisodeRepository>()
                  .Setup(s => s.Find(It.IsAny<int>(), It.IsAny<string>()))
                  .Returns(episodes.ToList());
        }

        [Test]
        public void should_return_null_when_finds_no_episode()
        {
            GivenEpisodes();

            Subject.FindEpisode(SERIES_ID, AIR_DATE, null).Should().BeNull();
        }

        [Test]
        public void should_get_episode_when_single_episode_exists_for_air_date()
        {
            GivenEpisodes(CreateEpisode(1, 1));

            Subject.FindEpisode(SERIES_ID, AIR_DATE, null).Should().NotBeNull();
        }

        [Test]
        public void should_get_special_when_its_the_only_episode_for_the_date_provided()
        {
            GivenEpisodes(CreateEpisode(0, 1));

            Subject.FindEpisode(SERIES_ID, AIR_DATE, null).Should().NotBeNull();
        }

        [Test]
        public void should_get_episode_when_two_regular_episodes_share_the_same_air_date_and_performer_part_is_provided()
        {
            var episode1 = CreateEpisode(2023, 1, "Jenna Jay");
            var episode2 = CreateEpisode(2023, 2, "Jackie Bush");

            GivenEpisodes(episode1, episode2);

            Subject.FindEpisode(SERIES_ID, AIR_DATE, " - Jenna Jay - [WEBDL-1080p]").Should().Be(episode1);
            Subject.FindEpisode(SERIES_ID, AIR_DATE, " - Jackie Bush - [WEBDL-1080p]").Should().Be(episode2);
        }

        [Test]
        public void should_get_episode_when_two_regular_episodes_share_the_same_air_date_and_performer_and_part_is_provided()
        {
            var episode1 = CreateEpisode(2023, 1, "Jenna Jay", "Get Some");
            var episode2 = CreateEpisode(2023, 2, "Jenna Jay", "Good Times");

            GivenEpisodes(episode1, episode2);

            Subject.FindEpisode(SERIES_ID, AIR_DATE, " - Jenna Jay - Get Some - [WEBDL-1080p]").Should().Be(episode1);
            Subject.FindEpisode(SERIES_ID, AIR_DATE, " - Jenna Jay - Good Times - [WEBDL-1080p]").Should().Be(episode2);
        }

        [Test]
        public void should_get_episode_when_two_regular_episodes_with_perfomer_in_title_share_the_same_air_date_and_performer_and_part_is_provided()
        {
            var title1 = "EvilAngel - 2021-02-24 - Whitney Wright POV Anal & A2M";
            var title2 = "EvilAngel - 2021-02-24 - BTS Whitney Wright POV Anal & A2M";

            var seriesTitleInfo1 = Parser.Parser.ParseTitle(title1);
            var seriesTitleInfo2 = Parser.Parser.ParseTitle(title2);

            var episode1 = CreateEpisode(2023, 1, "Whitney Wright", "Whitney Wright: POV Anal & A2M", "2021-02-24");
            var episode2 = CreateEpisode(2023, 2, "Whitney Wright", "BTS-Whitney Wright: POV Anal & A2M", "2021-02-24");

            GivenEpisodes(episode1, episode2);

            Subject.FindEpisode(SERIES_ID, seriesTitleInfo1.AirDate, seriesTitleInfo1.ReleaseTokens).Should().Be(episode1);
            Subject.FindEpisode(SERIES_ID, seriesTitleInfo1.AirDate, seriesTitleInfo2.ReleaseTokens).Should().Be(episode2);
        }

        [Test]
        public void should_get_episode_when_two_regular_episodes_share_the_same_air_date_and_title_part_is_provided()
        {
            var episode1 = CreateEpisode(2023, 1, "Jenna Jay", "Get Some");
            var episode2 = CreateEpisode(2023, 2, "Jackie Bush", "Good Times");

            GivenEpisodes(episode1, episode2);

            Subject.FindEpisode(SERIES_ID, AIR_DATE, " - Get Some - [WEBDL-1080p]").Should().Be(episode1);
            Subject.FindEpisode(SERIES_ID, AIR_DATE, " - Good Times - [WEBDL-1080p]").Should().Be(episode2);
        }

        [Test]
        public void should_normalize_episode_titles_for_compare()
        {
            var episode1 = CreateEpisode(2023, 1, "Jenna Jay", "Get Some");
            var episode2 = CreateEpisode(2023, 2, "Jackie Bush", "Good Times");

            GivenEpisodes(episode1, episode2);

            Subject.FindEpisode(SERIES_ID, AIR_DATE, ".Jenna.Jay.Get.Some.XXX.720p.MP4-SEXALiTY").Should().Be(episode1);
            Subject.FindEpisode(SERIES_ID, AIR_DATE, ".Jackie.Bush.Good.Times.XXX.720p.MP4-SEXALiTY").Should().Be(episode2);
        }
    }
}
