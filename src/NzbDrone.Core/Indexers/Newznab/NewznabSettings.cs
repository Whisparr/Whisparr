using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Equ;
using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Indexers.Newznab
{
    public enum DateSearchFormat
    {
        YearMonthDay = 0,
        DayMonthYear = 1,
        Both = 2
    }

    public enum SeriesNameSource
    {
        Site = 0,
        Network = 1,
        Both = 2
    }

    public class NewznabSettingsValidator : AbstractValidator<NewznabSettings>
    {
        private static readonly string[] ApiKeyAllowList =
        {
            "nzbs.org",
            "nzb.life",
            "dognzb.cr",
            "nzbplanet.net",
            "nzbid.org",
            "nzbndx.com",
            "nzbindex.in"
        };

        private static bool ShouldHaveApiKey(NewznabSettings settings)
        {
            return settings.BaseUrl != null && ApiKeyAllowList.Any(c => settings.BaseUrl.ToLowerInvariant().Contains(c));
        }

        private static readonly Regex AdditionalParametersRegex = new(@"(&.+?\=.+?)+", RegexOptions.Compiled);

        public NewznabSettingsValidator()
        {
            RuleFor(c => c).Custom((c, context) =>
            {
                if (c.Categories.Empty())
                {
                    context.AddFailure("'Categories' must be provided");
                }
            });

            RuleFor(c => c.BaseUrl).ValidRootUrl();
            RuleFor(c => c.ApiPath).ValidUrlBase("/api");
            RuleFor(c => c.ApiKey).NotEmpty().When(ShouldHaveApiKey);
            RuleFor(c => c.AdditionalParameters).Matches(AdditionalParametersRegex)
                                                .When(c => !c.AdditionalParameters.IsNullOrWhiteSpace());
        }
    }

    public class NewznabSettings : PropertywiseEquatable<NewznabSettings>, IIndexerSettings
    {
        private static readonly NewznabSettingsValidator Validator = new();

        public NewznabSettings()
        {
            ApiPath = "/api";
            Categories = new[] { 6000, 6010, 6020, 6030, 6040, 6045, 6050, 6070, 6080, 6090 };
            DateSearchFormat = DateSearchFormat.YearMonthDay;
            SeriesNameSource = SeriesNameSource.Site;
            FailDownloads = Array.Empty<int>();
        }

        [FieldDefinition(0, Label = "URL")]
        public string BaseUrl { get; set; }

        [FieldDefinition(1, Label = "API Path", HelpText = "Path to the api, usually /api", Advanced = true)]
        public string ApiPath { get; set; }

        [FieldDefinition(2, Label = "API Key", Privacy = PrivacyLevel.ApiKey)]
        public string ApiKey { get; set; }

        [FieldDefinition(3, Label = "Categories", Type = FieldType.Select, SelectOptionsProviderAction = "newznabCategories", HelpText = "Drop down list, leave blank to disable standard/daily shows")]
        public IEnumerable<int> Categories { get; set; }

        [FieldDefinition(6, Label = "Additional Parameters", HelpText = "Additional Newznab parameters", Advanced = true)]
        public string AdditionalParameters { get; set; }

        [FieldDefinition(7, Label = "Search Title Only", Type = FieldType.Checkbox, HelpText = "Search using title only (without date/year).", Advanced = true)]
        public bool SearchTitleOnly { get; set; }

        [FieldDefinition(8, Label = "Search Site + Title", Type = FieldType.Checkbox, HelpText = "Search using site name + title (without date/year).", Advanced = true)]
        public bool SearchSiteTitleOnly { get; set; }

        [FieldDefinition(9, Label = "Search External ID Only", Type = FieldType.Checkbox, HelpText = "Search using external ID only (without date/year/title).", Advanced = true)]
        public bool SearchExternalIdOnly { get; set; }

        [FieldDefinition(10, Label = "Date Format", Type = FieldType.Select, SelectOptions = typeof(DateSearchFormat), HelpText = "Date format to use in searches: YY.MM.DD, DD.MM.YY, or Both", Advanced = true)]
        public DateSearchFormat DateSearchFormat { get; set; }

        [FieldDefinition(11, Label = "Network or Site", Type = FieldType.Select, SelectOptions = typeof(SeriesNameSource), HelpText = "Choose between Site Name, Network Name, or both for series searches", Advanced = true)]
        public SeriesNameSource SeriesNameSource { get; set; }

        // Field 12 is used by TorznabSettings MinimumSeeders
        // Field 13 is used by TorznabSettings SeedCriteria

        [FieldDefinition(14, Type = FieldType.Select, SelectOptions = typeof(FailDownloads), Label = "Fail Downloads", HelpText = "Mark downloads containing these file types as failed", Advanced = true)]
        public IEnumerable<int> FailDownloads { get; set; }

        // If you need to add another field here, update TorznabSettings as well and this comment

        public virtual NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
