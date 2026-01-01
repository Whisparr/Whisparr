using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Whisparr.Http;

namespace Whisparr.Api.V3.Movies
{
    public class MoviePagingRequestResource : PagingRequestResource
    {
        private const int DefaultPageSize = 100;
        private const int MaxPageSize = 1000;

        public MoviePagingRequestResource()
        {
            Page = Page <= 0 ? 1 : Page;
            PageSize = PageSize is { } size && size > 0 ? size : DefaultPageSize;
        }

        /// <summary>
        /// JSON encoded array describing selected filters. Each entry should align with the filter
        /// structure used by the frontend custom filter builder.
        /// </summary>
        [FromQuery(Name = "filters")]
        [JsonPropertyName("filters")]
        public string FilterPayload { get; set; }

        /// <summary>
        /// Ensures the requested page size respects default and maximum constraints.
        /// </summary>
        public int ResolvePageSize()
        {
            var size = PageSize ?? DefaultPageSize;

            if (size <= 0)
            {
                size = DefaultPageSize;
            }

            if (size > MaxPageSize)
            {
                size = MaxPageSize;
            }

            return size;
        }

        public int ResolvePage()
        {
            var page = Page ?? 1;
            return page <= 0 ? 1 : page;
        }
    }
}
