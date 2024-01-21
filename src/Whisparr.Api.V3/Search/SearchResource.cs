using Whisparr.Api.V3.Movies;
using Whisparr.Api.V3.Performers;
using Whisparr.Http.REST;

namespace Whisparr.Api.V3.Search
{
    public class
    SearchResource : RestResource
    {
        public string ForeignId { get; set; }
        public MovieResource Movie { get; set; }
        public PerformerResource Performer { get; set; }
    }
}
