using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.IndexerSearch
{
    public interface ISearchForReleases
    {
        Task<List<DownloadDecision>> EpisodeSearch(int episodeId, bool userInvokedSearch, bool interactiveSearch);
        Task<List<DownloadDecision>> EpisodeSearch(Episode episode, bool userInvokedSearch, bool interactiveSearch);
        Task<List<DownloadDecision>> SeasonSearch(int seriesId, int seasonNumber, bool missingOnly, bool monitoredOnly, bool userInvokedSearch, bool interactiveSearch);
        Task<List<DownloadDecision>> SeasonSearch(int seriesId, int seasonNumber, List<Episode> episodes, bool monitoredOnly, bool userInvokedSearch, bool interactiveSearch);
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

        public async Task<List<DownloadDecision>> EpisodeSearch(int episodeId, bool userInvokedSearch, bool interactiveSearch)
        {
            var episode = _episodeService.GetEpisode(episodeId);

            return await EpisodeSearch(episode, userInvokedSearch, interactiveSearch);
        }

        public async Task<List<DownloadDecision>> EpisodeSearch(Episode episode, bool userInvokedSearch, bool interactiveSearch)
        {
            var series = _seriesService.GetSeries(episode.SeriesId);

            return await SearchSingle(series, episode, false, userInvokedSearch, interactiveSearch);
        }

        public async Task<List<DownloadDecision>> SeasonSearch(int seriesId, int seasonNumber, bool missingOnly, bool monitoredOnly, bool userInvokedSearch, bool interactiveSearch)
        {
            var episodes = _episodeService.GetEpisodesBySeason(seriesId, seasonNumber);

            if (missingOnly)
            {
                episodes = episodes.Where(e => !e.HasFile).ToList();
            }

            return await SeasonSearch(seriesId, seasonNumber, episodes, monitoredOnly, userInvokedSearch, interactiveSearch);
        }

        public async Task<List<DownloadDecision>> SeasonSearch(int seriesId, int seasonNumber, List<Episode> episodes, bool monitoredOnly, bool userInvokedSearch, bool interactiveSearch)
        {
            var series = _seriesService.GetSeries(seriesId);

            var downloadDecisions = new List<DownloadDecision>();

            if (episodes.Count == 1)
            {
                var searchSpec = Get<SingleEpisodeSearchCriteria>(series, episodes, monitoredOnly, userInvokedSearch, interactiveSearch);

                var decisions = await Dispatch(indexer => indexer.Fetch(searchSpec), searchSpec);
                downloadDecisions.AddRange(decisions);
            }
            else
            {
                var searchSpec = Get<SeasonSearchCriteria>(series, episodes, monitoredOnly, userInvokedSearch, interactiveSearch);
                searchSpec.Year = seasonNumber;

                var decisions = await Dispatch(indexer => indexer.Fetch(searchSpec), searchSpec);
                downloadDecisions.AddRange(decisions);
            }

            return DeDupeDecisions(downloadDecisions);
        }

        private async Task<List<DownloadDecision>> SearchSingle(Series series, Episode episode, bool monitoredOnly, bool userInvokedSearch, bool interactiveSearch)
        {
            var downloadDecisions = new List<DownloadDecision>();

            var searchSpec = Get<SingleEpisodeSearchCriteria>(series, new List<Episode> { episode }, monitoredOnly, userInvokedSearch, interactiveSearch);
            searchSpec.ReleaseDate = DateOnly.Parse(episode.AirDate);
            searchSpec.Performer = episode.Actors.Select(p => p.Name).FirstOrDefault();
            searchSpec.EpisodeTitle = episode.Title;

            var decisions = await Dispatch(indexer => indexer.Fetch(searchSpec), searchSpec);
            downloadDecisions.AddRange(decisions);

            return DeDupeDecisions(downloadDecisions);
        }

        private TSpec Get<TSpec>(Series series, List<Episode> episodes, bool monitoredOnly, bool userInvokedSearch, bool interactiveSearch)
            where TSpec : SearchCriteriaBase, new()
        {
            var spec = new TSpec();
            spec.SceneTitles = episodes.Select(e => e.Title).ToList();
            spec.SceneTitles.Add(series.TitleSlug);

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

        private async Task<List<DownloadDecision>> Dispatch(Func<IIndexer, Task<IList<ReleaseInfo>>> searchAction, SearchCriteriaBase criteriaBase)
        {
            var indexers = criteriaBase.InteractiveSearch ?
                _indexerFactory.InteractiveSearchEnabled() :
                _indexerFactory.AutomaticSearchEnabled();

            // Filter indexers to untagged indexers and indexers with intersecting tags
            indexers = indexers.Where(i => i.Definition.Tags.Empty() || i.Definition.Tags.Intersect(criteriaBase.Series.Tags).Any()).ToList();

            _logger.ProgressInfo("Searching indexers for {0}. {1} active indexers", criteriaBase, indexers.Count);

            var tasks = indexers.Select(indexer => DispatchIndexer(searchAction, indexer, criteriaBase));

            var batch = await Task.WhenAll(tasks);

            var reports = batch.SelectMany(x => x).ToList();

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

        private async Task<IList<ReleaseInfo>> DispatchIndexer(Func<IIndexer, Task<IList<ReleaseInfo>>> searchAction, IIndexer indexer, SearchCriteriaBase criteriaBase)
        {
            try
            {
                return await searchAction(indexer);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error while searching for {0}", criteriaBase);
            }

            return Array.Empty<ReleaseInfo>();
        }

        private List<DownloadDecision> DeDupeDecisions(List<DownloadDecision> decisions)
        {
            // De-dupe reports by guid so duplicate results aren't returned. Pick the one with the least rejections and higher indexer priority.
            return decisions.GroupBy(d => d.RemoteEpisode.Release.Guid)
                .Select(d => d.OrderBy(v => v.Rejections.Count()).ThenBy(v => v.RemoteEpisode?.Release?.IndexerPriority ?? IndexerDefinition.DefaultPriority).First())
                .ToList();
        }
    }
}
