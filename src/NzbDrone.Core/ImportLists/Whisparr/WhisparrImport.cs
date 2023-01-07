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
                    if ((!Settings.ProfileIds.Any() || Settings.ProfileIds.Contains(item.QualityProfileId)) &&
                        (!Settings.LanguageProfileIds.Any() || Settings.LanguageProfileIds.Contains(item.LanguageProfileId)) &&
                        (!Settings.TagIds.Any() || Settings.TagIds.Any(tagId => item.Tags.Any(itemTagId => itemTagId == tagId))))
                    {
                        series.Add(new ImportListItemInfo
                        {
                            TvdbId = item.TvdbId,
                            Title = item.Title
                        });
                    }
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

            if (action == "getProfiles")
            {
                Settings.Validate().Filter("ApiKey").ThrowOnError();

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

            if (action == "getLanguageProfiles")
            {
                Settings.Validate().Filter("ApiKey").ThrowOnError();

                var langProfiles = _whisparrV3Proxy.GetLanguageProfiles(Settings);

                return new
                {
                    options = langProfiles.OrderBy(d => d.Name, StringComparer.InvariantCultureIgnoreCase)
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

            return new { };
        }

        protected override void Test(List<ValidationFailure> failures)
        {
            failures.AddIfNotNull(_whisparrV3Proxy.Test(Settings));
        }
    }
}
