using System;
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
            // If we have an external ID, prioritize matching by external ID
            if (!string.IsNullOrWhiteSpace(singleEpisodeSpec.ExternalId) && !string.IsNullOrWhiteSpace(remoteEpisode.ParsedEpisodeInfo.ExternalId))
            {
                if (singleEpisodeSpec.ExternalId.Equals(remoteEpisode.ParsedEpisodeInfo.ExternalId, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.Debug("External ID matches searched episode: {0}", singleEpisodeSpec.ExternalId);
                    return Decision.Accept();
                }
                else
                {
                    _logger.Debug("External ID does not match searched episode. Expected: {0}, Got: {1}", singleEpisodeSpec.ExternalId, remoteEpisode.ParsedEpisodeInfo.ExternalId);
                    return Decision.Reject("Wrong External ID");
                }
            }

            if (!singleEpisodeSpec.ReleaseDate.HasValue)
            {
                _logger.Debug("Searched episode has no release date, skipping.");
                return Decision.Reject("No Episode Release Date");
            }

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
