using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.ImportLists.Exclusions;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.ImportLists
{
    public class ImportListSyncService : IExecute<ImportListSyncCommand>
    {
        private readonly IImportListFactory _importListFactory;
        private readonly IImportListExclusionService _importListExclusionService;
        private readonly IFetchAndParseImportList _listFetcherAndParser;
        private readonly ISearchForNewSeries _seriesSearchService;
        private readonly ISeriesService _seriesService;
        private readonly IEpisodeService _episodeService;
        private readonly IAddSeriesService _addSeriesService;
        private readonly Logger _logger;

        public ImportListSyncService(IImportListFactory importListFactory,
                              IImportListExclusionService importListExclusionService,
                              IFetchAndParseImportList listFetcherAndParser,
                              ISearchForNewSeries seriesSearchService,
                              ISeriesService seriesService,
                              IEpisodeService episodeService,
                              IAddSeriesService addSeriesService,
                              Logger logger)
        {
            _importListFactory = importListFactory;
            _importListExclusionService = importListExclusionService;
            _listFetcherAndParser = listFetcherAndParser;
            _seriesSearchService = seriesSearchService;
            _seriesService = seriesService;
            _episodeService = episodeService;
            _addSeriesService = addSeriesService;
            _logger = logger;
        }

        private void SyncAll()
        {
            if (_importListFactory.AutomaticAddEnabled().Empty())
            {
                _logger.Debug("No import lists with automatic add enabled");

                return;
            }

            _logger.ProgressInfo("Starting Import List Sync");

            var listItems = _listFetcherAndParser.Fetch().ToList();

            ProcessListItems(listItems);
        }

        private void SyncList(ImportListDefinition definition)
        {
            _logger.ProgressInfo(string.Format("Starting Import List Refresh for List {0}", definition.Name));

            var listItems = _listFetcherAndParser.FetchSingleList(definition).ToList();

            ProcessListItems(listItems);
        }

        private void ProcessListItems(List<ImportListItemInfo> items)
        {
            var seriesToAdd = new List<Series>();

            if (items.Count == 0)
            {
                _logger.ProgressInfo("No list items to process");

                return;
            }

            _logger.ProgressInfo("Processing {0} list items", items.Count);

            var reportNumber = 1;

            var listExclusions = _importListExclusionService.All();
            var importLists = _importListFactory.All();
            var existingTvdbIds = _seriesService.AllSeriesTvdbIds();

            foreach (var item in items)
            {
                _logger.ProgressTrace("Processing list item {0}/{1}", reportNumber, items.Count);

                reportNumber++;

                var importList = importLists.Single(x => x.Id == item.ImportListId);

                // Map TVDb if we only have a series name
                if (item.TpdbSiteId <= 0 && item.Title.IsNotNullOrWhiteSpace())
                {
                    var mappedSeries = _seriesSearchService.SearchForNewSeries(item.Title)
                        .FirstOrDefault();

                    if (mappedSeries != null)
                    {
                        item.TpdbSiteId = mappedSeries.TvdbId;
                        item.Title = mappedSeries?.Title;
                    }
                }

                // Check to see if series excluded
                var excludedSeries = listExclusions.Where(s => s.TvdbId == item.TpdbSiteId).SingleOrDefault();

                if (excludedSeries != null)
                {
                    _logger.Debug("{0} [{1}] Rejected due to list exclusion", item.TpdbSiteId, item.Title);
                    continue;
                }

                // Break if Series Exists in DB
                if (existingTvdbIds.Any(x => x == item.TpdbSiteId))
                {
                    _logger.Debug("{0} [{1}] Rejected, Series Exists in DB", item.TpdbSiteId, item.Title);

                    // Set montiored if episode item
                    if (importList.ShouldMonitor == ImportListMonitorTypes.SpecificEpisode && item.TpdbEpisodeId > 0)
                    {
                        var series = _seriesService.FindByTvdbId(item.TpdbSiteId);

                        if (series != null)
                        {
                            var seriesEpisodes = _episodeService.GetEpisodeBySeries(series.Id);

                            var episode = seriesEpisodes.FirstOrDefault(x => x.TvdbId == item.TpdbEpisodeId);

                            if (episode != null && !episode.Monitored)
                            {
                                _episodeService.SetEpisodeMonitored(episode.Id, true);
                            }
                        }
                    }

                    continue;
                }

                // Append Series if not already in DB or already on add list
                if (seriesToAdd.All(s => s.TvdbId != item.TpdbSiteId))
                {
                    var monitored = importList.ShouldMonitor != ImportListMonitorTypes.None;

                    var toAdd = new Series
                    {
                        TvdbId = item.TpdbSiteId,
                        Title = item.Title,
                        Year = item.Year,
                        Monitored = monitored,
                        RootFolderPath = importList.RootFolderPath,
                        QualityProfileId = importList.QualityProfileId,
                        Tags = importList.Tags,
                        AddOptions = new AddSeriesOptions
                                     {
                                         SearchForMissingEpisodes = importList.SearchForMissingEpisodes,
                                         Monitor = importList.ShouldMonitor == ImportListMonitorTypes.EntireSite ? importList.SiteMonitorType : MonitorTypes.Existing
                                     }
                    };

                    seriesToAdd.Add(toAdd);
                }

                if (importList.ShouldMonitor == ImportListMonitorTypes.SpecificEpisode && item.TpdbEpisodeId > 0)
                {
                    var index = seriesToAdd.FindIndex(c => c.TvdbId == item.TpdbSiteId);

                    if (index >= 0 && seriesToAdd[index].AddOptions != null)
                    {
                        seriesToAdd[index].AddOptions.EpisodesToMonitor.Add(item.TpdbEpisodeId);
                    }
                }
            }

            _addSeriesService.AddSeries(seriesToAdd, true);

            var message = string.Format("Import List Sync Completed. Items found: {0}, Series added: {1}", items.Count, seriesToAdd.Count);

            _logger.ProgressInfo(message);
        }

        public void Execute(ImportListSyncCommand message)
        {
            if (message.DefinitionId.HasValue)
            {
                SyncList(_importListFactory.Get(message.DefinitionId.Value));
            }
            else
            {
                SyncAll();
            }
        }
    }
}
