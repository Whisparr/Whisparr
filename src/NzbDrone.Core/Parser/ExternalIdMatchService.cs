using System.IO;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Parser
{
    public interface IExternalIdMatchService
    {
        Episode FindEpisode(string filename);
        Episode FindEpisodeInFolder(string folder);
        Series FindSeries(string filename);
        Series FindSeriesInFolder(string folder);
    }

    public class ExternalIdMatchService : IExternalIdMatchService
    {
        private readonly IEpisodeService _episodeService;
        private readonly ISeriesService _seriesService;
        private readonly IDiskScanService _diskScanService;
        private readonly Logger _logger;

        public ExternalIdMatchService(IEpisodeService episodeService,
                                      ISeriesService seriesService,
                                      IDiskScanService diskScanService,
                                      Logger logger)
        {
            _episodeService = episodeService;
            _seriesService = seriesService;
            _diskScanService = diskScanService;
            _logger = logger;
        }

        public Episode FindEpisode(string filename)
        {
            var externalId = Parser.ParseExternalIdFromFilename(filename);

            if (externalId.IsNullOrWhiteSpace())
            {
                return null;
            }

            _logger.Debug("Attempting to find episode by external ID: {0}", externalId);

            var episode = _episodeService.FindByExternalId(externalId);

            if (episode == null)
            {
                _logger.Debug("No episode found with external ID: {0}", externalId);
                return null;
            }

            _logger.Debug("Found episode '{0}' via external ID: {1}", episode.Title, externalId);

            return episode;
        }

        public Episode FindEpisodeInFolder(string folder)
        {
            if (folder.IsNullOrWhiteSpace() || !Directory.Exists(folder))
            {
                return null;
            }

            foreach (var videoFile in _diskScanService.GetVideoFiles(folder))
            {
                var episode = FindEpisode(Path.GetFileName(videoFile));

                if (episode != null)
                {
                    return episode;
                }
            }

            return null;
        }

        public Series FindSeries(string filename)
        {
            var episode = FindEpisode(filename);

            return episode == null ? null : _seriesService.GetSeries(episode.SeriesId);
        }

        public Series FindSeriesInFolder(string folder)
        {
            var episode = FindEpisodeInFolder(folder);

            return episode == null ? null : _seriesService.GetSeries(episode.SeriesId);
        }
    }
}
