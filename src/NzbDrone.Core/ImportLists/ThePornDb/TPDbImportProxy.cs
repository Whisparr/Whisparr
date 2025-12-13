using System;
using System.Collections.Generic;
using System.Net;
using FluentValidation.Results;
using Newtonsoft.Json;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Core.ImportLists.TPDb
{
    public interface ITPDbImportProxy
    {
        List<PerformerScene> GetPerformer(TPDbPerformerSettings settings);
        ValidationFailure Test(TPDbPerformerSettings settings);
    }

    public class TPDbImportProxy : ITPDbImportProxy
    {
        private readonly IHttpClient _httpClient;
        private readonly IConfigFileProvider _configFileProvider;
        private readonly Logger _logger;

        public TPDbImportProxy(IHttpClient httpClient, IConfigFileProvider configFileProvider, Logger logger)
        {
            _httpClient = httpClient;
            _configFileProvider = configFileProvider;
            _logger = logger;
        }

        public List<PerformerScene> GetPerformer(TPDbPerformerSettings settings)
        {
            return Execute<PerformerScene>(settings);
        }

        public ValidationFailure Test(TPDbPerformerSettings settings)
        {
            try
            {
                GetPerformer(settings);
            }
            catch (HttpException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.Error(ex, "There was an authorization issue. We cannot get the list from the provider.");
                    return new ValidationFailure("", "It seems we are unauthorized to make this request.");
                }

                _logger.Error(ex, "Unable to send test message");
                return new ValidationFailure("", $"We are unable to make the request to that URL. StatusCode: {ex.Response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to send test message");
                return new ValidationFailure("", "Unable to send test message");
            }

            return null;
        }

        private List<TResource> Execute<TResource>(TPDbPerformerSettings settings)
        {
            var metadataUrl = _configFileProvider.WhisparrMetadata;

            if (string.IsNullOrEmpty(metadataUrl))
            {
                metadataUrl = "https://api.whisparr.com/v3/{route}";
            }

            var url = metadataUrl.Replace("{route}", $"performer/{settings.PerformerId}/scenes");
            var request = new HttpRequestBuilder(url).Accept(HttpAccept.Json).Build();
            request.AllowAutoRedirect = true;
            var response = _httpClient.Get(request);
            var results = JsonConvert.DeserializeObject<List<TResource>>(response.Content);

            return results;
        }
    }
}
