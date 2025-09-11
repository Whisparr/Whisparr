using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine.Specifications.Search;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.DecisionEngineTests.Search.SingleEpisodeSearchMatchSpecificationTests
{
    [TestFixture]
    public class ExternalIdMatchFixture : CoreTest<SingleEpisodeSearchMatchSpecification>
    {
        private Series _series;
        private Episode _episode;
        private RemoteEpisode _remoteEpisode;
        private SingleEpisodeSearchCriteria _searchCriteria;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>.CreateNew().Build();
            _episode = Builder<Episode>.CreateNew()
                                      .With(e => e.SeriesId = _series.Id)
                                      .Build();

            _remoteEpisode = new RemoteEpisode
            {
                Series = _series,
                Episodes = new List<Episode> { _episode },
                ParsedEpisodeInfo = new ParsedEpisodeInfo()
            };

            _searchCriteria = new SingleEpisodeSearchCriteria
            {
                Series = _series,
                Episodes = new List<Episode> { _episode }
            };
        }

        [Test]
        public void should_accept_when_external_ids_match()
        {
            _searchCriteria.ExternalId = "SAVR-235";
            _remoteEpisode.ParsedEpisodeInfo.ExternalId = "SAVR-235";

            Subject.IsSatisfiedBy(_remoteEpisode, _searchCriteria).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_accept_when_external_ids_match_case_insensitive()
        {
            _searchCriteria.ExternalId = "SAVR-235";
            _remoteEpisode.ParsedEpisodeInfo.ExternalId = "savr-235";

            Subject.IsSatisfiedBy(_remoteEpisode, _searchCriteria).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_reject_when_external_ids_do_not_match()
        {
            _searchCriteria.ExternalId = "SAVR-235";
            _remoteEpisode.ParsedEpisodeInfo.ExternalId = "PRED-456";

            var result = Subject.IsSatisfiedBy(_remoteEpisode, _searchCriteria);
            result.Accepted.Should().BeFalse();
            result.Reason.Should().Be("Wrong External ID");
        }

        [Test]
        public void should_fall_back_to_air_date_when_search_criteria_has_no_external_id()
        {
            _searchCriteria.ExternalId = null;
            _searchCriteria.ReleaseDate = new System.DateOnly(2023, 1, 15);
            _remoteEpisode.ParsedEpisodeInfo.ExternalId = "SAVR-235";
            _remoteEpisode.ParsedEpisodeInfo.AirDate = "2023-01-15";

            Subject.IsSatisfiedBy(_remoteEpisode, _searchCriteria).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_fall_back_to_air_date_when_remote_episode_has_no_external_id()
        {
            _searchCriteria.ExternalId = "SAVR-235";
            _searchCriteria.ReleaseDate = new System.DateOnly(2023, 1, 15);
            _remoteEpisode.ParsedEpisodeInfo.ExternalId = null;
            _remoteEpisode.ParsedEpisodeInfo.AirDate = "2023-01-15";

            Subject.IsSatisfiedBy(_remoteEpisode, _searchCriteria).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_reject_when_release_date_does_not_match_and_no_external_ids()
        {
            _searchCriteria.ExternalId = null;
            _searchCriteria.ReleaseDate = new System.DateOnly(2023, 1, 15);
            _remoteEpisode.ParsedEpisodeInfo.ExternalId = null;
            _remoteEpisode.ParsedEpisodeInfo.AirDate = "2023-01-20";

            var result = Subject.IsSatisfiedBy(_remoteEpisode, _searchCriteria);
            result.Accepted.Should().BeFalse();
            result.Reason.Should().Be("Wrong Episode");
        }

        [Test]
        public void should_accept_when_no_search_criteria()
        {
            Subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_reject_when_search_criteria_has_no_release_date()
        {
            _searchCriteria.ExternalId = null;
            _searchCriteria.ReleaseDate = null;
            _remoteEpisode.ParsedEpisodeInfo.ExternalId = null;

            var result = Subject.IsSatisfiedBy(_remoteEpisode, _searchCriteria);
            result.Accepted.Should().BeFalse();
            result.Reason.Should().Be("No Episode Release Date");
        }
    }
}
