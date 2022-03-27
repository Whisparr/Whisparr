using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.WhisparrList
{
    public class WhisparrSettingsValidator : AbstractValidator<WhisparrListSettings>
    {
        public WhisparrSettingsValidator()
        {
            RuleFor(c => c.Url).ValidRootUrl();
        }
    }

    public class WhisparrListSettings : IProviderConfig
    {
        private static readonly WhisparrSettingsValidator Validator = new WhisparrSettingsValidator();

        [FieldDefinition(0, Label = "List URL", HelpText = "The URL for the movie list")]
        public string Url { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
