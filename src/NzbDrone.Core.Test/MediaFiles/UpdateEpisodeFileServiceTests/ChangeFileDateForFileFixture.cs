using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.UpdateEpisodeFileServiceTests
{
    [TestFixture]
    public class ChangeFileDateForFileFixture : CoreTest<UpdateEpisodeFileService>
    {
        private readonly DateTime _veryOldAirDateUtc = new(1965, 01, 01, 0, 0, 0, 512, 512, DateTimeKind.Utc);
        private DateTime _lastWrite = new(2025, 07, 27, 12, 0, 0, 512, 512, DateTimeKind.Utc);
        private Series _series;
        private EpisodeFile _episodeFile;
        private string _seriesFolder;
        private List<Episode> _episodes;

        [SetUp]
        public void Setup()
        {
            _seriesFolder = @"C:\Test\TV\Series Title".AsOsAgnostic();

            _series = Builder<Series>.CreateNew()
                                     .With(s => s.Path = _seriesFolder)
                                     .Build();

            _episodes = Builder<Episode>.CreateListOfSize(1)
                                        .All()
                                        .With(e => e.AirDateUtc = _lastWrite.AddDays(2))
                                        .With(e => e.AirDate = _lastWrite.AddDays(2).ToString("yyyy-MM-dd"))
                                        .Build()
                                        .ToList();

            _episodeFile = Builder<EpisodeFile>.CreateNew()
                                               .With(f => f.Path = Path.Combine(_series.Path, "Season 1", "Series Title - S01E01.mkv").AsOsAgnostic())
                                               .With(f => f.RelativePath = @"Season 1\Series Title - S01E01.mkv".AsOsAgnostic())
                                               .Build();

            Mocker.GetMock<IDiskProvider>()
                .Setup(x => x.FileGetLastWrite(_episodeFile.Path))
                .Returns(() => _lastWrite);

            Mocker.GetMock<IDiskProvider>()
                .Setup(x => x.FileSetLastWriteTime(_episodeFile.Path, It.IsAny<DateTime>()))
                .Callback<string, DateTime>((path, dateTime) =>
                {
                    _lastWrite = dateTime.Kind == DateTimeKind.Utc
                        ? dateTime
                        : dateTime.ToUniversalTime();
                });

            Mocker.GetMock<IConfigService>()
                .Setup(x => x.FileDate)
                .Returns(FileDateType.LocalAirDate);
        }

        [Test]
        public void should_change_date_once_only()
        {
            var previousWrite = new DateTime(_lastWrite.Ticks, _lastWrite.Kind);

            Subject.ChangeFileDateForFile(_episodeFile, _series, _episodes);
            Subject.ChangeFileDateForFile(_episodeFile, _series, _episodes);

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.FileSetLastWriteTime(_episodeFile.Path, It.IsAny<DateTime>()), Times.Once());

            var actualWriteTime = Mocker.GetMock<IDiskProvider>().Object.FileGetLastWrite(_episodeFile.Path).ToLocalTime();
            actualWriteTime.Should().Be(DateTime.Parse(_episodes[0].AirDate + " 12:00").WithTicksFrom(previousWrite));
        }

        [Test]
        public void should_clamp_mtime_on_posix()
        {
            PosixOnly();

            var previousWrite = new DateTime(_lastWrite.Ticks, _lastWrite.Kind);
            _episodes[0].AirDateUtc = _veryOldAirDateUtc;
            _episodes[0].AirDate = _veryOldAirDateUtc.ToString("yyyy-MM-dd");

            Subject.ChangeFileDateForFile(_episodeFile, _series, _episodes);
            Subject.ChangeFileDateForFile(_episodeFile, _series, _episodes);

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.FileSetLastWriteTime(_episodeFile.Path, It.IsAny<DateTime>()), Times.Once());

            var actualWriteTime = Mocker.GetMock<IDiskProvider>().Object.FileGetLastWrite(_episodeFile.Path).ToLocalTime();
            actualWriteTime.Should().Be(DateTimeExtensions.EpochTime.ToLocalTime().WithTicksFrom(previousWrite));
        }

        [Test]
        public void should_not_clamp_mtime_on_windows()
        {
            WindowsOnly();

            var previousWrite = new DateTime(_lastWrite.Ticks, _lastWrite.Kind);
            _episodes[0].AirDateUtc = _veryOldAirDateUtc;
            _episodes[0].AirDate = _veryOldAirDateUtc.ToString("yyyy-MM-dd");

            Subject.ChangeFileDateForFile(_episodeFile, _series, _episodes);
            Subject.ChangeFileDateForFile(_episodeFile, _series, _episodes);

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.FileSetLastWriteTime(_episodeFile.Path, It.IsAny<DateTime>()), Times.Once());

            var actualWriteTime = Mocker.GetMock<IDiskProvider>().Object.FileGetLastWrite(_episodeFile.Path).ToLocalTime();
            actualWriteTime.Should().Be(DateTime.Parse(_episodes[0].AirDate + " 12:00").WithTicksFrom(previousWrite));
        }
    }
}
