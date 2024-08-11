using System;
using System.Linq;
using NLog;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Download;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Performers;
using NzbDrone.Core.Movies.Studios;

namespace NzbDrone.Core.IndexerSearch
{
    public class PerformersSearchService : IExecute<PerformersSearchCommand>
    {
        private readonly IMovieService _movieService;
        private readonly IPerformerService _performerService;
        private readonly IStudioService _studioService;
        private readonly ISearchForReleases _releaseSearchService;
        private readonly IProcessDownloadDecisions _processDownloadDecisions;
        private readonly Logger _logger;

        public PerformersSearchService(IMovieService movieService,
                                       IPerformerService performerService,
                                       IStudioService studioService,
                                       ISearchForReleases releaseSearchService,
                                       IProcessDownloadDecisions processDownloadDecisions,
                                       Logger logger)
        {
            _movieService = movieService;
            _performerService = performerService;
            _studioService = studioService;
            _releaseSearchService = releaseSearchService;
            _processDownloadDecisions = processDownloadDecisions;
            _logger = logger;
        }

        public void Execute(PerformersSearchCommand message)
        {
            var downloadedCount = 0;
            foreach (var performerId in message.PerformerIds)
            {
                var performer = _performerService.GetById(performerId);
                var userInvokedSearch = message.Trigger == CommandTrigger.Manual;

                var items = _movieService.GetByPerformerForeignId(performer.ForeignId).Where(m => m.Monitored).ToList();

                if (message.StudioIds.Count > 0)
                {
                    var studioForeignIds = _studioService.GetStudios(message.StudioIds).Select(x => x.ForeignId);
                    items = items.Where(x => studioForeignIds.Contains(x.MovieMetadata.Value.StudioForeignId)).ToList();
                }

                foreach (var movie in items)
                {
                    var decisions = _releaseSearchService.MovieSearch(movie.Id, userInvokedSearch, false).GetAwaiter().GetResult();
                    var processDecisions = _processDownloadDecisions.ProcessDecisions(decisions).GetAwaiter().GetResult();
                    downloadedCount += processDecisions.Grabbed.Count;
                }
            }

            _logger.ProgressInfo("Performer search completed. {0} reports downloaded.", downloadedCount);
        }
    }
}
