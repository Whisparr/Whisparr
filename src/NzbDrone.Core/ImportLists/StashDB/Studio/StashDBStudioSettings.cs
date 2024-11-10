using FluentValidation;
using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.ImportLists.StashDB.Studio
{
    public class StashDBPerformerSettingsValidator : StashDBSettingsBaseValidator<StashDBStudioSettings>
    {
        public StashDBPerformerSettingsValidator()
        : base()
        {
            RuleFor(c => c.ApiKey)
                .NotEmpty()
                .WithMessage("Api Key must not be empty");

            RuleFor(c => c.Studios)
                .NotEmpty()
                .WithMessage("Studios StashIDs must not be empty");
        }
    }

    public class StashDBStudioSettings : StashDBSettingsBase<StashDBStudioSettings>
    {
        protected override AbstractValidator<StashDBStudioSettings> Validator => new StashDBPerformerSettingsValidator();

        [FieldDefinition(0, Label = "Studios stashIDs", HelpText = "Enter studios StashIDs, Comma Seperated")]
        public string Studios { get; set; }

        [FieldDefinition(0, Label = "Tag stashIds", HelpText = "Enter studios StashIDs, Comma Seperated  (Optional)")]
        public string Tags { get; set; }

        [FieldDefinition(0, Label = "Only favorite performers", Type = FieldType.Checkbox,  HelpText = "Filter by favorite performers")]
        public bool OnlyFavoritePerformers { get; set; }
    }
}
