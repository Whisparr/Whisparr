using System;
using System.Linq;
using NUnit.Framework;
using NzbDrone.Core.ImportLists.ThePornDb;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ImportListTests.TPDb
{
    public class TPDbSceneSettingsValidatorFixture : CoreTest
    {
        [TestCase("", TestName = "Invalid_ApiKey_Empty")]
        [TestCase("validApiKey", TestName = "Valid_ApiKey")]
        public void Validate_ApiKey_Should_Contain_Errors(string apiKey)
        {
            var settings = new TPDbSceneSettings
            {
                ApiKey = apiKey,
                OrderBy = "SomeOrderBy",
                Date = "2023-12-01",
                DateContext = "SomeContext"
            };

            var result = settings.Validate();
            Assert.AreEqual(string.IsNullOrEmpty(apiKey), result.Errors.Any(e => e.PropertyName == nameof(settings.ApiKey)));
        }

        [TestCase("", TestName = "Invalid_OrderBy_Empty")]
        [TestCase("SomeOrderBy", TestName = "Valid_OrderBy")]
        public void Validate_OrderBy_Should_Contain_Errors(string orderBy)
        {
            var settings = new TPDbSceneSettings
            {
                ApiKey = "validApiKey",
                OrderBy = orderBy,
                Date = "2023-12-01",
                DateContext = "SomeContext"
            };

            var result = settings.Validate();
            Assert.AreEqual(string.IsNullOrEmpty(orderBy), result.Errors.Any(e => e.PropertyName == nameof(settings.OrderBy)));
        }

        [TestCase("2023-02-30", "SomeContext", TestName = "Invalid_Non_Existent_Date")]
        [TestCase("2023-12-01", "SomeContext", TestName = "Valid_Date")]
        [TestCase("2023-13-01", "SomeContext", TestName = "Invalid_Invalid_Date_Format")]
        [TestCase("2023-02-30", null, TestName = "Invalid_Non_Existent_Date_and_Null_DateContext")]
        [TestCase("2023-12-01", null, TestName = "Invalid_Valid_Date_Null_DateContext")]
        [TestCase("2023-13-01", null, TestName = "Invalid_Invalid_Date_Format_Null_DateContext")]
        [TestCase("2023-02-30", "", TestName = "Invalid_Non_Existent_Date_Empty_DateContext")]
        [TestCase("2023-12-01", "", TestName = "Invalid_Valid_Date_Empty_DateContext")]
        [TestCase("2023-13-01", "", TestName = "Invalid_Invalid_Date_Format_Empty_DateContext")]
        public void Validate_Date_Should_Contain_Errors(string date, string dateContext)
        {
            var settings = new TPDbSceneSettings
            {
                ApiKey = "validApiKey",
                OrderBy = "SomeOrderBy",
                Date = date,
                DateContext = dateContext
            };

            var result = settings.Validate();
            Assert.AreEqual(!DateTime.TryParseExact(date, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out _),
                result.Errors.Any(e => e.PropertyName == nameof(settings.Date)));
        }

        [TestCase(null, TestName = "DateContext_Null_When_Date_Provided")]
        [TestCase("", TestName = "DateContext_Empty_When_Date_Provided")]
        [TestCase("SomeContext", TestName = "Valid_DateContext_When_Date_Provided")]
        public void Validate_DateContext_Should_Contain_Errors(string dateContext)
        {
            var settings = new TPDbSceneSettings
            {
                ApiKey = "validApiKey",
                OrderBy = "SomeOrderBy",
                Date = "2023-12-01",
                DateContext = dateContext
            };

            var result = settings.Validate();
            var expectedError = !string.IsNullOrEmpty(settings.Date) && string.IsNullOrEmpty(dateContext);
            Assert.AreEqual(expectedError, result.Errors.Any(e => e.PropertyName == nameof(settings.DateContext)));
        }

        [TestCase("", "", "", "", false, TestName = "Invalid_All_Fields_Empty")]
        [TestCase("validApiKey", "", "", "", false, TestName = "Invalid_OrderBy_Empty")]
        [TestCase("validApiKey", "OrderByValue", "2023-13-01", "", false, TestName = "Invalid_Invalid_Date_Format")]
        [TestCase("validApiKey", "OrderByValue", "2023-02-30", "", false, TestName = "Invalid_Non_Existent_Date")]
        [TestCase("validApiKey", "OrderByValue", "2023-12-01", "", false, TestName = "Invalid_DateContext_Empty")]
        [TestCase("validApiKey", "OrderByValue", "2023-12-01", "SomeContext", true, TestName = "Valid_All_Fields_Provided")]
        [TestCase("validApiKey", "OrderByValue", null, null, true, TestName = "Valid_Optional_Date_And_Context_Not_Provided")]
        public void Validate_CustomSettings_Should_Be_Valid_Or_Invalid(string apiKey, string orderBy, string date, string dateContext, bool isValid)
        {
            var settings = new TPDbSceneSettings
            {
                ApiKey = apiKey,
                OrderBy = orderBy,
                Date = date,
                DateContext = dateContext
            };

            var result = settings.Validate();
            Assert.AreEqual(isValid, result.IsValid);
        }
    }
}
