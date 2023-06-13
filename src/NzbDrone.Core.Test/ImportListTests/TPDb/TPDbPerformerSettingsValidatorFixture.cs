using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.ImportLists.TPDb;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ImportListTests.TPDb
{
    public class TPDbPerformerSettingsValidatorFixture : CoreTest
    {
        [TestCase("")]
        [TestCase("a")]
        [TestCase("asdf5-asdf45")]
        [TestCase(null)]
        public void invalid_performerId_should_not_validate(string performerId)
        {
            var setting = new TPDbPerformerSettings
            {
                PerformerId = performerId,
            };

            setting.Validate().IsValid.Should().BeFalse();
            setting.Validate().Errors.Should().Contain(c => c.PropertyName == "PerformerId");
        }

        [TestCase("1")]
        [TestCase("82885")]
        [TestCase("164502")]
        [TestCase("a05a86a7-5b5f-41cd-ad02-213101cae2b0")]
        public void valid_performerId_should_validate(string performerId)
        {
            var setting = new TPDbPerformerSettings
            {
                PerformerId = performerId,
            };

            setting.Validate().IsValid.Should().BeTrue();
            setting.Validate().Errors.Should().NotContain(c => c.PropertyName == "PerformerId");
        }
    }
}
