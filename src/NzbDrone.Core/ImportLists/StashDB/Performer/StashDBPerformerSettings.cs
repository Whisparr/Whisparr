using FluentValidation;
using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.ImportLists.StashDB.Performer
{
    public class StashDBPerformerSettingsValidator : StashDBSettingsBaseValidator<StashDBPerformerSettings>
    {
        public StashDBPerformerSettingsValidator()
        : base()
        {
            RuleFor(c => c.ApiKey)
                .NotEmpty()
                .WithMessage("Api Key must not be empty");

            RuleFor(c => c.Performers)
                .NotEmpty()
                .WithMessage("Performers StashIDs must not be empty");
        }
    }

    public class StashDBPerformerSettings : StashDBSettingsBase<StashDBPerformerSettings>
    {
        protected override AbstractValidator<StashDBPerformerSettings> Validator => new StashDBPerformerSettingsValidator();

        [FieldDefinition(0, Label = "Performers stashIDs",  HelpText = "Enter Performers StashIDs, Comma Seperated")]
        public string Performers { get; set; }

        [FieldDefinition(0, Label = "Studios stashIDs", HelpText = "Enter studios StashIDs, Comma Seperated (Optional)")]
        public string Studios { get; set; }

        [FieldDefinition(0, Label = "Tag stashIds", HelpText = "Enter studios StashIDs, Comma Seperated  (Optional)")]
        public string Tags { get; set; }

        [FieldDefinition(0, Label = "Only favorite studios", Type = FieldType.Checkbox,  HelpText = "Filter by favorite studios")]
        public bool OnlyFavoriteStudios { get; set; }
    }
}
