using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.StashDB
{
    public class StashDBSettingsBaseValidator<TSettings> : AbstractValidator<TSettings>
    where TSettings : StashDBSettingsBase<TSettings>
    {
        public StashDBSettingsBaseValidator()
        {
            RuleFor(c => c.ApiKey)
                .NotEmpty()
                .WithMessage("Api Key must not be empty");

            // Limit not smaller than 1 and not larger than 100
            RuleFor(c => c.Limit)
                .GreaterThan(0)
                .WithMessage("Must be integer greater than 0");
        }
    }

    public class StashDBSettingsBase<TSettings> : IProviderConfig
        where TSettings : StashDBSettingsBase<TSettings>
    {
        protected virtual AbstractValidator<TSettings> Validator => new StashDBSettingsBaseValidator<TSettings>();

        public StashDBSettingsBase()
        {
            Limit = 100;
            Sort = SceneSort.CREATED;
            ApiKey = "";
        }

        [FieldDefinition(0, Label = "Api Key", Privacy = PrivacyLevel.ApiKey, HelpText = "Your StashDB Api Key")]
        public string ApiKey { get; set; }

        [FieldDefinition(1, Label = "Limit", HelpText = "Limit the number of movies to get")]
        public int Limit { get; set; }

        [FieldDefinition(2, Label = "Sort Date Descending", Type = FieldType.Select, SelectOptions = typeof(SceneSort), HelpText = "Descending sort by date style")]
        public SceneSort Sort { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate((TSettings)this));
        }
    }
}
