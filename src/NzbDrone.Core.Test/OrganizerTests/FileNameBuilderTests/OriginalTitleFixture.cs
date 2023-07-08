using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.OrganizerTests.FileNameBuilderTests
{
    [TestFixture]
    public class OriginalTitleFixture : CoreTest<FileNameBuilder>
    {
        private Series _series;
        private Episode _episode;
        private EpisodeFile _episodeFile;
        private NamingConfig _namingConfig;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>
                    .CreateNew()
                    .With(s => s.Title = "My Series")
                    .Build();

            _episode = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "City Sushi")
                            .With(e => e.SeasonNumber = 15)
                            .With(e => e.AbsoluteEpisodeNumber = 100)
                            .Build();

            _episodeFile = new EpisodeFile { Id = 5, Quality = new QualityModel(Quality.HDTV720p), ReleaseGroup = "WhisparrTest" };

            _namingConfig = NamingConfig.Default;
            _namingConfig.RenameEpisodes = true;

            Mocker.GetMock<INamingConfigService>()
                  .Setup(c => c.GetConfig()).Returns(_namingConfig);

            Mocker.GetMock<IQualityDefinitionService>()
                .Setup(v => v.Get(Moq.It.IsAny<Quality>()))
                .Returns<Quality>(v => Quality.DefaultQualityDefinitions.First(c => c.Quality == v));

            Mocker.GetMock<ICustomFormatService>()
                  .Setup(v => v.All())
                  .Returns(new List<CustomFormat>());
        }

        [Test]
        public void should_include_original_title_if_not_current_file_name()
        {
            _episodeFile.SceneName = "my.series.s15e06";
            _episodeFile.RelativePath = "My Series - S15E06 - City Sushi";
            _namingConfig.StandardEpisodeFormat = "{Site Title} - {Episode Title} {[Original Title]}";

            Subject.BuildFileName(new List<Episode> { _episode }, _series, _episodeFile)
                   .Should().Be("My Series - City Sushi [my.series.s15e06]");
        }

        [Test]
        public void should_include_current_filename_if_not_renaming_files()
        {
            _episodeFile.SceneName = "my.series.s15e06";
            _namingConfig.RenameEpisodes = false;

            Subject.BuildFileName(new List<Episode> { _episode }, _series, _episodeFile)
                   .Should().Be("my.series.s15e06");
        }

        [Test]
        public void should_include_current_filename_if_not_including_season_and_episode_tokens_for_standard_series()
        {
            _episodeFile.RelativePath = "My Series - S15E06 - City Sushi";
            _namingConfig.StandardEpisodeFormat = "{Original Title} {Quality Title}";

            Subject.BuildFileName(new List<Episode> { _episode }, _series, _episodeFile)
                   .Should().Be("My Series - S15E06 - City Sushi HDTV-720p");
        }

        [Test]
        public void should_include_current_filename_for_new_file_if_including_season_and_episode_tokens_for_standard_series()
        {
            _episodeFile.Id = 0;
            _episodeFile.RelativePath = "My Series - City Sushi";
            _namingConfig.StandardEpisodeFormat = "{Site Title} - {[Original Title]}";

            Subject.BuildFileName(new List<Episode> { _episode }, _series, _episodeFile)
                   .Should().Be("My Series - [My Series - City Sushi]");
        }
    }
}
