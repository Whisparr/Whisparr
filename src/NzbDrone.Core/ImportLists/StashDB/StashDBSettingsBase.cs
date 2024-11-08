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
        }
    }

    public class StashDBSettingsBase<TSettings> : IProviderConfig
        where TSettings : StashDBSettingsBase<TSettings>
    {
        protected virtual AbstractValidator<TSettings> Validator => new StashDBSettingsBaseValidator<TSettings>();

        public StashDBSettingsBase()
        {
            Sort = SceneSort.CREATED;
            ApiKey = "";
        }

        [FieldDefinition(0, Label = "Api Key", Privacy = PrivacyLevel.ApiKey, HelpText = "Your StashDB Api Key")]
        public string ApiKey { get; set; }

        [FieldDefinition(2, Label = "Sort Date Descending", Type = FieldType.Select, SelectOptions = typeof(SceneSort), HelpText = "Descending sort by date style")]
        public SceneSort Sort { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate((TSettings)this));
        }
    }
}
