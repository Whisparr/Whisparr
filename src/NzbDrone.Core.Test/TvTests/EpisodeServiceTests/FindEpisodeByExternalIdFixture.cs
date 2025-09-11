using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.TvTests.EpisodeServiceTests
{
    [TestFixture]
    public class FindEpisodeByExternalIdFixture : CoreTest<EpisodeService>
    {
        private const int SERIES_ID = 1;
        private List<Episode> _episodes;

        [SetUp]
        public void Setup()
        {
            _episodes = Builder<Episode>.CreateListOfSize(5)
                                        .All()
                                        .With(e => e.SeriesId = SERIES_ID)
                                        .Build()
                                        .ToList();
        }

        private void GivenEpisodesWithExternalIds(params string[] externalIds)
        {
            for (var i = 0; i < externalIds.Length && i < _episodes.Count; i++)
            {
                _episodes[i].ExternalId = externalIds[i];
            }

            Mocker.GetMock<IEpisodeRepository>()
                  .Setup(c => c.GetEpisodes(SERIES_ID))
                  .Returns(_episodes);
        }

        [Test]
        public void should_find_episode_by_external_id()
        {
            const string externalId = "SAVR-235";
            GivenEpisodesWithExternalIds("PRED-123", externalId, "DVRT-456");

            var result = Subject.FindEpisodeByExternalId(SERIES_ID, externalId);

            result.Should().NotBeNull();
            result.ExternalId.Should().Be(externalId);
        }

        [Test]
        public void should_be_case_insensitive()
        {
            const string externalId = "SAVR-235";
            GivenEpisodesWithExternalIds("PRED-123", externalId, "DVRT-456");

            var result = Subject.FindEpisodeByExternalId(SERIES_ID, "savr-235");

            result.Should().NotBeNull();
            result.ExternalId.Should().Be(externalId);
        }

        [Test]
        public void should_return_null_when_external_id_not_found()
        {
            GivenEpisodesWithExternalIds("PRED-123", "SAVR-235", "DVRT-456");

            var result = Subject.FindEpisodeByExternalId(SERIES_ID, "NONEXISTENT-999");

            result.Should().BeNull();
        }

        [Test]
        public void should_return_null_when_external_id_is_null()
        {
            GivenEpisodesWithExternalIds("PRED-123", "SAVR-235", "DVRT-456");

            var result = Subject.FindEpisodeByExternalId(SERIES_ID, null);

            result.Should().BeNull();
        }

        [Test]
        public void should_return_null_when_external_id_is_empty()
        {
            GivenEpisodesWithExternalIds("PRED-123", "SAVR-235", "DVRT-456");

            var result = Subject.FindEpisodeByExternalId(SERIES_ID, "");

            result.Should().BeNull();
        }

        [Test]
        public void should_return_null_when_external_id_is_whitespace()
        {
            GivenEpisodesWithExternalIds("PRED-123", "SAVR-235", "DVRT-456");

            var result = Subject.FindEpisodeByExternalId(SERIES_ID, "   ");

            result.Should().BeNull();
        }

        [Test]
        public void should_ignore_episodes_with_null_external_id()
        {
            GivenEpisodesWithExternalIds(null, "SAVR-235", "");

            var result = Subject.FindEpisodeByExternalId(SERIES_ID, "SAVR-235");

            result.Should().NotBeNull();
            result.ExternalId.Should().Be("SAVR-235");
        }

        [Test]
        public void should_return_first_match_when_multiple_episodes_have_same_external_id()
        {
            // This shouldn't happen in practice, but test the behavior
            _episodes[1].ExternalId = "SAVR-235";
            _episodes[3].ExternalId = "SAVR-235";

            Mocker.GetMock<IEpisodeRepository>()
                  .Setup(c => c.GetEpisodes(SERIES_ID))
                  .Returns(_episodes);

            var result = Subject.FindEpisodeByExternalId(SERIES_ID, "SAVR-235");

            result.Should().NotBeNull();
            result.Should().Be(_episodes[1]); // Should return the first match
        }
    }
}
