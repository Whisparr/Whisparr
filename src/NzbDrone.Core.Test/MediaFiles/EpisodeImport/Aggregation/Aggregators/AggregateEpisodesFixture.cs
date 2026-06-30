using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Download;
using NzbDrone.Core.History;
using NzbDrone.Core.MediaFiles.EpisodeImport.Aggregation.Aggregators;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.EpisodeImport.Aggregation.Aggregators
{
    [TestFixture]
    public class AugmentEpisodesFixture : CoreTest<AggregateEpisodes>
    {
        private Series _series;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>.CreateNew().Build();

            var augmenters = new List<Mock<IAggregateLocalEpisode>>
                             {
                                 new Mock<IAggregateLocalEpisode>()
                             };

            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.GetEpisodes(It.IsAny<ParsedEpisodeInfo>(), _series, It.IsAny<bool>(), null))
                  .Returns(Builder<Episode>.CreateListOfSize(1).BuildList());

            Mocker.SetConstant(augmenters.Select(c => c.Object));
        }

        [Test]
        public void should_not_use_folder_for_full_season()
        {
            var fileEpisodeInfo = Parser.Parser.ParseTitle("Series.Title.23.01.15");
            var folderEpisodeInfo = Parser.Parser.ParseTitle("Series.Title.2023");
            var localEpisode = new LocalEpisode
                               {
                                   FileEpisodeInfo = fileEpisodeInfo,
                                   FolderEpisodeInfo = folderEpisodeInfo,
                                   Path = @"C:\Test\Unsorted TV\Series.Title.2023\Series.Title.23.01.15.mkv".AsOsAgnostic(),
                                   Series = _series
                               };

            Subject.Aggregate(localEpisode, null);

            Mocker.GetMock<IParsingService>()
                  .Verify(v => v.GetEpisodes(fileEpisodeInfo, _series, localEpisode.SceneSource, null), Times.Once());
        }

        [Test]
        public void should_not_use_folder_when_it_contains_more_than_one_valid_video_file()
        {
            var fileEpisodeInfo = Parser.Parser.ParseTitle("Series.Title.23.01.15");
            var folderEpisodeInfo = Parser.Parser.ParseTitle("Series.Title.2023");
            var localEpisode = new LocalEpisode
            {
                FileEpisodeInfo = fileEpisodeInfo,
                FolderEpisodeInfo = folderEpisodeInfo,
                Path = @"C:\Test\Unsorted TV\Series.Title.2023\Series.Title.23.01.15.mkv".AsOsAgnostic(),
                Series = _series,
                OtherVideoFiles = true
            };

            Subject.Aggregate(localEpisode, null);

            Mocker.GetMock<IParsingService>()
                  .Verify(v => v.GetEpisodes(fileEpisodeInfo, _series, localEpisode.SceneSource, null), Times.Once());
        }

        [Test]
        public void should_not_use_folder_name_if_file_name_is_scene_name()
        {
            var fileEpisodeInfo = Parser.Parser.ParseTitle("Series.Title.23.01.15");
            var folderEpisodeInfo = Parser.Parser.ParseTitle("Series.Title.23.01.15");
            var localEpisode = new LocalEpisode
            {
                FileEpisodeInfo = fileEpisodeInfo,
                FolderEpisodeInfo = folderEpisodeInfo,
                Path = @"C:\Test\Unsorted TV\Series.Title.23.01.15\Series.Title.23.01.15.720p.HDTV-Whisparr.mkv".AsOsAgnostic(),
                Series = _series
            };

            Subject.Aggregate(localEpisode, null);

            Mocker.GetMock<IParsingService>()
                  .Verify(v => v.GetEpisodes(fileEpisodeInfo, _series, localEpisode.SceneSource, null), Times.Once());
        }

        [Test]
        public void should_use_folder_when_only_one_video_file()
        {
            var fileEpisodeInfo = Parser.Parser.ParseTitle("Series.Title.23.01.15");
            var folderEpisodeInfo = Parser.Parser.ParseTitle("Series.Title.23.01.15");
            var localEpisode = new LocalEpisode
            {
                FileEpisodeInfo = fileEpisodeInfo,
                FolderEpisodeInfo = folderEpisodeInfo,
                Path = @"C:\Test\Unsorted TV\Series.Title.23.01.15\Series.Title.23.01.15.mkv".AsOsAgnostic(),
                Series = _series
            };

            Subject.Aggregate(localEpisode, null);

            Mocker.GetMock<IParsingService>()
                  .Verify(v => v.GetEpisodes(folderEpisodeInfo, _series, localEpisode.SceneSource, null), Times.Once());
        }

        [Test]
        public void should_use_file_when_folder_is_absolute_and_file_is_not()
        {
            var fileEpisodeInfo = Parser.Parser.ParseTitle("Series.Title.23.01.15");
            var folderEpisodeInfo = Parser.Parser.ParseTitle("Series.Title.01");
            var localEpisode = new LocalEpisode
                               {
                                   FileEpisodeInfo = fileEpisodeInfo,
                                   FolderEpisodeInfo = folderEpisodeInfo,
                                   Path = @"C:\Test\Unsorted TV\Series.Title.101\Series.Title.23.01.15.mkv".AsOsAgnostic(),
                                   Series = _series
                               };

            Subject.Aggregate(localEpisode, null);

            Mocker.GetMock<IParsingService>()
                  .Verify(v => v.GetEpisodes(fileEpisodeInfo, _series, localEpisode.SceneSource, null), Times.Once());
        }

        [Test]
        [Ignore("No specials in scene releases")]
        public void should_use_special_info_when_not_null()
        {
            var fileEpisodeInfo = Parser.Parser.ParseTitle("S00E01");
            var specialEpisodeInfo = fileEpisodeInfo.JsonClone();

            var localEpisode = new LocalEpisode
                               {
                                   FileEpisodeInfo = fileEpisodeInfo,
                                   Path = @"C:\Test\TV\Series\Specials\S00E01.mkv".AsOsAgnostic(),
                                   Series = _series
                               };

            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.GetEpisodes(fileEpisodeInfo, _series, It.IsAny<bool>(), null))
                  .Returns(new List<Episode>());

            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.ParseSpecialEpisodeTitle(fileEpisodeInfo, It.IsAny<string>(), _series))
                  .Returns(specialEpisodeInfo);

            Subject.Aggregate(localEpisode, null);

            Mocker.GetMock<IParsingService>()
                  .Verify(v => v.GetEpisodes(specialEpisodeInfo, _series, localEpisode.SceneSource, null), Times.Once());
        }

        [Test]
        public void should_match_episode_via_grab_history_when_filename_unparseable()
        {
            const int episodeId = 1695;
            var episode = Builder<Episode>.CreateNew()
                .With(e => e.Id = episodeId)
                .With(e => e.SeriesId = _series.Id)
                .Build();

            var downloadItem = Builder<DownloadClientItem>.CreateNew()
                .With(d => d.DownloadId = "a1b2c3d4e5f6789012345678901234567890abcd")
                .With(d => d.Title = "TestStudio-Test Performer-720p.mp4")
                .Build();

            var localEpisode = new LocalEpisode
            {
                Path = @"C:\Test\Downloads\TestStudio-Test Performer-720p.mp4".AsOsAgnostic(),
                Series = _series,
                DownloadItem = downloadItem,
                SceneSource = true
            };

            Mocker.GetMock<IHistoryService>()
                .Setup(s => s.FindByDownloadId(downloadItem.DownloadId))
                .Returns(new List<EpisodeHistory>
                {
                    new EpisodeHistory
                    {
                        EpisodeId = episodeId,
                        SeriesId = _series.Id,
                        EventType = EpisodeHistoryEventType.Grabbed,
                        SourceTitle = "[TestStudio] Test Performer (Test Episode) [2017-06-30, 720p]"
                    }
                });

            Mocker.GetMock<IEpisodeService>()
                .Setup(s => s.GetEpisodes(It.Is<IEnumerable<int>>(ids => ids.Contains(episodeId))))
                .Returns(new List<Episode> { episode });

            Subject.Aggregate(localEpisode, downloadItem);

            localEpisode.Episodes.Should().HaveCount(1);
            localEpisode.Episodes.First().Id.Should().Be(episodeId);
            Mocker.GetMock<IParsingService>()
                .Verify(v => v.GetEpisodes(It.IsAny<ParsedEpisodeInfo>(), It.IsAny<Series>(), It.IsAny<bool>(), null), Times.Never());
        }

        [Test]
        public void should_not_match_via_grab_history_when_no_grab_history()
        {
            var downloadItem = Builder<DownloadClientItem>.CreateNew()
                .With(d => d.DownloadId = "abc123")
                .With(d => d.Title = "TestStudio-Test Performer-720p.mp4")
                .Build();

            var localEpisode = new LocalEpisode
            {
                Path = @"C:\Test\Downloads\TestStudio-Test Performer-720p.mp4".AsOsAgnostic(),
                Series = _series,
                DownloadItem = downloadItem,
                SceneSource = true
            };

            Mocker.GetMock<IHistoryService>()
                .Setup(s => s.FindByDownloadId(downloadItem.DownloadId))
                .Returns(new List<EpisodeHistory>());

            Subject.Aggregate(localEpisode, downloadItem);

            localEpisode.Episodes.Should().BeEmpty();
        }

        [Test]
        public void should_not_use_grab_history_when_filename_parses()
        {
            var fileEpisodeInfo = Parser.Parser.ParseTitle("Series.Title.23.01.15");
            var parsedEpisodes = Builder<Episode>.CreateListOfSize(1).BuildList();

            var downloadItem = Builder<DownloadClientItem>.CreateNew()
                .With(d => d.DownloadId = "abc123")
                .Build();

            var localEpisode = new LocalEpisode
            {
                FileEpisodeInfo = fileEpisodeInfo,
                Path = @"C:\Test\Unsorted TV\Series.Title.23.01.15\Series.Title.23.01.15.mkv".AsOsAgnostic(),
                Series = _series,
                DownloadItem = downloadItem
            };

            Mocker.GetMock<IParsingService>()
                .Setup(s => s.GetEpisodes(fileEpisodeInfo, _series, localEpisode.SceneSource, null))
                .Returns(parsedEpisodes);

            Subject.Aggregate(localEpisode, downloadItem);

            localEpisode.Episodes.Should().BeEquivalentTo(parsedEpisodes);
            Mocker.GetMock<IHistoryService>()
                .Verify(v => v.FindByDownloadId(It.IsAny<string>()), Times.Never());
        }
    }
}
