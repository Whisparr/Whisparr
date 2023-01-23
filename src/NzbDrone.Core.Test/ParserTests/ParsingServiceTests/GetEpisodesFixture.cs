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
        }

        private void GivenSceneNumberingSeries()
        {
            _series.UseSceneNumbering = true;
        }

        [Test]
        public void should_use_scene_numbering_when_series_uses_scene_numbering()
        {
            GivenSceneNumberingSeries();

            Subject.Map(_parsedEpisodeInfo, _series.TvdbId);

            Mocker.GetMock<IEpisodeService>()
                .Verify(v => v.FindEpisodesBySceneNumbering(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once());
        }

        [Test]
        public void should_match_search_criteria_by_scene_numbering()
        {
            GivenSceneNumberingSeries();

            Subject.Map(_parsedEpisodeInfo, _series.TvdbId, _singleEpisodeSearchCriteria);

            Mocker.GetMock<IEpisodeService>()
                .Verify(v => v.FindEpisodesBySceneNumbering(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never());
        }

        [Test]
        public void should_fallback_to_findEpisode_when_search_criteria_match_fails_for_scene_numbering()
        {
            GivenSceneNumberingSeries();
            _episodes.First().SceneEpisodeNumber = 10;

            Subject.Map(_parsedEpisodeInfo, _series.TvdbId, _singleEpisodeSearchCriteria);

            Mocker.GetMock<IEpisodeService>()
                .Verify(v => v.FindEpisodesBySceneNumbering(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once());
        }

        [Test]
        public void should_find_episode()
        {
            Subject.Map(_parsedEpisodeInfo, _series.TvdbId);

            Mocker.GetMock<IEpisodeService>()
                .Verify(v => v.FindEpisode(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once());
        }

        [Test]
        public void should_match_episode_with_search_criteria()
        {
            Subject.Map(_parsedEpisodeInfo, _series.TvdbId, _singleEpisodeSearchCriteria);

            Mocker.GetMock<IEpisodeService>()
                .Verify(v => v.FindEpisode(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never());
        }

        [Test]
        public void should_fallback_to_findEpisode_when_search_criteria_match_fails()
        {
            _episodes.First().EpisodeNumber = 10;

            Subject.Map(_parsedEpisodeInfo, _series.TvdbId, _singleEpisodeSearchCriteria);

            Mocker.GetMock<IEpisodeService>()
                .Verify(v => v.FindEpisode(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once());
        }
    }
}
