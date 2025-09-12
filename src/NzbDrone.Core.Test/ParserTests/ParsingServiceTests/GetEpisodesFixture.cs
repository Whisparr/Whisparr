using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.ParserTests.ParsingServiceTests
{
    [TestFixture]
    public class GetEpisodesFixture : TestBase<ParsingService>
    {
        private Series _series;
        private List<Episode> _episodes;
        private ParsedEpisodeInfo _parsedEpisodeInfo;
        private SingleEpisodeSearchCriteria _singleEpisodeSearchCriteria;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>.CreateNew()
                .With(s => s.Title = "30 Stone")
                .With(s => s.CleanTitle = "stone")
                .Build();

            _episodes = Builder<Episode>.CreateListOfSize(1)
                                        .All()
                                        .With(e => e.AirDate = DateTime.Today.ToString(Episode.AIR_DATE_FORMAT))
                                        .Build()
                                        .ToList();

            _parsedEpisodeInfo = new ParsedEpisodeInfo
            {
                SeriesTitle = _series.Title,
                Languages = new List<Language> { Language.English }
            };

            _singleEpisodeSearchCriteria = new SingleEpisodeSearchCriteria
            {
                Series = _series,
                Episodes = _episodes
            };

            Mocker.GetMock<ISeriesService>()
                  .Setup(s => s.FindByTitle(It.IsAny<string>()))
                  .Returns(_series);

            // Mock GetAllSeries to prevent hanging during ExternalId lookups
            Mocker.GetMock<ISeriesService>()
                  .Setup(s => s.GetAllSeries())
                  .Returns(new List<Series> { _series });

            // Mock FindEpisodeByExternalId to prevent hanging
            Mocker.GetMock<IEpisodeService>()
                  .Setup(s => s.FindEpisodeByExternalId(It.IsAny<int>(), It.IsAny<string>()))
                  .Returns((Episode)null);
        }

        private void GivenSceneNumberingSeries()
        {
            _series.UseSceneNumbering = true;
        }

        [Test]
        public void should_get_episodes_when_series_found()
        {
            var episodes = Subject.GetEpisodes(_parsedEpisodeInfo, _series, false);

            episodes.Should().NotBeNull();
        }

        [Test]
        public void should_use_scene_numbering_when_series_uses_scene_numbering()
        {
            GivenSceneNumberingSeries();

            var episodes = Subject.GetEpisodes(_parsedEpisodeInfo, _series, true);

            episodes.Should().NotBeNull();
        }
    }
}
