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
        private readonly ISeriesService _seriesService;
        private readonly Logger _logger;

        public SeasonSearchService(ISearchForReleases releaseSearchService,
                                   IProcessDownloadDecisions processDownloadDecisions,
                                   IEpisodeService episodeService,
                                   ISeriesService seriesService,
                                   Logger logger)
        {
            _releaseSearchService = releaseSearchService;
            _processDownloadDecisions = processDownloadDecisions;
            _episodeService = episodeService;
            _seriesService = seriesService;
            _logger = logger;
        }

        public void Execute(SeasonSearchCommand message)
        {
            var series = _seriesService.GetSeries(message.SeriesId);
            var episodes = _episodeService.GetEpisodesBySeason(message.SeriesId, message.SeasonNumber);
            var userInvokedSearch = message.Trigger == CommandTrigger.Manual;
            var downloadedCount = 0;

            _logger.ProgressInfo("Searching for {0} episodes in {1}", episodes.Count, series.Title);

            var searchedCount = 0;

            foreach (var episode in episodes)
            {
                searchedCount++;
                _logger.ProgressInfo("Searching for {0} - {1} [{2}/{3}]", series.Title, episode.Title, searchedCount, episodes.Count);

                var decisions = _releaseSearchService.EpisodeSearch(episode, userInvokedSearch, false).GetAwaiter().GetResult();
                var processed = _processDownloadDecisions.ProcessDecisions(decisions).GetAwaiter().GetResult();

                downloadedCount += processed.Grabbed.Count;
            }

            _logger.ProgressInfo("Season search completed. {0} reports downloaded.", downloadedCount);
        }
    }
}
