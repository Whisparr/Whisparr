using System;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.ImportLists.StashDB
{
    public abstract class StashDBImportBase<TSettings> : HttpImportListBase<TSettings>
    where TSettings : StashDBSettingsBase<TSettings>, new()
    {
        public StashDBImportBase(IHttpClient httpClient,
                                    IImportListStatusService importListStatusService,
                                    IConfigService configService,
                                    IParsingService parsingService,
                                    Logger logger)
            : base(httpClient, importListStatusService, configService, parsingService, logger)
        {
        }

        public override int PageSize => 100;
        public override bool Enabled => true;
        public override bool EnableAuto => false;
        public override ImportListType ListType => ImportListType.StashDB;
        public override TimeSpan MinRefreshInterval => TimeSpan.FromHours(12);

        public override IParseImportListResponse GetParser()
        {
            return new StashDBParser();
        }
    }
}
