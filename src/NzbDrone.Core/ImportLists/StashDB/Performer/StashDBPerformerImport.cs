using System;
using NLog;
using NzbDrone.Common.Cloud;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.ImportLists.StashDB.Performer
{
    public class StashDBPerformerImport : StashDBImportBase<StashDBPerformerSettings>
    {
        public StashDBPerformerImport(IWhisparrCloudRequestBuilder requestBuilder,
                                    IHttpClient httpClient,
                                    IImportListStatusService importListStatusService,
                                    IConfigService configService,
                                    IParsingService parsingService,
                                    ISearchForNewMovie skyhookProxy,
                                    Logger logger)
            : base(httpClient, importListStatusService, configService, parsingService, logger)
        {
            // _skyhookProxy = skyhookProxy;
            _requestBuilder = requestBuilder.StashDB;
        }

        private readonly IHttpRequestBuilderFactory _requestBuilder;

        public override int PageSize => 100;
        public override string Name => "StashDB Performer";
        public override bool Enabled => true;
        public override bool EnableAuto => false;
        public override ImportListType ListType => ImportListType.StashDB;
        public override TimeSpan MinRefreshInterval => TimeSpan.FromHours(1);

        public override IImportListRequestGenerator GetRequestGenerator()
        {
            return new StashDBPerformerRequestGenerator(PageSize, Settings.Limit)
            {
                RequestBuilder = _requestBuilder,
                Settings = Settings,
                Logger = _logger,
                HttpClient = _httpClient,
            };
        }
    }
}
