using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Whisparr.Http;

namespace Whisparr.Api.V3.Studios
{
    public class StudioPagingRequestResource : PagingRequestResource
    {
        private const int DefaultPageSize = 100;
        private const int MaxPageSize = 1000;

        public StudioPagingRequestResource()
        {
            Page = Page <= 0 ? 1 : Page;
            PageSize = PageSize is { } size && size > 0 ? size : DefaultPageSize;
        }

        [FromQuery(Name = "filters")]
        [JsonPropertyName("filters")]
        public string FilterPayload { get; set; }

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
