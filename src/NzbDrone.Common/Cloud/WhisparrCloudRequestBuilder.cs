using NzbDrone.Common.Http;

namespace NzbDrone.Common.Cloud
{
    public interface IWhisparrCloudRequestBuilder
    {
        IHttpRequestBuilderFactory Services { get; }
        IHttpRequestBuilderFactory WhisparrMetadata { get; }
        IHttpRequestBuilderFactory GithubReleases { get; }
    }

    public class WhisparrCloudRequestBuilder : IWhisparrCloudRequestBuilder
    {
        public WhisparrCloudRequestBuilder()
        {
            Services = new HttpRequestBuilder("https://whisparr.servarr.com/v1/")
                .CreateFactory();

            WhisparrMetadata = new HttpRequestBuilder("https://api.whisparr.com/v3/{route}")
                .CreateFactory();

            GithubReleases = new HttpRequestBuilder("https://api.github.com/repos/{githubownerrepo}/releases")
                .CreateFactory();
        }

        public IHttpRequestBuilderFactory Services { get; }

        public IHttpRequestBuilderFactory WhisparrMetadata { get; }

        public IHttpRequestBuilderFactory GithubReleases { get; }
    }
}
