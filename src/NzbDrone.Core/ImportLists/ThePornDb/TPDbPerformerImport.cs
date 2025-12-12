using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.ImportLists.TPDb
{
    public class TPDbPerformer : ImportListBase<TPDbPerformerSettings>
    {
        private readonly ITPDbImportProxy _customProxy;
        private readonly ISeriesService _seriesService;

        public override string Name => "TPDb Performer";

        public override TimeSpan MinRefreshInterval => TimeSpan.FromHours(6);

        public override ImportListType ListType => ImportListType.Advanced;

        public TPDbPerformer(ITPDbImportProxy customProxy,
                             IImportListStatusService importListStatusService,
                             IConfigService configService,
                             IParsingService parsingService,
                             ISeriesService seriesService,
                             Logger logger)
            : base(importListStatusService, configService, parsingService, logger)
        {
            _customProxy = customProxy;
            _seriesService = seriesService;
        }

        public override IList<ImportListItemInfo> Fetch()
        {
            var series = new List<ImportListItemInfo>();

            try
            {
                var remoteSeries = _customProxy.GetPerformer(Settings);
                var excludedIds = Settings.ExcludedSiteIds?.ToHashSet() ?? new HashSet<int>();

                foreach (var item in remoteSeries)
                {
                    // Skip excluded sites
                    if (excludedIds.Contains(item.SiteId))
                    {
                        continue;
                    }

                    series.Add(new ImportListItemInfo
                    {
                        TpdbSiteId = item.SiteId,
                        TpdbEpisodeId = item.EpisodeId
                    });
                }

                _importListStatusService.RecordSuccess(Definition.Id);
            }
            catch
            {
                _importListStatusService.RecordFailure(Definition.Id);
            }

            return CleanupListItems(series);
        }

        public override object RequestAction(string action, IDictionary<string, string> query)
        {
            if (action == "previewPerformer")
            {
                var performerId = query["performerId"];
                var tempSettings = new TPDbPerformerSettings
                {
                    PerformerId = performerId
                };

                var scenes = _customProxy.GetPerformer(tempSettings) ?? new List<PerformerScene>();

                // Debug: log first scene to see what we're getting
                if (scenes.Any())
                {
                    var first = scenes.First();
                    _logger.Debug($"First scene - SiteId: {first.SiteId}, SiteName: '{first.SiteName}', EpisodeId: {first.EpisodeId}");
                }

                // Group by SiteId and get site name + count
                var siteGroups = scenes
                    .GroupBy(s => s.SiteId)
                    .ToDictionary(
                        g => g.Key,
                        g => new { Name = g.First().SiteName, Count = g.Count() });

                var previews = new List<SitePreview>();

                foreach (var kvp in siteGroups)
                {
                    var siteId = kvp.Key;
                    var siteInfo = kvp.Value;

                    var preview = new SitePreview
                    {
                        TvdbId = siteId,
                        SceneCount = siteInfo.Count
                    };

                    // Check if series exists in local library
                    var localSeries = _seriesService.FindByTvdbId(siteId);

                    if (localSeries != null)
                    {
                        preview.Title = localSeries.Title;
                        preview.Exists = true;
                    }
                    else
                    {
                        // Use the site name from TPDb API
                        preview.Title = siteInfo.Name ?? $"Site {siteId}";
                        preview.Exists = false;
                    }

                    previews.Add(preview);
                }

                return new { sites = previews.OrderBy(p => p.Title).ToList() };
            }

            return new { };
        }

        protected override void Test(List<ValidationFailure> failures)
        {
            failures.AddIfNotNull(_customProxy.Test(Settings));
        }
    }
}
