using System.Linq;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Tv;
using Whisparr.Http;

namespace Whisparr.Api.V3.SeasonPass
{
    [V3ApiController]
    public class SeasonPassController : Controller
    {
        private readonly ISeriesService _seriesService;
        private readonly IEpisodeMonitoredService _episodeMonitoredService;

        public SeasonPassController(ISeriesService seriesService, IEpisodeMonitoredService episodeMonitoredService)
        {
            _seriesService = seriesService;
            _episodeMonitoredService = episodeMonitoredService;
        }

        [HttpPost]
        [Consumes("application/json")]
        public IActionResult UpdateAll([FromBody] SeasonPassResource resource)
        {
            var seriesToUpdate = _seriesService.GetSeries(resource.Series.Select(s => s.Id));

            foreach (var s in resource.Series)
            {
                var series = seriesToUpdate.Single(c => c.Id == s.Id);

                if (s.Monitored.HasValue)
                {
                    series.Monitored = s.Monitored.Value;
                }

                if (s.Seasons != null && s.Seasons.Any())
                {
                    foreach (var seriesSeason in series.Seasons)
                    {
                        var season = s.Seasons.FirstOrDefault(c => c.SeasonNumber == seriesSeason.SeasonNumber);

                        if (season != null)
                        {
                            seriesSeason.Monitored = season.Monitored;
                        }
                    }
                }

                if (resource.MonitoringOptions != null)
                {
                    if (resource.MonitoringOptions.Monitor == MonitorTypes.None)
                    {
                        series.Monitored = false;
                    }
                    else
                    {
                        // Re-enable series monitoring when changing from None to another monitor type
                        series.Monitored = true;
                    }
                }

                _episodeMonitoredService.SetEpisodeMonitoredStatus(series, resource.MonitoringOptions);
            }

            return Accepted(new object());
        }
    }
}
