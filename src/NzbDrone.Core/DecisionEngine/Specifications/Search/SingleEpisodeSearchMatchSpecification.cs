using System;
using System.Linq;
using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.DecisionEngine.Specifications.Search
{
    public class SingleEpisodeSearchMatchSpecification : IDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public SingleEpisodeSearchMatchSpecification(Logger logger)
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

            var singleEpisodeSpec = searchCriteria as SingleEpisodeSearchCriteria;
            if (singleEpisodeSpec != null)
            {
                return IsSatisfiedBy(remoteEpisode, singleEpisodeSpec);
            }

            return Decision.Accept();
        }

        private Decision IsSatisfiedBy(RemoteEpisode remoteEpisode, SingleEpisodeSearchCriteria singleEpisodeSpec)
        {
            // TODO match by performer or release date
            var releaseDate = singleEpisodeSpec.ReleaseDate.Value.ToString(Episode.AIR_DATE_FORMAT);

            if (releaseDate != remoteEpisode.ParsedEpisodeInfo.AirDate)
            {
                _logger.Debug("Release date does not match searched episode, skipping.");
                return Decision.Reject("Wrong Episode");
            }

            // if (!remoteEpisode.ParsedEpisodeInfo.EpisodeNumbers.Any())
            // {
            //     _logger.Debug("Full season result during single episode search, skipping.");
            //     return Decision.Reject("Full season pack");
            // }

            // if (!remoteEpisode.ParsedEpisodeInfo.EpisodeNumbers.Contains(singleEpisodeSpec.EpisodeNumber))
            // {
            //     _logger.Debug("Episode number does not match searched episode number, skipping.");
            //     return Decision.Reject("Wrong episode");
            // }

            return Decision.Accept();
        }
    }
}
