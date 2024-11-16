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

        [FieldDefinition(3, Label = "Studios StashIDs", HelpText = "Enter Studios StashIDs, comma seperated")]
        public string Studios { get; set; }

        [FieldDefinition(4, Label = "Performers Filter", Type = FieldType.Select, SelectOptions = typeof(FilterModifier), HelpText = "Filter performers by")]
        public FilterModifier PerformersFilter { get; set; }

        [FieldDefinition(5, Label = "Tag StashIDs", HelpText = "Enter Tags StashIDs, comma seperated (Optional)")]
        public string Tags { get; set; }

        [FieldDefinition(6, Label = "Tags Filter", Type = FieldType.Select, SelectOptions = typeof(FilterModifier), HelpText = "Filter tags by")]
        public FilterModifier TagsFilter { get; set; }

        [FieldDefinition(7, Label = "Only Favorite Performers", Type = FieldType.Checkbox,  HelpText = "Filter by favorite performers")]
        public bool OnlyFavoritePerformers { get; set; }
    }
}
