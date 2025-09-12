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

            // Mock the FindByExternalId methods that our implementation actually calls
            Mocker.GetMock<IEpisodeRepository>()
                  .Setup(c => c.FindByExternalId(It.IsAny<int>(), It.IsAny<string>()))
                  .Returns<int, string>((seriesId, externalId) =>
                  {
                      if (string.IsNullOrWhiteSpace(externalId))
                      {
                          return null;
                      }

                      // Try exact match first
                      var exactMatch = _episodes.FirstOrDefault(e => e.SeriesId == seriesId && e.ExternalId == externalId);
                      if (exactMatch != null)
                      {
                          return exactMatch;
                      }

                      // Fall back to case-insensitive match
                      return _episodes.FirstOrDefault(e => e.SeriesId == seriesId &&
                                                          !string.IsNullOrWhiteSpace(e.ExternalId) &&
                                                          e.ExternalId.Equals(externalId, System.StringComparison.OrdinalIgnoreCase));
                  });

            // Mock global external ID search as well
            Mocker.GetMock<IEpisodeRepository>()
                  .Setup(c => c.FindByExternalId(It.IsAny<string>()))
                  .Returns<string>((externalId) =>
                  {
                      if (string.IsNullOrWhiteSpace(externalId))
                      {
                          return null;
                      }

                      // Try exact match first
                      var exactMatch = _episodes.FirstOrDefault(e => e.ExternalId == externalId);
                      if (exactMatch != null)
                      {
                          return exactMatch;
                      }

                      // Fall back to case-insensitive match
                      return _episodes.FirstOrDefault(e => !string.IsNullOrWhiteSpace(e.ExternalId) &&
                                                          e.ExternalId.Equals(externalId, System.StringComparison.OrdinalIgnoreCase));
                  });
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
    }
}
