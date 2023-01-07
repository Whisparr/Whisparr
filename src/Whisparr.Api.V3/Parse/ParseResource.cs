using System.Collections.Generic;
using NzbDrone.Core.Parser.Model;
using Whisparr.Api.V3.Episodes;
using Whisparr.Api.V3.Series;
using Whisparr.Http.REST;

namespace Whisparr.Api.V3.Parse
{
    public class ParseResource : RestResource
    {
        public string Title { get; set; }
        public ParsedEpisodeInfo ParsedEpisodeInfo { get; set; }
        public SeriesResource Series { get; set; }
        public List<EpisodeResource> Episodes { get; set; }
    }
}
