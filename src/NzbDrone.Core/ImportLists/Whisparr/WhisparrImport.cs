using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.Whisparr
{
    public class WhisparrImport : ImportListBase<WhisparrSettings>
    {
        private readonly IWhisparrV3Proxy _whisparrV3Proxy;
        public override string Name => "Whisparr";

        public override ImportListType ListType => ImportListType.Program;
        public override TimeSpan MinRefreshInterval => TimeSpan.FromMinutes(5);

        public WhisparrImport(IWhisparrV3Proxy whisparrV3Proxy,
                            IImportListStatusService importListStatusService,
                            IConfigService configService,
                            IParsingService parsingService,
                            Logger logger)
            : base(importListStatusService, configService, parsingService, logger)
        {
            _whisparrV3Proxy = whisparrV3Proxy;
        }

        public override IList<ImportListItemInfo> Fetch()
        {
            var series = new List<ImportListItemInfo>();

            try
            {
                var remoteSeries = _whisparrV3Proxy.GetSeries(Settings);

                foreach (var item in remoteSeries)
                {
                    if (Settings.ProfileIds.Any() && !Settings.ProfileIds.Contains(item.QualityProfileId))
                    {
                        series.Add(new ImportListItemInfo
                        {
                            TpdbSiteId = item.TvdbId,
                            Title = item.Title
                        });
                    }

                    if (Settings.TagIds.Any() && !Settings.TagIds.Any(tagId => item.Tags.Any(itemTagId => itemTagId == tagId)))
                    {
                        continue;
                    }

                    if (Settings.RootFolderPaths.Any() && !Settings.RootFolderPaths.Any(rootFolderPath => item.RootFolderPath.ContainsIgnoreCase(rootFolderPath)))
                    {
                        continue;
                    }

                    series.Add(new ImportListItemInfo
                    {
                        TpdbSiteId = item.TvdbId,
                        Title = item.Title
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
            // Return early if there is not an API key
            if (Settings.ApiKey.IsNullOrWhiteSpace())
            {
                return new
                {
                    devices = new List<object>()
                };
            }

            Settings.Validate().Filter("ApiKey").ThrowOnError();

            if (action == "getProfiles")
            {
                var profiles = _whisparrV3Proxy.GetQualityProfiles(Settings);

                return new
                {
                    options = profiles.OrderBy(d => d.Name, StringComparer.InvariantCultureIgnoreCase)
                        .Select(d => new
                        {
                            value = d.Id,
                            name = d.Name
                        })
                };
            }

            if (action == "getTags")
            {
                var tags = _whisparrV3Proxy.GetTags(Settings);

                return new
                {
                    options = tags.OrderBy(d => d.Label, StringComparer.InvariantCultureIgnoreCase)
                        .Select(d => new
                        {
                            value = d.Id,
                            name = d.Label
                        })
                };
            }

            if (action == "getRootFolders")
            {
                var remoteRootFolders = _whisparrV3Proxy.GetRootFolders(Settings);

                return new
                {
                    options = remoteRootFolders.OrderBy(d => d.Path, StringComparer.InvariantCultureIgnoreCase)
                        .Select(d => new
                        {
                            value = d.Path,
                            name = d.Path
                        })
                };
            }

            return new { };
        }

        protected override void Test(List<ValidationFailure> failures)
        {
            failures.AddIfNotNull(_whisparrV3Proxy.Test(Settings));
        }
    }
}
