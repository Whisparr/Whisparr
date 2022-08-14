using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine.Specifications.RssSync;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]

    public class MonitoredMovieSpecificationFixture : CoreTest<MonitoredMovieSpecification>
    {
        private MonitoredMovieSpecification _monitoredEpisodeSpecification;

        private RemoteMovie _parseResultMulti;
        private RemoteMovie _parseResultSingle;
        private Media _fakeSeries;
        private Media _firstEpisode;
        private Media _secondEpisode;

        [SetUp]
        public void Setup()
        {
            _monitoredEpisodeSpecification = Mocker.Resolve<MonitoredMovieSpecification>();

            _fakeSeries = Builder<Media>.CreateNew()
                .With(c => c.Monitored = true)
                .Build();

            _firstEpisode = new Media() { Monitored = true };
            _secondEpisode = new Media() { Monitored = true };

            var singleEpisodeList = new List<Media> { _firstEpisode };
            var doubleEpisodeList = new List<Media> { _firstEpisode, _secondEpisode };

            _parseResultMulti = new RemoteMovie
            {
                Movie = _fakeSeries
            };

            _parseResultSingle = new RemoteMovie
            {
                Movie = _fakeSeries
            };
        }

        private void WithMovieUnmonitored()
        {
            _fakeSeries.Monitored = false;
        }

        [Test]
        public void setup_should_return_monitored_episode_should_return_true()
        {
            _monitoredEpisodeSpecification.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeTrue();
            _monitoredEpisodeSpecification.IsSatisfiedBy(_parseResultMulti, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void not_monitored_series_should_be_skipped()
        {
            _fakeSeries.Monitored = false;
            _monitoredEpisodeSpecification.IsSatisfiedBy(_parseResultMulti, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void only_episode_not_monitored_should_return_false()
        {
            WithMovieUnmonitored();
            _monitoredEpisodeSpecification.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_for_single_episode_search()
        {
            _fakeSeries.Monitored = false;
            _monitoredEpisodeSpecification.IsSatisfiedBy(_parseResultSingle, new MovieSearchCriteria { UserInvokedSearch = true }).Accepted.Should().BeTrue();
        }
    }
}
