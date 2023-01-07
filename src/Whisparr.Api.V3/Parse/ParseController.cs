using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Download.Aggregation;
using NzbDrone.Core.Parser;
using Whisparr.Api.V3.Episodes;
using Whisparr.Api.V3.Series;
using Whisparr.Http;

namespace Whisparr.Api.V3.Parse
{
    [V3ApiController]
    public class ParseController : Controller
    {
        private readonly IParsingService _parsingService;
        private readonly IRemoteEpisodeAggregationService _aggregationService;

        public ParseController(IParsingService parsingService,
                               IRemoteEpisodeAggregationService aggregationService)
        {
            _parsingService = parsingService;
            _aggregationService = aggregationService;
        }

        [HttpGet]
        [Produces("application/json")]
        public ParseResource Parse(string title, string path)
        {
            if (title.IsNullOrWhiteSpace())
            {
                return null;
            }

            var parsedEpisodeInfo = path.IsNotNullOrWhiteSpace() ? Parser.ParsePath(path) : Parser.ParseTitle(title);

            if (parsedEpisodeInfo == null)
            {
                return new ParseResource
                {
                    Title = title
                };
            }

            var remoteEpisode = _parsingService.Map(parsedEpisodeInfo, 0);

            _aggregationService.Augment(remoteEpisode);

            if (remoteEpisode != null)
            {
                return new ParseResource
                {
                    Title = title,
                    ParsedEpisodeInfo = remoteEpisode.ParsedEpisodeInfo,
                    Series = remoteEpisode.Series.ToResource(),
                    Episodes = remoteEpisode.Episodes.ToResource()
                };
            }
            else
            {
                return new ParseResource
                {
                    Title = title,
                    ParsedEpisodeInfo = parsedEpisodeInfo
                };
            }
        }
    }
}
