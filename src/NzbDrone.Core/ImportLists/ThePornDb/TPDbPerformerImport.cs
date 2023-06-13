using System;
using System.Collections.Generic;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.ImportLists.TPDb
{
    public class TPDbPerformer : ImportListBase<TPDbPerformerSettings>
    {
        private readonly ITPDbImportProxy _customProxy;
        public override string Name => "TPDb Performer";

        public override TimeSpan MinRefreshInterval => TimeSpan.FromHours(6);

        public override ImportListType ListType => ImportListType.Advanced;

        public TPDbPerformer(ITPDbImportProxy customProxy,
                             IImportListStatusService importListStatusService,
                             IConfigService configService,
                             IParsingService parsingService,
                             Logger logger)
            : base(importListStatusService, configService, parsingService, logger)
        {
            _customProxy = customProxy;
        }

        public override IList<ImportListItemInfo> Fetch()
        {
            var series = new List<ImportListItemInfo>();

            try
            {
                var remoteSeries = _customProxy.GetPerformer(Settings);

                foreach (var item in remoteSeries)
                {
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
            return new { };
        }

        protected override void Test(List<ValidationFailure> failures)
        {
            failures.AddIfNotNull(_customProxy.Test(Settings));
        }
    }
}
