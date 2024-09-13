using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.StashDB
{
    public class StashDBSettingsValidator : AbstractValidator<StashDBSettings>
    {
        public StashDBSettingsValidator()
        {
            RuleFor(c => c.ApiKey)
                .NotEmpty()
                .WithMessage("Api Key must not be empty");
        }
    }

    public class StashDBSettings : IProviderConfig
    {
        private static readonly StashDBSettingsValidator Validator = new StashDBSettingsValidator();

        public StashDBSettings()
        {
            Filter = FavoriteFilter.ALL;
            Sort = SceneSort.RELEASED;
            ApiKey = "";
        }

        [FieldDefinition(0, Label = "Api Key", Privacy = PrivacyLevel.ApiKey, HelpText = "Your StashDB Api Key")]
        public string ApiKey { get; set; }

        [FieldDefinition(1, Label = "Favorite Filter", Type = FieldType.Select, SelectOptions = typeof(FavoriteFilter), HelpText = "Filter by favorited entity")]
        public FavoriteFilter Filter { get; set; }

        [FieldDefinition(2, Label = "Sort Date Descending", Type =FieldType.Select, SelectOptions =typeof(SceneSort), HelpText = "Descending sort by date style")]
        public SceneSort Sort { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
