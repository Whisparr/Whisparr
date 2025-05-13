using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Localization;
using NzbDrone.Core.Organizer;

namespace NzbDrone.Core.HealthCheck.Checks
{
    [CheckOn(typeof(ModelEvent<NamingConfig>))]
    public class NamingConfigCheck : HealthCheckBase, IProvideHealthCheck
    {
        private readonly INamingConfigService _namingConfigService;

        public NamingConfigCheck(INamingConfigService namingConfigService, ILocalizationService localizationService)
            : base(localizationService)
        {
            _namingConfigService = namingConfigService;
        }

        public override HealthCheck Check()
        {
            return new HealthCheck(GetType());
        }
    }
}
