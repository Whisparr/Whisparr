using System;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles.EpisodeImport;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.EpisodeImport
{
    [TestFixture]
    public class DetectSampleFixture : CoreTest<DetectSample>
    {
        private Series _series;
        private LocalEpisode _localEpisode;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>.CreateNew()
                                     .With(s => s.Runtime = 30)
                                     .Build();

            var episodes = Builder<Episode>.CreateListOfSize(1)
                                           .All()
                                           .With(e => e.SeasonNumber = 1)
                                           .Build()
                                           .ToList();

            _localEpisode = new LocalEpisode
                                {
                                    Path = @"C:\Test\30 Rock\30.rock.s01e01.avi",
                                    Episodes = episodes,
                                    Series = _series,
                                    Quality = new QualityModel(Quality.HDTV720p)
                                };
        }

        private void GivenRuntime(int seconds)
        {
            Mocker.GetMock<IVideoFileInfoReader>()
                  .Setup(s => s.GetRunTime(It.IsAny<string>()))
                  .Returns(new TimeSpan(0, 0, seconds));
        }

        [Test]
        public void should_return_false_for_flv()
        {
            _localEpisode.Path = @"C:\Test\some.show.s01e01.flv";

            ShouldBeNotSample();

            Mocker.GetMock<IVideoFileInfoReader>().Verify(c => c.GetRunTime(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_return_false_for_strm()
        {
            _localEpisode.Path = @"C:\Test\some.show.s01e01.strm";

            ShouldBeNotSample();

            Mocker.GetMock<IVideoFileInfoReader>().Verify(c => c.GetRunTime(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_use_runtime()
        {
            GivenRuntime(120);

            Subject.IsSample(_localEpisode.Series,
                             _localEpisode.Path);

            Mocker.GetMock<IVideoFileInfoReader>().Verify(v => v.GetRunTime(It.IsAny<string>()), Times.Once());
        }

        [Test]
        public void should_return_true_if_runtime_is_less_than_minimum()
        {
            GivenRuntime(60);

            ShouldBeSample();
        }

        [Test]
        public void should_return_false_if_runtime_greater_than_minimum()
        {
            GivenRuntime(600);

            ShouldBeNotSample();
        }

        [Test]
        public void should_return_false_if_runtime_greater_than_webisode_minimum()
        {
            _series.Runtime = 6;
            GivenRuntime(299);

            ShouldBeNotSample();
        }

        [Test]
        public void should_return_false_if_runtime_greater_than_anime_short_minimum()
        {
            _series.Runtime = 2;
            GivenRuntime(60);

            ShouldBeNotSample();
        }

        [Test]
        public void should_return_true_if_runtime_less_than_anime_short_minimum()
        {
            _series.Runtime = 2;
            GivenRuntime(10);

            ShouldBeSample();
        }

        [Test]
        public void should_return_indeterminate_if_mediainfo_result_is_null()
        {
            Mocker.GetMock<IVideoFileInfoReader>()
                  .Setup(s => s.GetRunTime(It.IsAny<string>()))
                  .Returns((TimeSpan?)null);

            Subject.IsSample(_localEpisode.Series,
                             _localEpisode.Path).Should().Be(DetectSampleResult.Indeterminate);

            ExceptionVerification.ExpectedErrors(1);
        }

        private void ShouldBeSample()
        {
            Subject.IsSample(_localEpisode.Series,
                             _localEpisode.Path).Should().Be(DetectSampleResult.Sample);
        }

        private void ShouldBeNotSample()
        {
            Subject.IsSample(_localEpisode.Series,
                             _localEpisode.Path).Should().Be(DetectSampleResult.NotSample);
        }
    }
}
