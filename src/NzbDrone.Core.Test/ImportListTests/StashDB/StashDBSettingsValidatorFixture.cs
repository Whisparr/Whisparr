using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.ImportLists.StashDB;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ImportListTests.StashDB
{
    public class StashDBSettingsValidatorFixture : CoreTest
    {
        [TestCase("")]
        [TestCase(null)]
        public void invalid_api_key_should_not_validate(string apiKey)
        {
            var settings = new StashDBSettings
            {
                ApiKey = apiKey,
            };

            settings.Validate().IsValid.Should().BeFalse();
            settings.Validate().Errors.Should().Contain(c => c.PropertyName == "ApiKey");
        }

        [TestCase("validApiKey123")]
        [TestCase("anotherValidApiKey456")]
        public void valid_api_key_should_validate(string apiKey)
        {
            var settings = new StashDBSettings
            {
                ApiKey = apiKey,
            };

            settings.Validate().IsValid.Should().BeTrue();
            settings.Validate().Errors.Should().NotContain(c => c.PropertyName == "ApiKey");
        }
    }
}
