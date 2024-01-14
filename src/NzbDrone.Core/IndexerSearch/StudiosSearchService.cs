using System.Linq;
using NLog;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Download;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Studios;

namespace NzbDrone.Core.IndexerSearch
{
    public class StudiosSearchService : IExecute<StudiosSearchCommand>
    {
        private readonly IMovieService _movieService;
        private readonly IStudioService _studioService;
        private readonly ISearchForReleases _releaseSearchService;
        private readonly IProcessDownloadDecisions _processDownloadDecisions;
        private readonly Logger _logger;

        public StudiosSearchService(IMovieService movieService,
                                    IStudioService studioService,
                                    ISearchForReleases releaseSearchService,
                                    IProcessDownloadDecisions processDownloadDecisions,
                                    Logger logger)
        {
            _movieService = movieService;
            _studioService = studioService;
            _releaseSearchService = releaseSearchService;
            _processDownloadDecisions = processDownloadDecisions;
            _logger = logger;
        }

        public void Execute(StudiosSearchCommand message)
        {
            var downloadedCount = 0;
            foreach (var studioId in message.StudioIds)
            {
                var studio = _studioService.GetById(studioId);
                var userInvokedSearch = message.Trigger == CommandTrigger.Manual;

                var items = _movieService.GetByStudioForeignId(studio.ForeignId);

                if (message.Years.Count > 0)
                {
                    items = items.Where(x => message.Years.Contains(x.Year)).ToList();
                }

                foreach (var movie in items)
                {
                    var decisions = _releaseSearchService.MovieSearch(movie.Id, userInvokedSearch, false).GetAwaiter().GetResult();
                    var processDecisions = _processDownloadDecisions.ProcessDecisions(decisions).GetAwaiter().GetResult();
                    downloadedCount += processDecisions.Grabbed.Count;
                }
            }

            _logger.ProgressInfo("Studio search completed. {0} reports downloaded.", downloadedCount);
        }
    }
}
