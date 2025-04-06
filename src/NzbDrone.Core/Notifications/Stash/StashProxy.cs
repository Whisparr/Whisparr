using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;

namespace NzbDrone.Core.Notifications.Stash
{
    public class StashProxy
    {
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

        private string ProcessRequest(HttpRequest request, StashSettings settings)
        {
            if (settings.ApiKey.IsNotNullOrWhiteSpace())
            {
                request.Headers.Add("ApiKey", settings.ApiKey);
            }

            var response = _httpClient.Post(request);
            _logger.Trace("Response: {0}", response.Content);

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
