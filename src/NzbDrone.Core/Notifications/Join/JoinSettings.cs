using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.Join
{
    public class JoinSettingsValidator : AbstractValidator<JoinSettings>
    {
        public JoinSettingsValidator()
        {
            RuleFor(s => s.ApiKey).NotEmpty();
            RuleFor(s => s.DeviceIds).Empty().WithMessage("Use Device Names instead");
        }
    }

    public class JoinSettings : NotificationSettingsBase<JoinSettings>
    {
        private static readonly JoinSettingsValidator Validator = new JoinSettingsValidator();

        public JoinSettings()
        {
            Priority = (int)JoinPriority.Normal;
        }

        [FieldDefinition(0, Label = "API Key", HelpText = "The API Key from your Join account settings (click Join API button).", HelpLink = "https://joinjoaomgcd.appspot.com/")]
        public string ApiKey { get; set; }

        [FieldDefinition(1, Label = "Device IDs", HelpText = "Deprecated, use Device Names instead. Comma separated list of Device IDs you'd like to send notifications to. If unset, all devices will receive notifications.")]
        public string DeviceIds { get; set; }

        [FieldDefinition(2, Label = "Device Names", HelpText = "Comma separated list of full or partial device names you'd like to send notifications to. If unset, all devices will receive notifications.", HelpLink = "https://joaoapps.com/join/api/")]
        public string DeviceNames { get; set; }

        [FieldDefinition(3, Label = "Notification Priority", Type = FieldType.Select, SelectOptions = typeof(JoinPriority))]
        public int Priority { get; set; }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
