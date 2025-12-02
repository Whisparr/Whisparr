using System.Collections.Generic;
using System.IO;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Download;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.MediaFiles.MovieImport.Aggregation.Aggregators.Augmenters.Quality;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.MediaInfo
{
    [TestFixture]
    public class UpdateMediaInfoServiceFixture : CoreTest<UpdateMediaInfoService>
    {
        private Movie _movie;

        [SetUp]
        public void Setup()
        {
            _movie = new Movie
            {
                Id = 1,
                Path = @"C:\movie".AsOsAgnostic()
            };

            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.EnableMediaInfo)
                  .Returns(true);
        }

        private void GivenFileExists()
        {
            Mocker.GetMock<IDiskProvider>()
                  .Setup(v => v.FileExists(It.IsAny<string>()))
                  .Returns(true);
        }

        private void GivenSuccessfulScan()
        {
            Mocker.GetMock<IVideoFileInfoReader>()
                  .Setup(v => v.GetMediaInfo(It.IsAny<string>()))
                  .Returns(new MediaInfoModel());
        }

        private void GivenFailedScan(string path)
        {
            Mocker.GetMock<IVideoFileInfoReader>()
                  .Setup(v => v.GetMediaInfo(path))
                  .Returns((MediaInfoModel)null);
        }

        [Test]
        public void should_skip_up_to_date_media_info()
        {
            var movieFiles = Builder<MovieFile>.CreateListOfSize(3)
                .All()
                .With(v => v.Path = null)
                .With(v => v.RelativePath = "media.mkv")
                .TheFirst(1)
                .With(v => v.MediaInfo = new MediaInfoModel { SchemaRevision = VideoFileInfoReader.CURRENT_MEDIA_INFO_SCHEMA_REVISION })
                .BuildList();

            Mocker.GetMock<IMediaFileService>()
                  .Setup(v => v.GetFilesByMovie(1))
                  .Returns(movieFiles);

            GivenFileExists();
            GivenSuccessfulScan();

            Subject.Handle(new MovieScannedEvent(_movie, new List<string>()));

            Mocker.GetMock<IVideoFileInfoReader>()
                  .Verify(v => v.GetMediaInfo(Path.Combine(_movie.Path, "media.mkv")), Times.Exactly(2));

            Mocker.GetMock<IMediaFileService>()
                  .Verify(v => v.Update(It.IsAny<MovieFile>()), Times.Exactly(2));
        }

        [Test]
        public void should_skip_not_yet_date_media_info()
        {
            var movieFiles = Builder<MovieFile>.CreateListOfSize(3)
                .All()
                .With(v => v.Path = null)
                .With(v => v.RelativePath = "media.mkv")
                .TheFirst(1)
                .With(v => v.MediaInfo = new MediaInfoModel { SchemaRevision = VideoFileInfoReader.MINIMUM_MEDIA_INFO_SCHEMA_REVISION })
                .BuildList();

            Mocker.GetMock<IMediaFileService>()
                  .Setup(v => v.GetFilesByMovie(1))
                  .Returns(movieFiles);

            GivenFileExists();
            GivenSuccessfulScan();

            Subject.Handle(new MovieScannedEvent(_movie, new List<string>()));

            Mocker.GetMock<IVideoFileInfoReader>()
                  .Verify(v => v.GetMediaInfo(Path.Combine(_movie.Path, "media.mkv")), Times.Exactly(2));

            Mocker.GetMock<IMediaFileService>()
                  .Verify(v => v.Update(It.IsAny<MovieFile>()), Times.Exactly(2));
        }

        [Test]
        public void should_update_outdated_media_info()
        {
            var movieFiles = Builder<MovieFile>.CreateListOfSize(3)
                .All()
                .With(v => v.Path = null)
                .With(v => v.RelativePath = "media.mkv")
                .TheFirst(1)
                .With(v => v.MediaInfo = new MediaInfoModel())
                .BuildList();

            Mocker.GetMock<IMediaFileService>()
                  .Setup(v => v.GetFilesByMovie(1))
                  .Returns(movieFiles);

            GivenFileExists();
            GivenSuccessfulScan();

            Subject.Handle(new MovieScannedEvent(_movie, new List<string>()));

            Mocker.GetMock<IVideoFileInfoReader>()
                  .Verify(v => v.GetMediaInfo(Path.Combine(_movie.Path, "media.mkv")), Times.Exactly(3));

            Mocker.GetMock<IMediaFileService>()
                  .Verify(v => v.Update(It.IsAny<MovieFile>()), Times.Exactly(3));
        }

        [Test]
        public void should_ignore_missing_files()
        {
            var movieFiles = Builder<MovieFile>.CreateListOfSize(2)
                   .All()
                   .With(v => v.RelativePath = "media.mkv")
                   .BuildList();

            Mocker.GetMock<IMediaFileService>()
                  .Setup(v => v.GetFilesByMovie(1))
                  .Returns(movieFiles);

            GivenSuccessfulScan();

            Subject.Handle(new MovieScannedEvent(_movie, new List<string>()));

            Mocker.GetMock<IVideoFileInfoReader>()
                  .Verify(v => v.GetMediaInfo("media.mkv"), Times.Never());

            Mocker.GetMock<IMediaFileService>()
                  .Verify(v => v.Update(It.IsAny<MovieFile>()), Times.Never());
        }

        [Test]
        public void should_continue_after_failure()
        {
            var movieFiles = Builder<MovieFile>.CreateListOfSize(2)
                   .All()
                   .With(v => v.Path = null)
                   .With(v => v.RelativePath = "media.mkv")
                   .TheFirst(1)
                   .With(v => v.RelativePath = "media2.mkv")
                   .BuildList();

            Mocker.GetMock<IMediaFileService>()
                  .Setup(v => v.GetFilesByMovie(1))
                  .Returns(movieFiles);

            GivenFileExists();
            GivenSuccessfulScan();
            GivenFailedScan(Path.Combine(_movie.Path, "media2.mkv"));

            Subject.Handle(new MovieScannedEvent(_movie, new List<string>()));

            Mocker.GetMock<IVideoFileInfoReader>()
                  .Verify(v => v.GetMediaInfo(Path.Combine(_movie.Path, "media.mkv")), Times.Exactly(1));

            Mocker.GetMock<IMediaFileService>()
                  .Verify(v => v.Update(It.IsAny<MovieFile>()), Times.Exactly(1));
        }

        [Test]
        public void should_not_update_files_if_media_info_disabled()
        {
            var movieFiles = Builder<MovieFile>.CreateListOfSize(2)
                .All()
                .With(v => v.RelativePath = "media.mkv")
                .TheFirst(1)
                .With(v => v.RelativePath = "media2.mkv")
                .BuildList();

            Mocker.GetMock<IMediaFileService>()
                .Setup(v => v.GetFilesByMovie(1))
                .Returns(movieFiles);

            Mocker.GetMock<IConfigService>()
                .SetupGet(s => s.EnableMediaInfo)
                .Returns(false);

            GivenFileExists();
            GivenSuccessfulScan();

            Subject.Handle(new MovieScannedEvent(_movie, new List<string>()));

            Mocker.GetMock<IVideoFileInfoReader>()
                .Verify(v => v.GetMediaInfo(It.IsAny<string>()), Times.Never());

            Mocker.GetMock<IMediaFileService>()
                .Verify(v => v.Update(It.IsAny<MovieFile>()), Times.Never());
        }

        [Test]
        public void should_not_update_if_media_info_disabled()
        {
            var movieFile = Builder<MovieFile>.CreateNew()
                .With(v => v.RelativePath = "media.mkv")
                .Build();

            Mocker.GetMock<IConfigService>()
                .SetupGet(s => s.EnableMediaInfo)
                .Returns(false);

            GivenFileExists();
            GivenSuccessfulScan();

            Subject.Update(movieFile, _movie);

            Mocker.GetMock<IVideoFileInfoReader>()
                .Verify(v => v.GetMediaInfo(It.IsAny<string>()), Times.Never());

            Mocker.GetMock<IMediaFileService>()
                .Verify(v => v.Update(It.IsAny<MovieFile>()), Times.Never());
        }

        [Test]
        public void should_update_media_info()
        {
            var movieFile = Builder<MovieFile>.CreateNew()
                .With(v => v.Path = null)
                .With(v => v.RelativePath = "media.mkv")
                .With(e => e.MediaInfo = new MediaInfoModel { SchemaRevision = 3 })
                .Build();

            GivenFileExists();
            GivenSuccessfulScan();

            Subject.Update(movieFile, _movie);

            Mocker.GetMock<IVideoFileInfoReader>()
                .Verify(v => v.GetMediaInfo(Path.Combine(_movie.Path, "media.mkv")), Times.Once());

            Mocker.GetMock<IMediaFileService>()
                .Verify(v => v.Update(movieFile), Times.Once());
        }

        [Test]
        public void should_not_update_media_info_if_new_info_is_null()
        {
            var movieFile = Builder<MovieFile>.CreateNew()
                .With(v => v.RelativePath = "media.mkv")
                .With(e => e.MediaInfo = new MediaInfoModel { SchemaRevision = 3 })
                .Build();

            GivenFileExists();
            GivenFailedScan(Path.Combine(_movie.Path, "media.mkv"));

            Subject.Update(movieFile, _movie);

            movieFile.MediaInfo.Should().NotBeNull();
        }

        [Test]
        public void should_not_save_movie_file_if_new_info_is_null()
        {
            var movieFile = Builder<MovieFile>.CreateNew()
                .With(v => v.RelativePath = "media.mkv")
                .With(e => e.MediaInfo = new MediaInfoModel { SchemaRevision = 3 })
                .Build();

            GivenFileExists();
            GivenFailedScan(Path.Combine(_movie.Path, "media.mkv"));

            Subject.Update(movieFile, _movie);

            Mocker.GetMock<IMediaFileService>()
                .Verify(v => v.Update(movieFile), Times.Never());
        }

        [Test]
        public void should_not_update_media_info_if_file_does_not_support_media_info()
        {
            var path = Path.Combine(_movie.Path, "media.iso");

            var movieFile = Builder<MovieFile>.CreateNew()
                .With(v => v.Path = path)
                .Build();

            GivenFileExists();
            GivenFailedScan(path);

            Subject.Update(movieFile, _movie);

            Mocker.GetMock<IVideoFileInfoReader>()
                .Verify(v => v.GetMediaInfo(path), Times.Once());

            Mocker.GetMock<IMediaFileService>()
                .Verify(v => v.Update(movieFile), Times.Never());
        }

        [Test]
        public void should_set_webdl_2160_when_filename_indicates_web_and_mediainfo_indicates_2160()
        {
            var path = Path.Combine(_movie.Path, "vixen.2022-01-21.24.hours.christian.clay.rae.lil.black.4k.hevc.mp4");

            var movieFile = Builder<MovieFile>.CreateNew()
                .With(v => v.Path = path)
                .With(v => v.RelativePath = Path.GetFileName(path))
                .Build();

            GivenFileExists();

            // MediaInfo reader returns a 2160p file
            Mocker.GetMock<IVideoFileInfoReader>()
                  .Setup(v => v.GetMediaInfo(path))
                  .Returns(new MediaInfoModel { Width = 3840, Height = 2160 });

            // Create a filename augmenter that indicates a Web source
            var fileNameAugment = new Mock<IAugmentQuality>();
            fileNameAugment.SetupGet(s => s.Order).Returns(1);
            fileNameAugment.Setup(s => s.AugmentQuality(It.IsAny<LocalMovie>(), It.IsAny<DownloadClientItem>()))
                           .Returns(AugmentQualityResult.SourceOnly(QualitySource.Web, Confidence.Default));

            // Create a mediainfo augmenter that indicates resolution only (2160p)
            var mediaInfoAugment = new Mock<IAugmentQuality>();
            mediaInfoAugment.SetupGet(s => s.Order).Returns(4);
            mediaInfoAugment.Setup(s => s.AugmentQuality(It.IsAny<LocalMovie>(), It.IsAny<DownloadClientItem>()))
                            .Returns(AugmentQualityResult.ResolutionOnly((int)Resolution.R2160p, Confidence.MediaInfo));

            // Inject augmenters into AggregateQuality
            Mocker.SetConstant<IEnumerable<IAugmentQuality>>(new[] { fileNameAugment.Object, mediaInfoAugment.Object });

            Subject.Update(movieFile, _movie);

            movieFile.Quality.Should().NotBeNull();
            movieFile.Quality.Quality.Should().Be(Quality.WEBDL2160p);
            movieFile.Quality.ResolutionDetectionSource.Should().Be(QualityDetectionSource.MediaInfo);
        }
    }
}
