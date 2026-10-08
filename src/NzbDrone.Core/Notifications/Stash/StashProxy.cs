using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;

namespace NzbDrone.Core.Notifications.Stash
{
    public interface IStashProxy
    {
        void Clean(StashSettings settings, string path);
        void Update(StashSettings settings, string path);
        void GetStatus(StashSettings settings);
        List<StashPerformer> GetPerformers(StashSettings settings);
        StashPerformer UpdatePerformerFavorite(StashSettings settings, string performerId, bool favorite);
        StashPerformer CreatePerformer(StashSettings settings, string name, string stashDbId);
    }

    public class StashProxy : IStashProxy
    {
        public const string StashDbEndpoint = "https://stashdb.org/graphql";

        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;

        public StashProxy(IHttpClient httpClient, Logger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public void Clean(StashSettings settings, string path)
        {
            var request = BuildRequest(settings);
            request.Headers.ContentType = "application/json";

            var cleanPath = path.ToJson();

            request.SetContent(new
            {
                Query = $@"mutation {{
                        metadataClean(
                            input: {{
                                dryRun: false,
                                paths: [{cleanPath}]
                            }})
                        }}"
            }.ToJson());

            ProcessRequest(request, settings);
        }

        public void Update(StashSettings settings, string path)
        {
            var request = BuildRequest(settings);
            request.Headers.ContentType = "application/json";

            var cleanPath = path.ToJson();

            var source = "";
            if (settings.StashBoxEndpoint.IsNotNullOrWhiteSpace())
            {
                source += $@"{{source: {{stash_box_endpoint:""{settings.StashBoxEndpoint}""}} }},";
            }

            if (settings.BuiltinAutotag)
            {
                source += $@"{{source: {{scraper_id: ""builtin_autotag""}}, options: {{setOrganized: false}} }},";
            }

            var metadataIdentifyQuery =
                settings.MetadataIdentify ?
                $@"metadataIdentify(
                    input: {{
                        sources: [
                            {source}
                        ],
                        options: {{
                            includeMalePerformers: {(settings.IncludeMalePerformers ? "true" : "false")},
                            setCoverImage: {(settings.SetCoverImage ? "true" : "false")},
                            setOrganized: {(settings.SetOrganized ? "true" : "false")},
                            skipMultipleMatches: {(settings.SkipMultipleMatches ? "true" : "false")},
                            skipMultipleMatchTag: ""{settings.SkipMultipleMatchTag}"",
                            fieldOptions: [
                                {{ field: ""title"", strategy: MERGE, createMissing: null }},
                                {{ field: ""studio"", strategy: MERGE, createMissing: true }},
                                {{ field: ""performers"", strategy: MERGE, createMissing: true }},
                                {{ field: ""tags"", strategy: MERGE, createMissing: true }},
                                {{ field: ""date"", strategy: MERGE, createMissing: false }},
                                {{ field: ""stash_ids"", strategy: MERGE, createMissing: false }}
                            ]
                        }}, 
                        paths: [{cleanPath}]
                    }})" : "";

