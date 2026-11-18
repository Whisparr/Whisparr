using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Notifications;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.NotificationTests
{
    [TestFixture]
    public class NotificationServiceFixture : CoreTest<NotificationService>
    {
        private DownloadMessage _downloadMessage;

        [SetUp]
        public void Setup()
        {
            var notificationDefinition = new NotificationDefinition
            {
                Id = 1,
                Name = "Test Notification"
            };

            var notification = new Mock<INotification>();
            notification.SetupGet(v => v.Definition).Returns(notificationDefinition);
            notification.Setup(v => v.OnDownload(It.IsAny<DownloadMessage>()))
                        .Callback<DownloadMessage>(v => _downloadMessage = v);

            Mocker.GetMock<INotificationFactory>()
                  .Setup(v => v.OnDownloadEnabled(true))
                  .Returns(new List<INotification> { notification.Object });
        }

        [Test]
        public void should_not_prefix_release_date_with_release_year_in_download_message()
        {
            var series = new Series { Title = "Private" };
            var episodes = new List<Episode>
            {
                new Episode
                {
                    SeasonNumber = 2026,
                    AirDate = "2026-06-26",
                    Title = "Sherezade Lapiedra Has A Threesome With Her Boyfriend And A Nosey Neighbour"
                }
            };

            Subject.Handle(new EpisodeImportedEvent(
                new LocalEpisode
                {
                    Series = series,
                    Episodes = episodes,
                    Quality = new QualityModel(Quality.WEBDL1080p)
                },
                new EpisodeFile(),
                new List<EpisodeFile>(),
                true,
                null));

            _downloadMessage.Message.Should().Be("Private - 2026-06-26 - Sherezade Lapiedra Has A Threesome With Her Boyfriend And A Nosey Neighbour [WEBDL-1080p]");
        }

        [Test]
        public void should_separate_multiple_release_dates_in_download_message()
        {
            var series = new Series { Title = "Private" };
            var episodes = new List<Episode>
            {
                new Episode
                {
                    SeasonNumber = 2026,
                    AirDate = "2026-06-26",
                    Title = "First Scene"
                },
                new Episode
                {
                    SeasonNumber = 2026,
                    AirDate = "2026-06-27",
                    Title = "Second Scene"
                }
            };

            Subject.Handle(new EpisodeImportedEvent(
                new LocalEpisode
                {
                    Series = series,
                    Episodes = episodes,
                    Quality = new QualityModel(Quality.WEBDL1080p)
                },
                new EpisodeFile(),
                new List<EpisodeFile>(),
                true,
                null));

            _downloadMessage.Message.Should().Be("Private - 2026-06-26 + 2026-06-27 - First Scene + Second Scene [WEBDL-1080p]");
        }
    }
}
