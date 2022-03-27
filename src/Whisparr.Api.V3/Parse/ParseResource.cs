using NzbDrone.Core.Parser.Model;
using Whisparr.Api.V3.Movies;
using Whisparr.Http.REST;

namespace Whisparr.Api.V3.Parse
{
    public class ParseResource : RestResource
    {
        public string Title { get; set; }
        public ParsedMovieInfo ParsedMovieInfo { get; set; }
        public MovieResource Movie { get; set; }
    }
}
