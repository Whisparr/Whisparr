using NzbDrone.Core.Configuration;
using NzbDrone.Core.Configuration.Events;
using NzbDrone.Core.Localization;

namespace NzbDrone.Core.HealthCheck.Checks
{
    [CheckOn(typeof(ConfigSavedEvent))]
    public class MetadataUrlCheck : HealthCheckBase
    {
        private readonly IConfigFileProvider _configFileService;

        public MetadataUrlCheck(IConfigFileProvider configFileService, ILocalizationService localizationService)
            : base(localizationService)
        {
            _configFileService = configFileService;
        }

        public override HealthCheck Check()
        {
            var whisparrMetadata = _configFileService.WhisparrMetadata;
            if (whisparrMetadata == "https://api.whisparr.com/v3/{route}")
            {
                return new HealthCheck(GetType(), HealthCheckResult.Error, string.Format(_localizationService.GetLocalizedString("MetadataIncorrectUrlWarning"), _configFileService.Branch), "#config-metadata-url-mismatch");
            }

            return new HealthCheck(GetType());
        }
    }
}
