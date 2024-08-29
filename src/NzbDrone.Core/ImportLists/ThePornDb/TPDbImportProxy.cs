using System;
using System.Collections.Generic;
using System.Net;
using FluentValidation.Results;
using Newtonsoft.Json;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.ImportLists.ThePornDb;
using static NzbDrone.Core.ImportLists.ThePornDb.TPDbSceneSettings;

namespace NzbDrone.Core.ImportLists.TPDb
{
    public interface ITPDbImportProxy
    {
        List<PerformerScene> GetPerformer(TPDbPerformerSettings settings);
        List<PerformerScene> GetScenes(TPDbSceneSettings settings);
        ValidationFailure Test(TPDbPerformerSettings settings);
        ValidationFailure Test(TPDbSceneSettings settings);
    }

    public class TPDbImportProxy : ITPDbImportProxy
    {
        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;

        public TPDbImportProxy(IHttpClient httpClient, Logger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public List<PerformerScene> GetScenes(TPDbSceneSettings settings)
        {
            return Execute<TPDbApiObject>(settings).ToPerformerSceneList();
        }

        private void GetTestScenes(TPDbSceneSettings settings)
        {
            Check<TPDbApiObject>(settings);
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
                    return new ValidationFailure("BaseUrl", "It seems we are unauthorized to make this request.");
                }

                _logger.Error(ex, "Unable to send test message");
                return new ValidationFailure("BaseUrl", $"We are unable to make the request to that URL. StatusCode: {ex.Response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to send test message");
                return new ValidationFailure("", "Unable to send test message");
            }

            return null;
        }

        public ValidationFailure Test(TPDbSceneSettings settings)
        {
            try
            {
                GetTestScenes(settings);
            }
            catch (HttpException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.Error(ex, "There was an authorization issue. We cannot get the list from the provider.");
                    return new ValidationFailure("BaseUrl", "It seems we are unauthorized to make this request.");
                }

                _logger.Error(ex, "Unable to send test message");
                return new ValidationFailure("BaseUrl", $"We are unable to make the request to that URL. StatusCode: {ex.Response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to send test message");
                return new ValidationFailure("", "Unable to send test message");
            }

            return null;
        }

        private TPDbApiObject Execute<TResource>(TPDbSceneSettings settings)
            where TResource : TPDbApiObject, new()
        {
            var tPDbApiObject = new TResource();
            var currentPage = 1;
            int lastPage;
            do
            {
                var request = BuildRequest(settings, currentPage);
                var response = _httpClient.Get(request);
                var json = JsonConvert.DeserializeObject<TResource>(response.Content);
                tPDbApiObject.CombineWith(json);

                lastPage = tPDbApiObject.Meta.LastPage;
                currentPage = tPDbApiObject.Meta.CurrentPage + 1;
            }
            while (currentPage <= lastPage);

            return tPDbApiObject;
        }

        private void Check<TResource>(TPDbSceneSettings settings)
        {
            var request = new HttpRequestBuilder(settings.BaseUrl)
                .Resource("auth/user")
                .SetHeader("Authorization", $"Bearer {settings.ApiKey}")
                .Accept(HttpAccept.Json)
                .Build();
            _httpClient.Get(request);
        }

        private List<TResource> Execute<TResource>(TPDbPerformerSettings settings)
        {
            if (settings.BaseUrl.IsNullOrWhiteSpace())
            {
                return new List<TResource>();
            }

            var baseUrl = settings.BaseUrl.TrimEnd('/');
            var request = new HttpRequestBuilder(baseUrl).Resource($"performer/{settings.PerformerId}/scenes").Accept(HttpAccept.Json).Build();
            var response = _httpClient.Get(request);
            var results = JsonConvert.DeserializeObject<List<TResource>>(response.Content);

            return results;
        }

        private HttpRequest BuildRequest(TPDbSceneSettings settings, int page)
        {
            var baseUrl = settings.BaseUrl.TrimEnd('/');

            var httpRequestBuilder = new HttpRequestBuilder(baseUrl)
                .Resource("scenes")
                .SetHeader("Authorization", $"Bearer {settings.ApiKey}")
                .Accept(HttpAccept.Json)
                .AddQueryParam("page", page)
                .AddQueryParam("orderBy", settings.OrderBy)
                .AddQueryParam("per_page", settings.PerPageLimit);

            if (!string.IsNullOrEmpty(settings.Date) && !string.IsNullOrEmpty(settings.DateContext))
            {
                httpRequestBuilder
                    .AddQueryParam("date", settings.Date)
                    .AddQueryParam("date_operation", settings.DateContext);
            }

            if (settings.Collected != TriPosition.None)
            {
                httpRequestBuilder
                    .AddQueryParam("is_collected", (int)settings.Collected);
            }

            if (settings.Favourites != TriPosition.None)
            {
                httpRequestBuilder
                    .AddQueryParam("is_favourite", (int)settings.Favourites);
            }

            return httpRequestBuilder.Build();
        }
    }
}
