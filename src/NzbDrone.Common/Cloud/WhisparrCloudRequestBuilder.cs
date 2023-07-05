using NzbDrone.Common.Http;

namespace NzbDrone.Common.Cloud
{
    public interface IWhisparrCloudRequestBuilder
    {
        IHttpRequestBuilderFactory Services { get; }
        IHttpRequestBuilderFactory WhisparrMetadata { get; }
    }

    public class WhisparrCloudRequestBuilder : IWhisparrCloudRequestBuilder
    {
        public WhisparrCloudRequestBuilder()
        {
            Services = new HttpRequestBuilder("https://whisparr.servarr.com/v1/")
                .CreateFactory();

            WhisparrMetadata = new HttpRequestBuilder("https://api.whisparr.com/v3/{route}")
                .CreateFactory();
        }

        public IHttpRequestBuilderFactory Services { get; }

        public IHttpRequestBuilderFactory WhisparrMetadata { get; }
    }
}
