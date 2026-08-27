using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class AcceptableSizeSpecification : IDownloadDecisionEngineSpecification
    {
        private readonly IEpisodeService _episodeService;
        private readonly Logger _logger;

        public AcceptableSizeSpecification(IEpisodeService episodeService, Logger logger)
        {
            _episodeService = episodeService;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public DownloadSpecDecision IsSatisfiedBy(RemoteEpisode subject, ReleaseDecisionInformation information)
        {
            _logger.Debug("Beginning size check for: {0}", subject);

            var quality = subject.ParsedEpisodeInfo.Quality.Quality;

            if (subject.Release.Size == 0)
            {
                _logger.Debug("Release has unknown size, skipping size check");
                return DownloadSpecDecision.Accept();
            }

            var seriesRuntime = subject.Series.Runtime;
            var runtime = 0;

            // For each episode use the runtime of the episode or fallback to the series runtime
            // (which in turn might have fallen back to a default runtime of 45)
            foreach (var episode in subject.Episodes)
            {
                runtime += episode.Runtime > 0 ? episode.Runtime : seriesRuntime;
            }

            // Reject if the run time is 0
            if (runtime == 0)
            {
                _logger.Debug("Runtime of all episodes is 0, unable to validate size until it is available, rejecting");
                return DownloadSpecDecision.Reject(DownloadRejectionReason.UnknownRuntime, "Runtime of all episodes is 0, unable to validate size until it is available");
            }

            var qualityProfile = subject.Series.QualityProfile.Value;
            var qualityIndex = qualityProfile.GetIndex(quality, true);
            var qualityOrGroup = qualityProfile.Items[qualityIndex.Index];
            var item = qualityOrGroup.Quality == null ? qualityOrGroup.Items[qualityIndex.GroupIndex] : qualityOrGroup;

            if (item.MinSize.HasValue)
            {
                var minSize = item.MinSize.Value.Megabytes();

                // Multiply maxSize by runtime of all episodes
                minSize *= runtime;

                // If the parsed size is smaller than minSize we don't want it
                if (subject.Release.Size < minSize)
                {
                    var runtimeMessage = subject.Episodes.Count == 1 ? $"{runtime}min" : $"{subject.Episodes.Count}x {runtime}min";

                    _logger.Debug("Item: {0}, Size: {1} is smaller than minimum allowed size ({2} bytes for {3}), rejecting.", subject, subject.Release.Size, minSize, runtimeMessage);
                    return DownloadSpecDecision.Reject(DownloadRejectionReason.BelowMinimumSize, "{0} is smaller than minimum allowed {1} (for {2})", subject.Release.Size.SizeSuffix(), minSize.SizeSuffix(), runtimeMessage);
                }
            }

            if (!item.MaxSize.HasValue || item.MaxSize.Value == 0)
            {
                _logger.Debug("Max size is unlimited, skipping size check");
            }
            else
            {
                var maxSize = item.MaxSize.Value.Megabytes();

                // Multiply maxSize by runtime of all episodes
                maxSize *= runtime;

                // If the parsed size is greater than maxSize we don't want it
                if (subject.Release.Size > maxSize)
                {
                    var runtimeMessage = subject.Episodes.Count == 1 ? $"{runtime}min" : $"{subject.Episodes.Count}x {runtime}min";

                    _logger.Debug("Item: {0}, Size: {1} is greater than maximum allowed size ({2} for {3}), rejecting", subject, subject.Release.Size, maxSize, runtimeMessage);
                    return DownloadSpecDecision.Reject(DownloadRejectionReason.AboveMaximumSize, "{0} is larger than maximum allowed {1} (for {2})", subject.Release.Size.SizeSuffix(), maxSize.SizeSuffix(), runtimeMessage);
                }
            }

            _logger.Debug("Item: {0}, meets size constraints", subject);
            return DownloadSpecDecision.Accept();
        }
    }
}
