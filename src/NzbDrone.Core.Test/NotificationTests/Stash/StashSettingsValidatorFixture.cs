using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Notifications.Stash;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.NotificationTests.Stash
{
    [TestFixture]
    public class StashSettingsValidatorFixture : CoreTest
    {
        [Test]
        public void should_not_require_creation_defaults_when_sync_is_disabled()
        {
            var settings = new StashSettings
            {
                Host = "localhost",
                Port = 9999,
                PerformerSyncMode = StashPerformerSyncMode.Disabled
            };

            settings.Validate().IsValid.Should().BeTrue();
        }

        [Test]
        public void should_require_creation_defaults_when_syncing_to_whisparr()
        {
            var settings = new StashSettings
            {
                Host = "localhost",
                Port = 9999,
                PerformerSyncMode = StashPerformerSyncMode.StashToWhisparr
            };

            settings.Validate().IsValid.Should().BeFalse();
            settings.Validate().Errors.Should().Contain(error => error.PropertyName == nameof(StashSettings.PerformerSyncRootFolderPath));
            settings.Validate().Errors.Should().Contain(error => error.PropertyName == nameof(StashSettings.PerformerSyncQualityProfileId));
        }
    }
}
