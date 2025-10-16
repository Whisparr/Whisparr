using FluentValidation;
using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.ImportLists.StashDB.Studio
{
    public class StashDBTagsSettingsValidator : StashDBSettingsBaseValidator<StashDBTagsSettings>
    {
        public StashDBTagsSettingsValidator()
        : base()
        {
            RuleFor(c => c.ApiKey)
                .NotEmpty()
                .WithMessage("Api Key must not be empty");
        }
    }

    public class StashDBTagsSettings : StashDBSettingsBase<StashDBTagsSettings>
    {
        protected override AbstractValidator<StashDBTagsSettings> Validator => new StashDBTagsSettingsValidator();

        [FieldDefinition(4, Label = "Tag StashIDs", HelpText = "Enter Tags StashIDs, comma seperated (Optional)")]
        public string Tags { get; set; }

        [FieldDefinition(5, Label = "Tags Filter", Type = FieldType.Select, SelectOptions = typeof(FilterModifier), HelpText = "Filter tags by")]
        public FilterModifier TagsFilter { get; set; }
    }
}
