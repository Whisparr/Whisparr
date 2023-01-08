using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Common.TPL;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.IndexerSearch
{
    public interface ISearchForReleases
    {
        List<DownloadDecision> EpisodeSearch(int episodeId, bool userInvokedSearch, bool interactiveSearch);
        List<DownloadDecision> EpisodeSearch(Episode episode, bool userInvokedSearch, bool interactiveSearch);
        List<DownloadDecision> SeasonSearch(int seriesId, int seasonNumber, bool missingOnly, bool monitoredOnly, bool userInvokedSearch, bool interactiveSearch);
        List<DownloadDecision> SeasonSearch(int seriesId, int seasonNumber, List<Episode> episodes, bool monitoredOnly, bool userInvokedSearch, bool interactiveSearch);
    }

    public class ReleaseSearchService : ISearchForReleases
    {
        private readonly IIndexerFactory _indexerFactory;
        private readonly ISeriesService _seriesService;
        private readonly IEpisodeService _episodeService;
        private readonly IMakeDownloadDecision _makeDownloadDecision;
        private readonly Logger _logger;

        public ReleaseSearchService(IIndexerFactory indexerFactory,
                                ISeriesService seriesService,
                                IEpisodeService episodeService,
                                IMakeDownloadDecision makeDownloadDecision,
                                Logger logger)
        {
            _indexerFactory = indexerFactory;
            _seriesService = seriesService;
            _episodeService = episodeService;
            _makeDownloadDecision = makeDownloadDecision;
            _logger = logger;
        }

        public List<DownloadDecision> EpisodeSearch(int episodeId, bool userInvokedSearch, bool interactiveSearch)
        {
            var episode = _episodeService.GetEpisode(episodeId);

            return EpisodeSearch(episode, userInvokedSearch, interactiveSearch);
        }

        public List<DownloadDecision> EpisodeSearch(Episode episode, bool userInvokedSearch, bool interactiveSearch)
        {
            var series = _seriesService.GetSeries(episode.SeriesId);

            return SearchSpecial(series, new List<Episode> { episode }, false, userInvokedSearch, interactiveSearch);
        }

        public List<DownloadDecision> SeasonSearch(int seriesId, int seasonNumber, bool missingOnly, bool monitoredOnly, bool userInvokedSearch, bool interactiveSearch)
        {
            var episodes = _episodeService.GetEpisodesBySeason(seriesId, seasonNumber);

            if (missingOnly)
            {
                episodes = episodes.Where(e => !e.HasFile).ToList();
            }

            return SeasonSearch(seriesId, seasonNumber, episodes, monitoredOnly, userInvokedSearch, interactiveSearch);
        }

        public List<DownloadDecision> SeasonSearch(int seriesId, int seasonNumber, List<Episode> episodes, bool monitoredOnly, bool userInvokedSearch, bool interactiveSearch)
        {
            var series = _seriesService.GetSeries(seriesId);

            var downloadDecisions = new List<DownloadDecision>();

            downloadDecisions.AddRange(SearchSpecial(series, episodes, monitoredOnly, userInvokedSearch, interactiveSearch));

            return DeDupeDecisions(downloadDecisions);
        }

        private List<DownloadDecision> SearchSpecial(Series series, List<Episode> episodes, bool monitoredOnly, bool userInvokedSearch, bool interactiveSearch)
        {
            var downloadDecisions = new List<DownloadDecision>();

            var searchSpec = Get<SpecialEpisodeSearchCriteria>(series, episodes, monitoredOnly, userInvokedSearch, interactiveSearch);

            // build list of queries for each episode in the form: "<series> <episode-title>"
            searchSpec.EpisodeQueryTitles = episodes.Where(e => !string.IsNullOrWhiteSpace(e.Title))
                                                    .SelectMany(e => searchSpec.CleanSceneTitles.Select(title => title + " " + SearchCriteriaBase.GetCleanSceneTitle(e.Title)))
                                                    .ToArray();

            downloadDecisions.AddRange(Dispatch(indexer => indexer.Fetch(searchSpec), searchSpec));

            return DeDupeDecisions(downloadDecisions);
        }

        private TSpec Get<TSpec>(Series series, List<Episode> episodes, bool monitoredOnly, bool userInvokedSearch, bool interactiveSearch)
            where TSpec : SearchCriteriaBase, new()
        {
            var spec = new TSpec();
            spec.SceneTitles = new List<string> { series.Title };

            spec.Series = series;
            spec.Episodes = episodes;
            spec.MonitoredEpisodesOnly = monitoredOnly;
            spec.UserInvokedSearch = userInvokedSearch;
            spec.InteractiveSearch = interactiveSearch;

            if (!spec.SceneTitles.Contains(series.Title))
            {
                spec.SceneTitles.Add(series.Title);
            }

            return spec;
        }

        private List<DownloadDecision> Dispatch(Func<IIndexer, IEnumerable<ReleaseInfo>> searchAction, SearchCriteriaBase criteriaBase)
        {
            var indexers = criteriaBase.InteractiveSearch ?
                _indexerFactory.InteractiveSearchEnabled() :
                _indexerFactory.AutomaticSearchEnabled();

            // Filter indexers to untagged indexers and indexers with intersecting tags
            indexers = indexers.Where(i => i.Definition.Tags.Empty() || i.Definition.Tags.Intersect(criteriaBase.Series.Tags).Any()).ToList();

            var reports = new List<ReleaseInfo>();

            _logger.ProgressInfo("Searching indexers for {0}. {1} active indexers", criteriaBase, indexers.Count);

            var taskList = new List<Task>();
            var taskFactory = new TaskFactory(TaskCreationOptions.LongRunning, TaskContinuationOptions.None);

            foreach (var indexer in indexers)
            {
                var indexerLocal = indexer;

                taskList.Add(taskFactory.StartNew(() =>
                {
                    try
                    {
                        var indexerReports = searchAction(indexerLocal);

                        lock (reports)
                        {
                            reports.AddRange(indexerReports);
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "Error while searching for {0}", criteriaBase);
                    }
                }).LogExceptions());
            }

            Task.WaitAll(taskList.ToArray());

            _logger.Debug("Total of {0} reports were found for {1} from {2} indexers", reports.Count, criteriaBase, indexers.Count);

            // Update the last search time for all episodes if at least 1 indexer was searched.
            if (indexers.Any())
            {
                var lastSearchTime = DateTime.UtcNow;
                _logger.Debug("Setting last search time to: {0}", lastSearchTime);

                criteriaBase.Episodes.ForEach(e => e.LastSearchTime = lastSearchTime);
                _episodeService.UpdateLastSearchTime(criteriaBase.Episodes);
            }

            return _makeDownloadDecision.GetSearchDecision(reports, criteriaBase).ToList();
        }

        private List<DownloadDecision> DeDupeDecisions(List<DownloadDecision> decisions)
        {
            // De-dupe reports by guid so duplicate results aren't returned. Pick the one with the least rejections.

            return decisions.GroupBy(d => d.RemoteEpisode.Release.Guid).Select(d => d.OrderBy(v => v.Rejections.Count()).First()).ToList();
        }
    }
}
