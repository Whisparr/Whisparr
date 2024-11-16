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

        [FieldDefinition(3, Label = "Performers StashIDs",  HelpText = "Enter Performers StashIDs, comma seperated")]
        public string Performers { get; set; }

        [FieldDefinition(4, Label = "Studios StashIDs", HelpText = "Enter studios StashIDs, comma seperated (Optional)")]
        public string Studios { get; set; }

        [FieldDefinition(5, Label = "Studios Filter", Type = FieldType.Select, SelectOptions = typeof(FilterModifier), HelpText = "Filter studios by")]
        public FilterModifier StudiosFilter { get; set; }

        [FieldDefinition(6, Label = "Tag StashIDs", HelpText = "Enter tag StashIDs, comma seperated  (Optional)")]
        public string Tags { get; set; }

        [FieldDefinition(7, Label = "Tags Filter", Type = FieldType.Select, SelectOptions = typeof(FilterModifier), HelpText = "Filter tags by")]
        public FilterModifier TagsFilter { get; set; }

        [FieldDefinition(8, Label = "Only Favorite Studios", Type = FieldType.Checkbox,  HelpText = "Filter by favorite studios")]
        public bool OnlyFavoriteStudios { get; set; }
    }
}
