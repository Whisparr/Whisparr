using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.ParserTests.ParsingServiceTests
{
    [TestFixture]
    public class ExternalIdMatchingFixture : CoreTest<ParsingService>
    {
        private Series _series1;
        private Series _series2;
        private Episode _episode1;
        private Episode _episode2;
        private List<Series> _allSeries;

        [SetUp]
        public void Setup()
        {
            _series1 = Builder<Series>.CreateNew()
                                     .With(s => s.Id = 1)
                                     .With(s => s.Title = "JAV Series 1")
                                     .Build();

            _series2 = Builder<Series>.CreateNew()
                                     .With(s => s.Id = 2)
                                     .With(s => s.Title = "JAV Series 2")
                                     .Build();

            _episode1 = Builder<Episode>.CreateNew()
                                       .With(e => e.Id = 1)
                                       .With(e => e.SeriesId = 1)
                                       .With(e => e.ExternalId = "SAVR-235")
                                       .With(e => e.Series = _series1)
                                       .Build();

            _episode2 = Builder<Episode>.CreateNew()
                                       .With(e => e.Id = 2)
                                       .With(e => e.SeriesId = 2)
                                       .With(e => e.ExternalId = "PRED-456")
                                       .With(e => e.Series = _series2)
                                       .Build();

            _allSeries = new List<Series> { _series1, _series2 };

            Mocker.GetMock<ISeriesService>()
                  .Setup(s => s.GetAllSeries())
                  .Returns(_allSeries);

            // Setup global external ID lookups (what the ParsingService actually calls)
            Mocker.GetMock<IEpisodeService>()
                  .Setup(s => s.FindEpisodeByGlobalExternalId("SAVR-235"))
                  .Returns(_episode1);

            Mocker.GetMock<IEpisodeService>()
                  .Setup(s => s.FindEpisodeByGlobalExternalId("PRED-456"))
                  .Returns(_episode2);

            Mocker.GetMock<IEpisodeService>()
                  .Setup(s => s.FindEpisodeByGlobalExternalId("NONEXISTENT-999"))
                  .Returns((Episode)null);

            // Setup series service for series lookups by ID
            Mocker.GetMock<ISeriesService>()
                  .Setup(s => s.GetSeries(1))
                  .Returns(_series1);

            Mocker.GetMock<ISeriesService>()
                  .Setup(s => s.GetSeries(2))
                  .Returns(_series2);

            // Setup episode service for individual series lookups (for GetEpisodes method)
            Mocker.GetMock<IEpisodeService>()
                  .Setup(s => s.FindEpisodeByExternalId(1, "SAVR-235"))
                  .Returns(_episode1);

            Mocker.GetMock<IEpisodeService>()
                  .Setup(s => s.FindEpisodeByExternalId(2, "PRED-456"))
                  .Returns(_episode2);

            Mocker.GetMock<IEpisodeService>()
                  .Setup(s => s.FindEpisodeByExternalId(It.IsAny<int>(), "NONEXISTENT-999"))
                  .Returns((Episode)null);
        }

        [Test]
        public void should_find_series_by_external_id_when_no_series_title()
        {
            var parsedEpisodeInfo = new ParsedEpisodeInfo
            {
                ExternalId = "SAVR-235",
                SeriesTitle = "", // No series title for JAV content
                SeriesTitleInfo = new SeriesTitleInfo { Title = "" }
            };

            var result = Subject.Map(parsedEpisodeInfo, 0);

            result.Should().NotBeNull();
            result.Series.Should().NotBeNull();
            result.Series.Id.Should().Be(1);
            result.Series.Title.Should().Be("JAV Series 1");
            result.Episodes.Should().HaveCount(1);
            result.Episodes[0].ExternalId.Should().Be("SAVR-235");
        }

        [Test]
        public void should_find_different_series_by_different_external_id()
        {
            var parsedEpisodeInfo = new ParsedEpisodeInfo
            {
                ExternalId = "PRED-456",
                SeriesTitle = "",
                SeriesTitleInfo = new SeriesTitleInfo { Title = "" }
            };

            var result = Subject.Map(parsedEpisodeInfo, 0);

            result.Should().NotBeNull();
            result.Series.Should().NotBeNull();
            result.Series.Id.Should().Be(2);
            result.Series.Title.Should().Be("JAV Series 2");
            result.Episodes.Should().HaveCount(1);
            result.Episodes[0].ExternalId.Should().Be("PRED-456");
        }

        [Test]
        public void should_return_empty_episodes_when_external_id_not_found()
        {
            var parsedEpisodeInfo = new ParsedEpisodeInfo
            {
                ExternalId = "NONEXISTENT-999",
                SeriesTitle = "",
                SeriesTitleInfo = new SeriesTitleInfo { Title = "" }
            };

            var result = Subject.Map(parsedEpisodeInfo, 0);

            result.Should().NotBeNull();
            result.Series.Should().BeNull();
            result.Episodes.Should().BeEmpty();
        }

        [Test]
        public void should_prioritize_external_id_over_air_date_matching()
        {
            var parsedEpisodeInfo = new ParsedEpisodeInfo
            {
                ExternalId = "SAVR-235",
                SeriesTitle = "",
                SeriesTitleInfo = new SeriesTitleInfo { Title = "" },
                AirDate = "2023-01-01" // This should be ignored when external ID is available
            };

            // Setup to return empty for air date matching
            Mocker.GetMock<IEpisodeService>()
                  .Setup(s => s.FindEpisode(It.IsAny<int>(), "2023-01-01", It.IsAny<string>()))
                  .Returns((Episode)null);

            var result = Subject.Map(parsedEpisodeInfo, 0);

            result.Should().NotBeNull();
            result.Series.Should().NotBeNull();
            result.Episodes.Should().HaveCount(1);
            result.Episodes[0].ExternalId.Should().Be("SAVR-235");

            // Verify that air date matching was not called since external ID was found
            Mocker.GetMock<IEpisodeService>()
                  .Verify(s => s.FindEpisode(It.IsAny<int>(), "2023-01-01", It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void should_fall_back_to_air_date_when_external_id_empty()
        {
            var parsedEpisodeInfo = new ParsedEpisodeInfo
            {
                ExternalId = "", // Empty external ID
                SeriesTitle = "Regular TV Show",
                SeriesTitleInfo = new SeriesTitleInfo { Title = "Regular TV Show" },
                AirDate = "2023-01-01"
            };

            // Setup regular series matching
            var regularSeries = Builder<Series>.CreateNew()
                                              .With(s => s.Id = 99)
                                              .With(s => s.Title = "Regular TV Show")
                                              .Build();

            var regularEpisode = Builder<Episode>.CreateNew()
                                               .With(e => e.Id = 99)
                                               .With(e => e.SeriesId = 99)
                                               .With(e => e.AirDate = "2023-01-01")
                                               .Build();

            Mocker.GetMock<ISeriesService>()
                  .Setup(s => s.FindByTitle("Regular TV Show"))
                  .Returns(regularSeries);

            Mocker.GetMock<IEpisodeService>()
                  .Setup(s => s.FindEpisode(99, "2023-01-01", It.IsAny<string>()))
                  .Returns(regularEpisode);

            var result = Subject.Map(parsedEpisodeInfo, 0);

            result.Should().NotBeNull();
            result.Series.Should().NotBeNull();
            result.Series.Id.Should().Be(99);
            result.Episodes.Should().HaveCount(1);
            result.Episodes[0].AirDate.Should().Be("2023-01-01");
        }
    }
}
