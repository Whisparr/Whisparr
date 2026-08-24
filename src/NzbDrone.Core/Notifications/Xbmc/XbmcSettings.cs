using System.ComponentModel;
using FluentValidation;
using Newtonsoft.Json;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.Xbmc
{
    public class XbmcSettingsValidator : AbstractValidator<XbmcSettings>
    {
        public XbmcSettingsValidator()
        {
            RuleFor(c => c.Host).ValidHost();
            RuleFor(c => c.DisplayTime).GreaterThanOrEqualTo(2);
            RuleFor(c => c.UrlBase).ValidUrlBase();
        }
    }

    public class XbmcSettings : NotificationSettingsBase<XbmcSettings>
    {
        private static readonly XbmcSettingsValidator Validator = new ();

        public XbmcSettings()
        {
            Port = 8080;
            UrlBase = "/jsonrpc";
            DisplayTime = 5;
        }

        [FieldDefinition(0, Label = "Host")]
        public string Host { get; set; }

        [FieldDefinition(1, Label = "Port")]
        public int Port { get; set; }

        [FieldDefinition(2, Label = "Use SSL", Type = FieldType.Checkbox, HelpText = "Connect to Kodi over HTTPS instead of HTTP")]
        public bool UseSsl { get; set; }

        [FieldDefinition(3, Label = "URL Base", Type = FieldType.Textbox, Advanced = true, HelpText = "Adds a prefix to the Kodi URL, e.g. http://[host]:[port]/[urlBase]/jsonrpc")]
        public string UrlBase { get; set; }

        [FieldDefinition(4, Label = "Username", Privacy = PrivacyLevel.UserName)]
        public string Username { get; set; }

        [FieldDefinition(5, Label = "Password", Type = FieldType.Password, Privacy = PrivacyLevel.Password)]
        public string Password { get; set; }

        [DefaultValue(5)]
        [FieldDefinition(6, Label = "Display Time", HelpText = "How long the notification will be displayed for (In seconds)")]
        public int DisplayTime { get; set; }

        [FieldDefinition(7, Label = "GUI Notification", Type = FieldType.Checkbox)]
        public bool Notify { get; set; }

        [FieldDefinition(8, Label = "Update Library", HelpText = "Update Library on Import & Rename?", Type = FieldType.Checkbox)]
        public bool UpdateLibrary { get; set; }

        [FieldDefinition(9, Label = "Clean Library", HelpText = "Clean Library after update?", Type = FieldType.Checkbox)]
        public bool CleanLibrary { get; set; }

        [FieldDefinition(10, Label = "Always Update", HelpText = "Update Library even when a video is playing?", Type = FieldType.Checkbox)]
        public bool AlwaysUpdate { get; set; }

        [JsonIgnore]
        public string Address => $"{Host.ToUrlHost()}:{Port}{UrlBase}";

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
