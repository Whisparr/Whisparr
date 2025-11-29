using NLog;
using NzbDrone.Common.Cloud;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.ImportLists.StashDB.Studio
{
    public class StashDBTagsImport : StashDBImportBase<StashDBTagsSettings>
    {
        public StashDBTagsImport(IWhisparrCloudRequestBuilder requestBuilder,
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
        public override string Name => "StashDB Tags";
        public override IImportListRequestGenerator GetRequestGenerator()
        {
            return new StashDBTagsRequestGenerator(PageSize, Settings.Limit)
            {
                RequestBuilder = _requestBuilder,
                Settings = Settings,
                Logger = _logger,
                HttpClient = _httpClient,
            };
        }
    }
}
