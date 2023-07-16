using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.IndexerSearch.Definitions;

namespace NzbDrone.Core.Indexers.HDBits
{
    public class HDBitsRequestGenerator : IIndexerRequestGenerator
    {
        public HDBitsSettings Settings { get; set; }

        public virtual IndexerPageableRequestChain GetRecentRequests()
        {
            var pageableRequests = new IndexerPageableRequestChain();

            pageableRequests.Add(GetRequest(new TorrentQuery()));

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(SeasonSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            var queryBase = new TorrentQuery();
            if (TryAddSearchParameters(queryBase, searchCriteria))
            {
                foreach (var seasonNumber in searchCriteria.Episodes.Select(e => e.SeasonNumber).Distinct())
                {
                    var query = queryBase.Clone();

                    query.Search = $"{query.Search} {seasonNumber}";

                    pageableRequests.Add(GetRequest(query));
                }
            }

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(SingleEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            var releaseDate = searchCriteria.ReleaseDate?.ToString("yy.MM.dd") ?? string.Empty;

            var queryBase = new TorrentQuery();
            if (TryAddSearchParameters(queryBase, searchCriteria))
            {
                foreach (var episode in searchCriteria.Episodes)
                {
                    var query = queryBase.Clone();

                    query.Search = $"{query.Search} {releaseDate}";

                    pageableRequests.Add(GetRequest(query));
                }
            }

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(MovieSearchCriteria searchCriteria)
        {
            return new IndexerPageableRequestChain();
        }

        private bool TryAddSearchParameters(TorrentQuery query, SceneSearchCriteriaBase searchCriteria)
        {
            if (searchCriteria.Series.Title.IsNotNullOrWhiteSpace())
            {
                query.Search = searchCriteria.Series.Title;
                return true;
            }

            return false;
        }

        private IEnumerable<IndexerRequest> GetRequest(TorrentQuery query)
        {
            var request = new HttpRequestBuilder(Settings.BaseUrl)
                .Resource("/api/torrents")
                .Build();

            request.Method = HttpMethod.Post;
            const string appJson = "application/json";
            request.Headers.Accept = appJson;
            request.Headers.ContentType = appJson;

            query.Username = Settings.Username;
            query.Passkey = Settings.ApiKey;

            request.SetContent(query.ToJson());
            request.ContentSummary = query.ToJson(Formatting.None);

            yield return new IndexerRequest(request);
        }
    }
}