            request.SetContent(new
            {
                Query = $@"mutation {{
                            metadataScan(
                            input: {{
                                scanGenerateCovers: {(settings.GenerateCovers ? "true" : "false")},
                                scanGeneratePreviews: {(settings.GeneratePreviews ? "true" : "false")},
                                scanGenerateImagePreviews: {(settings.GenerateImagePreviews ? "true" : "false")},
                                scanGenerateSprites: {(settings.GenerateSprites ? "true" : "false")},
                                scanGeneratePhashes: {(settings.GeneratePhashes ? "true" : "false")},
                                paths: [{cleanPath}]
                            }})
                            {metadataIdentifyQuery}
                        }}"
            }.ToJson());

            ProcessRequest(request, settings);
        }

        public void GetStatus(StashSettings settings)
        {
            var request = BuildRequest(settings);
            request.Headers.ContentType = "application/json";

            request.SetContent(new
            {
                Query = "{ systemStatus { databaseSchema databasePath configPath appSchema status } }"
            }.ToJson());

            ProcessRequest(request, settings);
        }

        public List<StashPerformer> GetPerformers(StashSettings settings)
        {
            const string query = @"query($page:Int!){
                findPerformers(filter:{page:$page,per_page:500,sort:""id"",direction:ASC}){
                    count
                    performers{id name favorite stash_ids{endpoint stash_id}}
                }
            }";

            var performers = new List<StashPerformer>();
            var page = 1;

            while (true)
            {
                var data = ExecuteGraphQl<StashFindPerformersData>(settings, query, new { page });
                var result = data?.FindPerformers;

                if (result?.Performers == null)
                {
                    throw new InvalidOperationException("Stash returned an incomplete performer listing");
                }

                performers.AddRange(result.Performers);

                if (performers.Count >= result.Count)
                {
                    return performers;
                }

                if (result.Performers.Count == 0)
                {
                    throw new InvalidOperationException("Stash performer pagination ended before the reported count");
                }

                page++;
            }
        }

        public StashPerformer UpdatePerformerFavorite(StashSettings settings, string performerId, bool favorite)
        {
            const string mutation = @"mutation($input:PerformerUpdateInput!){
                performerUpdate(input:$input){id name favorite stash_ids{endpoint stash_id}}
            }";

            var data = ExecuteGraphQl<StashPerformerMutationData>(settings, mutation, new
            {
                input = new
                {
                    id = performerId,
                    favorite
                }
            });

            return data?.PerformerUpdate ?? throw new InvalidOperationException("Stash did not return the updated performer");
        }

        public StashPerformer CreatePerformer(StashSettings settings, string name, string stashDbId)
        {
            const string mutation = @"mutation($input:PerformerCreateInput!){
                performerCreate(input:$input){id name favorite stash_ids{endpoint stash_id}}
            }";

            var data = ExecuteGraphQl<StashPerformerMutationData>(settings, mutation, new
            {
                input = new
                {
                    name,
                    favorite = true,
                    stash_ids = new[]
                    {
                        new
                        {
                            endpoint = StashDbEndpoint,
                            stash_id = stashDbId
                        }
                    }
                }
            });

            return data?.PerformerCreate ?? throw new InvalidOperationException("Stash did not return the created performer");
        }

        public static bool IsStashDbEndpoint(string endpoint)
        {
            return string.Equals(endpoint?.TrimEnd('/'), StashDbEndpoint, StringComparison.OrdinalIgnoreCase);
        }

        private T ExecuteGraphQl<T>(StashSettings settings, string query, object variables)
        {
            var request = BuildRequest(settings);
            request.Headers.ContentType = "application/json";
            request.SetContent(new { query, variables }.ToJson());

            var content = ProcessRequest(request, settings, false);
            var response = JsonConvert.DeserializeObject<StashGraphQlResponse<T>>(content);

            if (response == null)
            {
                throw new InvalidOperationException("Stash returned an empty GraphQL response");
            }

            if (response.Errors?.Any() == true)
            {
                var messages = string.Join("; ", response.Errors.Select(error => error.Message).Where(message => message.IsNotNullOrWhiteSpace()));
                throw new InvalidOperationException($"Stash GraphQL request failed: {messages}");
            }

            return response.Data;
        }

        private string ProcessRequest(HttpRequest request, StashSettings settings, bool logResponse = true)
        {
            if (settings.ApiKey.IsNotNullOrWhiteSpace())
            {
                request.Headers.Add("ApiKey", settings.ApiKey);
            }

            var response = _httpClient.Post(request);
            if (logResponse)
            {
                _logger.Trace("Response: {0}", response.Content);
            }

            CheckForError(response);

            return response.Content;
        }

        private HttpRequest BuildRequest(StashSettings settings)
        {
            var scheme = settings.UseSsl ? "https" : "http";
            var url = $@"{scheme}://{settings.Address}/graphql";

            return new HttpRequestBuilder(url).Build();
        }

        private void CheckForError(HttpResponse response)
        {
            _logger.Debug("Looking for error in response: {0}", response);

            // TODO: actually check for the error
        }
    }
}
