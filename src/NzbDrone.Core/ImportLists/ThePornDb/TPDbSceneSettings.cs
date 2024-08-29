using System;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.ThePornDb
{
    public class CustomSettingsValidator : AbstractValidator<TPDbSceneSettings>
    {
        public CustomSettingsValidator()
        {
            RuleFor(c => c.ApiKey).NotEmpty().WithMessage("API Key must not be empty");
            RuleFor(c => c.OrderBy).NotEmpty().WithMessage("Must select a sort method");
            RuleFor(c => c.Date)
                .NotEmpty()
                .When(x => !string.IsNullOrEmpty(x.DateContext))
                .WithMessage("Date must not be empty if Date Context is provided")
                .Matches(@"^\d{4}-\d{2}-\d{2}$")
                .When(x => !string.IsNullOrEmpty(x.Date))
                .WithMessage("Date must be in the format YYYY-MM-DD")
                .Must(date =>
                {
                    return DateTime.TryParseExact(date, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out _);
                })
                .When(x => !string.IsNullOrEmpty(x.Date))
                .WithMessage("Date must be a valid calendar date");
            RuleFor(x => x.DateContext)
                .NotEmpty()
                .When(x => !string.IsNullOrEmpty(x.Date))
                .WithMessage("DateContext must not be empty if Date is provided");
        }
    }

    public class TPDbSceneSettings : ITPDbSettings
    {
        // Maximum page size the api will allow
        private readonly int _PerPageLimit = 100;
        private static readonly CustomSettingsValidator Validator = new CustomSettingsValidator();
        private readonly string _BaseUrl = "https://api.theporndb.net";

        public TPDbSceneSettings()
        {
        }

        [FieldDefinition(0, Label = "API Key", HelpText = "The TPDb API Key", Type = FieldType.Textbox, Privacy = PrivacyLevel.ApiKey)]
        public string ApiKey { get; set; }

        [FieldDefinition(1, Type = FieldType.Select, SelectOptionsProviderAction = "getOrderBy", Label = "Sort", HelpText = "Sort by one of the following")]
        public string OrderBy { get; set; }

        [FieldDefinition(2, Label = "Date", HelpText = "Filter by date with format (YYYY-MM-DD)", Type = FieldType.Textbox)]
        public string Date { get; set; }

        [FieldDefinition(3, Type = FieldType.Select, SelectOptionsProviderAction = "getDateContext", Label = "Sort", HelpText = "Filter by date")]
        public string DateContext { get; set; }

        [FieldDefinition(4, Type = FieldType.Select, SelectOptions = typeof(TriPosition), Label = "Collection", HelpText = "Filter by Collection")]
        public TriPosition Collected { get; set; }

        [FieldDefinition(5, Type = FieldType.Select, SelectOptions = typeof(TriPosition), Label = "Favourites", HelpText = "Filter by Favourites")]
        public TriPosition Favourites { get; set; }

        public int PerPageLimit
        {
            get
            {
                return _PerPageLimit;
            }
        }

        public string BaseUrl
        {
            get
            {
                return _BaseUrl;
            }
            set
            {
            }
        }

        public NzbDroneValidationResult Validate() => new NzbDroneValidationResult(Validator.Validate(this));

        public enum TriPosition
        {
            True = 1,
            False = 0,
            None = -1
        }
    }
}
