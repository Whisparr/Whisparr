using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications.Search
{
    public class SeasonMatchSpecification : IDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public SeasonMatchSpecification(Logger logger)
        {
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public Decision IsSatisfiedBy(RemoteEpisode remoteEpisode, SearchCriteriaBase searchCriteria)
        {
            if (searchCriteria == null)
            {
                return Decision.Accept();
            }

            var singleEpisodeSpec = searchCriteria as SeasonSearchCriteria;
            if (singleEpisodeSpec == null)
            {
                return Decision.Accept();
            }

            if (singleEpisodeSpec.Year != remoteEpisode.ParsedEpisodeInfo.SeasonNumber)
            {
                _logger.Debug("Year does not match searched year, skipping.");
                return Decision.Reject("Wrong year");
            }

            return Decision.Accept();
        }
    }
}
