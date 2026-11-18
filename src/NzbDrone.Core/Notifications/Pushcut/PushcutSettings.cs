using System.Collections.Generic;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.Pushcut
{
    public class PushcutSettingsValidator : AbstractValidator<PushcutSettings>
    {
        public PushcutSettingsValidator()
        {
            RuleFor(settings => settings.ApiKey).NotEmpty();
            RuleFor(settings => settings.NotificationName).NotEmpty();
        }
    }

    public class PushcutSettings : NotificationSettingsBase<PushcutSettings>
    {
        private static readonly PushcutSettingsValidator Validator = new();

        [FieldDefinition(0, Label = "Notification name", Type = FieldType.Textbox, HelpText = "Notification name from Notifications tab of the Pushcut app.")]
        public string NotificationName { get; set; }

        [FieldDefinition(1, Label = "API Key", Type = FieldType.Textbox, Privacy = PrivacyLevel.ApiKey, HelpText = "API Keys can be managed in the Account view of the Pushcut app.")]
        public string ApiKey { get; set; }

        [FieldDefinition(2, Label = "Time sensitive", Type = FieldType.Checkbox, HelpText = "Check to mark the notification as \"Time-Sensitive\"")]
        public bool TimeSensitive { get; set; }

        [FieldDefinition(3, Label = "NotificationsPushcutSettingsIncludePoster", Type = FieldType.Checkbox, HelpText = "NotificationsPushcutSettingsIncludePosterHelpText")]
        public bool IncludePoster { get; set; }

        [FieldDefinition(4, Label = "NotificationsPushcutSettingsMetadataLinks", Type = FieldType.Select, SelectOptions = typeof(MetadataLinkType), HelpText = "NotificationsPushcutSettingsMetadataLinksHelpText")]
        public IEnumerable<int> MetadataLinks { get; set; } = [];

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
