using System;
using System.Collections.Generic;
using System.Net;
using FluentValidation.Results;
using Newtonsoft.Json;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.Whisparr
{
    public interface IWhisparrV3Proxy
    {
        List<WhisparrMovie> GetMovies(WhisparrSettings settings);
        List<WhisparrProfile> GetProfiles(WhisparrSettings settings);
        List<WhisparrRootFolder> GetRootFolders(WhisparrSettings settings);
        List<WhisparrTag> GetTags(WhisparrSettings settings);
        ValidationFailure Test(WhisparrSettings settings);
    }

    public class WhisparrV3Proxy : IWhisparrV3Proxy
    {
        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;

        public WhisparrV3Proxy(IHttpClient httpClient, Logger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public List<WhisparrMovie> GetMovies(WhisparrSettings settings)
        {
            var requestBuilder = BuildRequest("/api/v3/movie", settings);

            requestBuilder.AddQueryParam("excludeLocalCovers", true);

            return Execute<WhisparrMovie>(requestBuilder, settings);
        }

        public List<WhisparrProfile> GetProfiles(WhisparrSettings settings)
        {
            return Execute<WhisparrProfile>(BuildRequest("/api/v3/qualityprofile", settings), settings);
        }

        public List<WhisparrRootFolder> GetRootFolders(WhisparrSettings settings)
        {
            return Execute<WhisparrRootFolder>(BuildRequest("api/v3/rootfolder", settings), settings);
        }

        public List<WhisparrTag> GetTags(WhisparrSettings settings)
        {
            return Execute<WhisparrTag>(BuildRequest("/api/v3/tag", settings), settings);
        }

        public ValidationFailure Test(WhisparrSettings settings)
        {
            try
            {
                GetMovies(settings);
            }
            catch (HttpException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.Error(ex, "API Key is invalid");
                    return new ValidationFailure("ApiKey", "API Key is invalid");
                }

                if (ex.Response.HasHttpRedirect)
                {
                    _logger.Error(ex, "Whisparr returned redirect and is invalid");
                    return new ValidationFailure("BaseUrl", "Whisparr URL is invalid, are you missing a URL base?");
                }

                _logger.Error(ex, "Unable to connect to import list.");
                return new ValidationFailure(string.Empty, $"Unable to connect to import list: {ex.Message}. Check the log surrounding this error for details.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to connect to import list.");
                return new ValidationFailure(string.Empty, $"Unable to connect to import list: {ex.Message}. Check the log surrounding this error for details.");
            }

            return null;
        }

        private HttpRequestBuilder BuildRequest(string resource, WhisparrSettings settings)
        {
            var baseUrl = settings.BaseUrl.TrimEnd('/');

            return new HttpRequestBuilder(baseUrl).Resource(resource)
                .Accept(HttpAccept.Json)
                .SetHeader("X-Api-Key", settings.ApiKey);
        }

        private List<TResource> Execute<TResource>(HttpRequestBuilder requestBuilder, WhisparrSettings settings)
        {
            if (settings.BaseUrl.IsNullOrWhiteSpace() || settings.ApiKey.IsNullOrWhiteSpace())
            {
                return new List<TResource>();
            }

            var request = requestBuilder.Build();

            var response = _httpClient.Get(request);

            if ((int)response.StatusCode >= 300)
            {
                throw new HttpException(response);
            }

            var results = JsonConvert.DeserializeObject<List<TResource>>(response.Content);

            return results;
        }
    }
}
