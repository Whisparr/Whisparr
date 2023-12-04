using RestSharp;
using Whisparr.Api.V3.Indexers;

namespace NzbDrone.Integration.Test.Client
{
    public class ReleaseClient : ClientBase<ReleaseResource>
    {
        public ReleaseClient(IRestClient restClient, string apiKey)
            : base(restClient, apiKey)
        {
        }
    }
}
