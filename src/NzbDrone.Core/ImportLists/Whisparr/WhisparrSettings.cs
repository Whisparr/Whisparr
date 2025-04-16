using System;
using System.Collections.Generic;
using FluentValidation;
using NzbDrone.Core.Annotations;
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

    public class WhisparrSettings : ImportListSettingsBase<WhisparrSettings>
    {
        private static readonly WhisparrSettingsValidator Validator = new WhisparrSettingsValidator();

        public WhisparrSettings()
        {
            ApiKey = "";
            ProfileIds = Array.Empty<int>();
            TagIds = Array.Empty<int>();
            RootFolderPaths = Array.Empty<string>();
        }

        [FieldDefinition(0, Label = "ImportListsWhisparrSettingsFullUrl", HelpText = "ImportListsWhisparrSettingsFullUrlHelpText")]
        public string BaseUrl { get; set; } = string.Empty;

        [FieldDefinition(1, Label = "ApiKey", Privacy = PrivacyLevel.ApiKey, HelpText = "ImportListsWhisparrSettingsApiKeyHelpText")]
        public string ApiKey { get; set; }

        [FieldDefinition(2, Type = FieldType.Select, SelectOptionsProviderAction = "getProfiles", Label = "QualityProfiles", HelpText = "ImportListsWhisparrSettingsQualityProfilesHelpText")]
        public IEnumerable<int> ProfileIds { get; set; }

        [FieldDefinition(3, Type = FieldType.Select, SelectOptionsProviderAction = "getTags", Label = "Tags", HelpText = "ImportListsWhisparrSettingsTagsHelpText")]
        public IEnumerable<int> TagIds { get; set; }

        [FieldDefinition(4, Type = FieldType.Select, SelectOptionsProviderAction = "getRootFolders", Label = "RootFolders", HelpText = "ImportListsWhisparrSettingsRootFoldersHelpText")]
        public IEnumerable<string> RootFolderPaths { get; set; }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
