using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.Notifiarr
{
    public class NotifiarrSettingsValidator : AbstractValidator<NotifiarrSettings>
    {
        public NotifiarrSettingsValidator()
        {
            RuleFor(c => c.ApiKey).NotEmpty();
        }
    }

    public class NotifiarrSettings : NotificationSettingsBase<NotifiarrSettings>
    {
        private static readonly NotifiarrSettingsValidator Validator = new();

        [FieldDefinition(0, Label = "API Key", Privacy = PrivacyLevel.ApiKey, HelpText = "Your API key from your profile", HelpLink = "https://notifiarr.com")]
        public string ApiKey { get; set; }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
