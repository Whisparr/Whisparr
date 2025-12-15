using System.Linq;
using NLog;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Download;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.IndexerSearch
{
    public class SeasonSearchService : IExecute<SeasonSearchCommand>
    {
        private readonly ISearchForReleases _releaseSearchService;
        private readonly IProcessDownloadDecisions _processDownloadDecisions;
        private readonly IEpisodeService _episodeService;
        private readonly Logger _logger;

        public SeasonSearchService(ISearchForReleases releaseSearchService,
                                   IProcessDownloadDecisions processDownloadDecisions,
                                   IEpisodeService episodeService,
                                   Logger logger)
        {
            _releaseSearchService = releaseSearchService;
            _processDownloadDecisions = processDownloadDecisions;
            _episodeService = episodeService;
            _logger = logger;
        }

        public void Execute(SeasonSearchCommand message)
        {
            var episodes = _episodeService.GetEpisodesBySeason(message.SeriesId, message.SeasonNumber);

            // Filter to only missing (no file) and monitored episodes
            episodes = episodes.Where(e => !e.HasFile && e.Monitored).ToList();

            if (!episodes.Any())
            {
                _logger.ProgressInfo("No missing monitored episodes found for this season.");
                return;
            }

            var totalGrabbed = 0;

            foreach (var episode in episodes)
            {
                var decisions = _releaseSearchService.EpisodeSearch(episode, message.Trigger == CommandTrigger.Manual, false).GetAwaiter().GetResult();
                var processed = _processDownloadDecisions.ProcessDecisions(decisions).GetAwaiter().GetResult();

                totalGrabbed += processed.Grabbed.Count;
            }

            _logger.ProgressInfo("Season search completed. {0} reports downloaded.", totalGrabbed);
        }
    }
}
