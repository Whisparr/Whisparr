using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.ImportLists.TPDb;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.ImportLists.ThePornDb
{
    public class TPDbSceneImport : ImportListBase<TPDbSceneSettings>
    {
        private readonly ITPDbImportProxy _customProxy;

        public override string Name => "TPDb Scenes";

        public override TimeSpan MinRefreshInterval => TimeSpan.FromHours(6);

        public override ImportListType ListType => ImportListType.Advanced;

        public TPDbSceneImport(
            ITPDbImportProxy customProxy,
            IImportListStatusService importListStatusService,
            IConfigService configService,
            IParsingService parsingService,
            Logger logger)
            : base(importListStatusService, configService, parsingService, logger)
        {
            _customProxy = customProxy;
        }

        public override object RequestAction(string stage, IDictionary<string, string> query)
        {
            if (stage == "getOrderBy")
            {
                var choices = new Dictionary<string, string>()
                {
                    { "recently_created", "Recently Created" },
                    { "former_created", "Oldest Added" },
                    { "recently_updated", "Recently Updated" },
                    { "former_updated", "Former Updated" },
                    { "recently_released", "Recently Released" },
                    { "former_released", "Former Released" },
                };
                return new
                {
                    options = choices.Select(i => new
                    {
                        value = i.Key,
                        name = i.Value
                    })
                };
            }

            if (stage == "getDateContext")
            {
                var choices = new Dictionary<string, string>()
                {
                    { "=", "Equal" },
                    { ">", "Greater" },
                    { ">=", "Greater or Equal" },
                    { "<", "Less" },
                    { "<=", "Less or Equal" }
                };
                return new
                {
                    options = choices.Select(i => new
                    {
                        value = i.Key,
                        Name = i.Value
                    })
                };
            }

            return new { };
        }

        // remove if implementation is going to get pushed down to the concrete classes:
        // TPDbCollectionImport/TPDbFavouritesImport
        public override IList<ImportListItemInfo> Fetch()
        {
            var series = new List<ImportListItemInfo>();

            try
            {
                var remoteSeries = _customProxy.GetScenes(Settings);

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

        protected override void Test(List<ValidationFailure> failures)
        {
            failures.AddIfNotNull(_customProxy.Test(Settings));
        }
    }
}
