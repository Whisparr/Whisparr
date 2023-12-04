using System;
using System.Collections.Generic;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.Whisparr
{
    public class WhisparrSettingsValidator : AbstractValidator<WhisparrSettings>
    {
        public WhisparrSettingsValidator()
        {
            RuleFor(c => c.BaseUrl).ValidRootUrl();
            RuleFor(c => c.ApiKey).NotEmpty();
        }
    }

    public class WhisparrSettings : IProviderConfig
    {
        private static readonly WhisparrSettingsValidator Validator = new WhisparrSettingsValidator();

        public WhisparrSettings()
        {
            BaseUrl = "";
            ApiKey = "";
            ProfileIds = Array.Empty<int>();
            TagIds = Array.Empty<int>();
            RootFolderPaths = Array.Empty<string>();
        }

        [FieldDefinition(0, Label = "Full URL", HelpText = "URL, including port, of the Whisparr instance to import from (Whisparr 3.0 or higher)")]
        public string BaseUrl { get; set; }

        [FieldDefinition(1, Label = "API Key", Privacy = PrivacyLevel.ApiKey, HelpText = "Apikey of the Whisparr instance to import from (Whisparr 3.0 or higher)")]
        public string ApiKey { get; set; }

        [FieldDefinition(2, Type = FieldType.Select, SelectOptionsProviderAction = "getProfiles", Label = "Profiles", HelpText = "Profiles from the source instance to import from")]
        public IEnumerable<int> ProfileIds { get; set; }

        [FieldDefinition(3, Type = FieldType.Select, SelectOptionsProviderAction = "getTags", Label = "Tags", HelpText = "Tags from the source instance to import from")]
        public IEnumerable<int> TagIds { get; set; }

        [FieldDefinition(4, Type = FieldType.Select, SelectOptionsProviderAction = "getRootFolders", Label = "Root Folders", HelpText = "Root Folders from the source instance to import from")]
        public IEnumerable<string> RootFolderPaths { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
