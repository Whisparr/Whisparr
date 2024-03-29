using FluentAssertions;
using NUnit.Framework;

namespace NzbDrone.Integration.Test.ApiTests
{
    [TestFixture]
    public class NamingConfigFixture : IntegrationTest
    {
        [Test]
        public void should_be_able_to_get()
        {
            NamingConfig.GetSingle().Should().NotBeNull();
        }

        [Test]
        public void should_be_able_to_get_by_id()
        {
            var config = NamingConfig.GetSingle();
            NamingConfig.Get(config.Id).Should().NotBeNull();
            NamingConfig.Get(config.Id).Id.Should().Be(config.Id);
        }

        [Test]
        public void should_be_able_to_update()
        {
            var config = NamingConfig.GetSingle();
            config.RenameEpisodes = false;
            config.StandardEpisodeFormat = "{Site Title} - {Release-Date} - {Episode Title}";

            var result = NamingConfig.Put(config);
            result.RenameEpisodes.Should().BeFalse();
            result.StandardEpisodeFormat.Should().Be(config.StandardEpisodeFormat);
        }

        [Test]
        public void should_get_bad_request_if_standard_format_is_empty()
        {
            var config = NamingConfig.GetSingle();
            config.RenameEpisodes = true;
            config.StandardEpisodeFormat = "";

            var errors = NamingConfig.InvalidPut(config);
            errors.Should().NotBeNull();
        }

        [Test]
        public void should_get_bad_request_if_standard_format_doesnt_contain_season_and_episode()
        {
            var config = NamingConfig.GetSingle();
            config.RenameEpisodes = true;
            config.StandardEpisodeFormat = "{season}";

            var errors = NamingConfig.InvalidPut(config);
            errors.Should().NotBeNull();
        }

        [Test]
        public void should_get_bad_request_if_daily_format_doesnt_contain_season_and_episode_or_air_date()
        {
            var config = NamingConfig.GetSingle();
            config.RenameEpisodes = true;
            config.StandardEpisodeFormat = "{Site Title} - {season}x{episode:00} - {Episode Title}";

            var errors = NamingConfig.InvalidPut(config);
            errors.Should().NotBeNull();
        }

        [Test]
        public void should_not_require_format_when_rename_episodes_is_false()
        {
            var config = NamingConfig.GetSingle();
            config.RenameEpisodes = false;
            config.StandardEpisodeFormat = "";

            var errors = NamingConfig.InvalidPut(config);
            errors.Should().NotBeNull();
        }

        [Test]
        public void should_require_format_when_rename_episodes_is_true()
        {
            var config = NamingConfig.GetSingle();
            config.RenameEpisodes = true;
            config.StandardEpisodeFormat = "";

            var errors = NamingConfig.InvalidPut(config);
            errors.Should().NotBeNull();
        }

        [Test]
        public void should_get_bad_request_if_series_folder_format_does_not_contain_series_title()
        {
            var config = NamingConfig.GetSingle();
            config.RenameEpisodes = true;
            config.SeriesFolderFormat = "This and That";

            var errors = NamingConfig.InvalidPut(config);
            errors.Should().NotBeNull();
        }
    }
}
