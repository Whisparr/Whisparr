using NzbDrone.Core.Download;
using Whisparr.Http;

namespace Whisparr.Api.V3.DownloadClient
{
    [V3ApiController]
    public class DownloadClientController : ProviderControllerBase<DownloadClientResource, DownloadClientBulkResource, IDownloadClient, DownloadClientDefinition>
    {
        public static readonly DownloadClientResourceMapper ResourceMapper = new DownloadClientResourceMapper();
        public static readonly DownloadClientBulkResourceMapper BulkResourceMapper = new DownloadClientBulkResourceMapper();

        public DownloadClientController(IDownloadClientFactory downloadClientFactory)
            : base(downloadClientFactory, "downloadclient", ResourceMapper, BulkResourceMapper)
        {
        }
    }
}
