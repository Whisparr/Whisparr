using NLog;
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

        public void Update(StashSettings settings, string path)
        {
            var request = BuildRequest(settings);
            request.Headers.ContentType = "application/json";

            var generatePreviews = settings.GeneratePreviews ? "true" : "false";
            var cleanPath = path.ToJson();


            var MetadataIdentifyQuery = ""
            if (settings.MetadataIdentify) {
                MetadataIdentifyQuery =  $@"metadataIdentify(
                                            input: {{
                                            sources: [
                                                {{source: {{stash_box_index: 0}}, options: {{setOrganized: false}} }}, 
                                                {{source: {{scraper_id: ""builtin_autotag""}}, options: {{setOrganized: false}} }}
                                            ],
                                            options: {{includeMalePerformers: false, setCoverImage: true, setOrganized: false}}, 
                                            paths: [{cleanPath}]
                                       }})"
            }

            request.SetContent(new
            {           
                Query = $@"mutation {{
                            metadataScan(
                            input: {{
                                useFileMetadata: false,
                                stripFileExtension: true,
                                scanGenerateCovers: {(settings.GenerateCovers ? "true" : "false")},
                                scanGeneratePreviews: {(settings.GeneratePreviews ? "true" : "false")},
                                scanGenerateImagePreviews: {(settings.GenerateImagePreviews ? "true" : "false")},
                                scanGenerateSprites: {(settings.GenerateSprites ? "true" : "false")},
                                scanGeneratePhashes: {(settings.GeneratePhashes ? "true" : "false")},
                                paths: [{cleanPath}]
                            }})
                            {(settings.MetadataIdentify ? MetadataIdentifyQuery : "" )}
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
            request.Headers.Add("ApiKey", settings.ApiKey);

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
