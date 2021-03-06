using NLog;
using NzbDrone.Common.Cloud;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.ImportLists.TMDb.Company
{
    public class TMDbCompanyImport : TMDbImportListBase<TMDbCompanySettings>
    {
        public TMDbCompanyImport(IWhisparrCloudRequestBuilder requestBuilder,
                                 IHttpClient httpClient,
                                 IImportListStatusService importListStatusService,
                                 IConfigService configService,
                                 IParsingService parsingService,
                                 ISearchForNewMovie searchForNewMovie,
                                 Logger logger)
        : base(requestBuilder, httpClient, importListStatusService, configService, parsingService, searchForNewMovie, logger)
        {
        }

        public override string Name => "TMDb Company";
        public override bool Enabled => true;
        public override bool EnableAuto => false;

        public override IParseImportListResponse GetParser()
        {
            return new TMDbCompanyParser();
        }

        public override IImportListRequestGenerator GetRequestGenerator()
        {
            return new TMDbCompanyRequestGenerator()
            {
                RequestBuilder = _requestBuilder,
                Settings = Settings,
                Logger = _logger,
                HttpClient = _httpClient
            };
        }
    }
}
