using System;
using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.TPDb
{
    public class CustomSettingsValidator : AbstractValidator<TPDbPerformerSettings>
    {
        public CustomSettingsValidator()
        {
            RuleFor(c => c.PerformerId).NotEmpty()
                                       .Must(x => Guid.TryParse(x, out var val))
                                       .When(x => x.PerformerId.IsNotNullOrWhiteSpace() && x.PerformerId.Contains('-'), ApplyConditionTo.CurrentValidator)
                                       .WithMessage("Invalid Performer guid provided")
                                       .Must(x => int.TryParse(x, out var val) && val > 0)
                                       .When(x => x.PerformerId.IsNotNullOrWhiteSpace() && !x.PerformerId.Contains('-'), ApplyConditionTo.CurrentValidator)
                                       .WithMessage("Invalid Performer id provided");
        }
    }

    public class TPDbPerformerSettings : IImportListSettings
    {
        private static readonly CustomSettingsValidator Validator = new CustomSettingsValidator();

        public TPDbPerformerSettings()
        {
            BaseUrl = "https://api.whisparr.com/v3";
        }

        public string BaseUrl { get; set; }

        [FieldDefinition(0, Label = "Performer Id", HelpText = "The TPDb Performer Guid")]
        public string PerformerId { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
