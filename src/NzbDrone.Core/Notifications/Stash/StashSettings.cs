using FluentValidation;
using Newtonsoft.Json;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.Stash
{
    public class StashSettingsValidator : AbstractValidator<StashSettings>
    {
        public StashSettingsValidator()
        {
            RuleFor(c => c.Host).ValidHost();
            RuleFor(c => c.Port).ValidPort();
            RuleFor(c => c.MapFrom).NotEmpty().Unless(c => c.MapTo.IsNullOrWhiteSpace());
            RuleFor(c => c.MapTo).NotEmpty().Unless(c => c.MapFrom.IsNullOrWhiteSpace());
            RuleFor(c => c.GenerateImagePreviews)
                .Equal(false)
                .Unless(c => c.GeneratePreviews)
                .WithMessage("Generate Previews must also be enabled");
            RuleFor(c => c.PerformerSyncRootFolderPath)
                .NotEmpty()
                .When(c => c.SyncsToWhisparr)
                .WithMessage("A root folder is required when syncing favorites to Whisparr");
            RuleFor(c => c.PerformerSyncQualityProfileId)
                .GreaterThan(0)
                .When(c => c.SyncsToWhisparr)
                .WithMessage("A quality profile is required when syncing favorites to Whisparr");
        }
    }

    public class StashSettings : NotificationSettingsBase<StashSettings>, IProviderConfig
    {
        private static readonly StashSettingsValidator Validator = new StashSettingsValidator();

        public StashSettings()
        {
            Port = 9999;
            StashBoxEndpoint = "https://stashdb.org/graphql";
        }

        [FieldDefinition(0, Label = "Host")]
        public string Host { get; set; }

        [FieldDefinition(1, Label = "Port")]
        public int Port { get; set; }

        [FieldDefinition(2, Label = "Use SSL", Type = FieldType.Checkbox, HelpText = "Connect to Stash over HTTPS instead of HTTP")]
        public bool UseSsl { get; set; }

        [FieldDefinition(3, Label = "API Key", Privacy = PrivacyLevel.ApiKey)]
        public string ApiKey { get; set; }

        [FieldDefinition(4, Label = "Scan: Generate Covers", HelpText = "Generate covers for new media during scan", Type = FieldType.Checkbox)]
        public bool GenerateCovers { get; set; }

        [FieldDefinition(5, Label = "Scan: Generate Previews", HelpText = "Generate previews for new media during scan", Type = FieldType.Checkbox)]
        public bool GeneratePreviews { get; set; }

        [FieldDefinition(6, Label = "Scan: Generate Image Previews", HelpText = "Generate image previews for new media during scan", Type = FieldType.Checkbox)]
        public bool GenerateImagePreviews { get; set; }

        [FieldDefinition(7, Label = "Scan: Generate Sprites", HelpText = "Generate sprites for new media during scan", Type = FieldType.Checkbox)]
        public bool GenerateSprites { get; set; }

        [FieldDefinition(8, Label = "Scan: Generate Phashes", HelpText = "Generate phash for new media during scan", Type = FieldType.Checkbox)]
        public bool GeneratePhashes { get; set; }

        [FieldDefinition(9, Label = "Run Identify Task", HelpText = "Run Metadata Identify task on new files", Type = FieldType.Checkbox)]
        public bool MetadataIdentify { get; set; }

        [FieldDefinition(10, Label = "Identify: Stash Box Endpoint", HelpText = "The Url for the Stash Box Endpoint (https://stashdb.org/graphql)", Type = FieldType.Textbox)]
        public string StashBoxEndpoint { get; set; }

        [FieldDefinition(11, Label = "Identify: Builtin Autotag", HelpText = "The Source Builtin Autotag", Type = FieldType.Checkbox)]
        public bool BuiltinAutotag { get; set; }

        [FieldDefinition(12, Label = "Identify: Include Male Performers", HelpText = "Include Male Performers during Identify task", Type = FieldType.Checkbox)]
        public bool IncludeMalePerformers { get; set; }

        [FieldDefinition(13, Label = "Identify: Set Cover Image", HelpText = "Set the cover image during Identify task", Type = FieldType.Checkbox)]
        public bool SetCoverImage { get; set; }

        [FieldDefinition(14, Label = "Identify: Skip Multiple Matches", HelpText = "Skip matches that have more than one result", Type = FieldType.Checkbox)]
        public bool SkipMultipleMatches { get; set; }

        [FieldDefinition(15, Label = "Identify: Skip Multiple Match Tag ID", HelpText = "Tag ID skipped matches with", Type = FieldType.Number)]
        public int SkipMultipleMatchTag { get; set; }

        [FieldDefinition(16, Label = "Identify: Set Organized", HelpText = "Use Set Organized during Identify task", Type = FieldType.Checkbox)]
        public bool SetOrganized { get; set; }

        [FieldDefinition(17, Label = "Map Paths From", Type = FieldType.Textbox, Advanced = true, HelpText = "Whisparr Path, Used to modify site paths when Stash sees library path location differently from Whisparr")]
        public string MapFrom { get; set; }

        [FieldDefinition(18, Label = "Map Paths To", Type = FieldType.Textbox, Advanced = true, HelpText = "Stash Path, Used to modify site paths when Stash sees library path location differently from Whisparr")]
        public string MapTo { get; set; }

        [FieldDefinition(19, Label = "Performer Sync Mode", Type = FieldType.Select, SelectOptions = typeof(StashPerformerSyncMode), HelpText = "Synchronize Whisparr performer monitoring with favorites in this local Stash instance")]
        public StashPerformerSyncMode PerformerSyncMode { get; set; }

        [FieldDefinition(20, Label = "Performer Sync Root Folder", Type = FieldType.Select, SelectOptionsProviderAction = "getRootFolders", HelpText = "Root folder used when a Stash favorite creates a performer in Whisparr")]
        public string PerformerSyncRootFolderPath { get; set; }

        [FieldDefinition(21, Label = "Performer Sync Quality Profile", Type = FieldType.Select, SelectOptionsProviderAction = "getQualityProfiles", HelpText = "Quality profile used when a Stash favorite creates a performer in Whisparr")]
        public int PerformerSyncQualityProfileId { get; set; }

        [FieldDefinition(22, Label = "Performer Sync State", Type = FieldType.Textbox, Hidden = HiddenType.Hidden)]
        public string PerformerSyncState { get; set; }

        [JsonIgnore]
        public string Address => $"{Host.ToUrlHost()}:{Port}";

        public bool IsValid => !string.IsNullOrWhiteSpace(Host) && Port > 0;

        [JsonIgnore]
        public bool SyncsToWhisparr => PerformerSyncMode == StashPerformerSyncMode.StashToWhisparr || PerformerSyncMode == StashPerformerSyncMode.Bidirectional;

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
