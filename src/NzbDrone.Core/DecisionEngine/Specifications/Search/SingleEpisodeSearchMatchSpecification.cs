using System;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.DecisionEngine.Specifications.Search
{
    public class SingleEpisodeSearchMatchSpecification : IDownloadDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public SingleEpisodeSearchMatchSpecification(Logger logger)
        {
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public DownloadSpecDecision IsSatisfiedBy(RemoteEpisode remoteEpisode, SearchCriteriaBase searchCriteria)
        {
            if (searchCriteria == null)
            {
                return DownloadSpecDecision.Accept();
            }

            var singleEpisodeSpec = searchCriteria as SingleEpisodeSearchCriteria;
            if (singleEpisodeSpec != null)
            {
                return IsSatisfiedBy(remoteEpisode, singleEpisodeSpec);
            }

            return DownloadSpecDecision.Accept();
        }

        private DownloadSpecDecision IsSatisfiedBy(RemoteEpisode remoteEpisode, SingleEpisodeSearchCriteria singleEpisodeSpec)
        {
            // Check if we matched by external ID
            if (!singleEpisodeSpec.ExternalId.IsNullOrWhiteSpace() &&
                !remoteEpisode.ParsedEpisodeInfo.ExternalId.IsNullOrWhiteSpace() &&
                string.Equals(singleEpisodeSpec.ExternalId, remoteEpisode.ParsedEpisodeInfo.ExternalId, StringComparison.OrdinalIgnoreCase))
            {
                _logger.Debug("Release matched by external ID: {0}", singleEpisodeSpec.ExternalId);
                return DownloadSpecDecision.Accept();
            }

            if (!singleEpisodeSpec.ReleaseDate.HasValue)
            {
                _logger.Debug("Searched episode has no release date, skipping.");
                return DownloadSpecDecision.Reject(DownloadRejectionReason.NoEpisodeReleaseDate, "No Episode Release Date");
            }

            // TODO match by performer or release date
            var releaseDate = singleEpisodeSpec.ReleaseDate.Value.ToString(Episode.AIR_DATE_FORMAT);

            if (releaseDate != remoteEpisode.ParsedEpisodeInfo.AirDate)
            {
                _logger.Debug("Release date does not match searched episode, skipping.");
                return DownloadSpecDecision.Reject(DownloadRejectionReason.WrongEpisode, "Wrong Episode");
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

            return DownloadSpecDecision.Accept();
        }
    }
}
