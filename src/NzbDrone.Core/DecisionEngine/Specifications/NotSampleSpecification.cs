using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class NotSampleSpecification : IDecisionEngineSpecification
    {
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public NotSampleSpecification(IConfigService configService, Logger logger)
        {
            _configService = configService;
            _logger = logger;
        }

        public Decision IsSatisfiedBy(RemoteMovie subject, SearchCriteriaBase searchCriteria)
        {
            if (subject.Release.Title.ToLower().Contains("sample") && subject.Release.Size < 70.Megabytes())
            {
                // If the Runtime is validated do not validate on file size.
                if (!_configService.WhisparrValidateRuntime)
                {
                    _logger.Debug("Sample release, rejecting.");
                    return Decision.Reject("Sample");
                }
            }

            return Decision.Accept();
        }
    }
}
