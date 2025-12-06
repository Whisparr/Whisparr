using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.MediaFiles.MovieImport.Aggregation.Aggregators.Augmenters.Quality
{
    public class AugmentQualityFromMediaInfo : IAugmentQuality
    {
        // Threshold constants for readability and easier adjustments
        private const int Threshold8KWidth = 7500;
        private const int Threshold8KHeight = 4300;

        private const int Threshold6KRedWidth = 6100;   // RED 6144x3160 (loose lower-bound)
        private const int Threshold6KRedHeight = 3100;

        private const int Threshold6KBMWidth = 6000;    // Blackmagic 6016x3384 (loose lower-bound)
        private const int Threshold6KBMHeight = 3300;

        private const int Threshold5KWidth = 5100;      // 5120x2880
        private const int Threshold5KHeight = 2800;

        private const int Threshold4KWidth = 3200;      // loose 3840x2160 match
        private const int Threshold4KHeight = 2100;

        private const int Threshold1080pWidth = 1800;
        private const int Threshold1080pHeight = 1000;

        private const int Threshold720pWidth = 1200;
        private const int Threshold720pHeight = 700;

        private const int Threshold576pWidth = 1000;
        private const int Threshold576pHeight = 560;

        private readonly Logger _logger;

        public int Order => 4;
        public string Name => "MediaInfo";

        public AugmentQualityFromMediaInfo(Logger logger)
        {
            _logger = logger;
        }

        public AugmentQualityResult AugmentQuality(LocalMovie localMovie, DownloadClientItem downloadClientItem)
        {
            if (localMovie.MediaInfo == null)
            {
                return null;
            }

            var width = localMovie.MediaInfo.Width;
            var height = localMovie.MediaInfo.Height;
            var source = QualitySource.Unknown;
            var sourceConfidence = Confidence.Default;
            var title = localMovie.MediaInfo.Title;

            if (title.IsNotNullOrWhiteSpace())
            {
                var parsedQuality = QualityParser.ParseQualityName(title.Trim());

                // Only use the quality if it's not unknown and the source is from the name (which is MediaInfo's title in this case)
                if (parsedQuality.Quality.Source != QualitySource.Unknown &&
                    parsedQuality.SourceDetectionSource == QualityDetectionSource.Name)
                {
                    source = parsedQuality.Quality.Source;
                    sourceConfidence = Confidence.MediaInfo;
                }
            }

            // If dimensions are not present, nothing to augment
            if (width <= 0 || height <= 0)
            {
                _logger.Trace("MediaInfo has no valid dimensions (width={0}, height={1})", width, height);
                return null;
            }

            // 8K
            if (width >= Threshold8KWidth || height >= Threshold8KHeight)
            {
                _logger.Trace("Resolution {0}x{1} considered 4320p (8K)", width, height);
                return AugmentQualityResult.SourceAndResolutionOnly(source, sourceConfidence, (int)Resolution.R4320p, Confidence.MediaInfo);
            }

            // 6K RED (6144x3160)
            if (width >= Threshold6KRedWidth && height >= Threshold6KRedHeight)
            {
                _logger.Trace("Resolution {0}x{1} considered 3160p (6K - RED)", width, height);
                return AugmentQualityResult.SourceAndResolutionOnly(source, sourceConfidence, (int)Resolution.R3160p, Confidence.MediaInfo);
            }

            // 6K Blackmagic (6016x3384)
            if (width >= Threshold6KBMWidth && height >= Threshold6KBMHeight)
            {
                _logger.Trace("Resolution {0}x{1} considered 3384p (6K - Blackmagic)", width, height);
                return AugmentQualityResult.SourceAndResolutionOnly(source, sourceConfidence, (int)Resolution.R3384p, Confidence.MediaInfo);
            }

            // 5k (5120x2880)
            if (width >= Threshold5KWidth && height >= Threshold5KHeight)
            {
                _logger.Trace("Resolution {0}x{1} considered 2880p (5K)", width, height);
                return AugmentQualityResult.SourceAndResolutionOnly(source, sourceConfidence, (int)Resolution.R2880p, Confidence.MediaInfo);
            }

            // 4K (UHD/2160p)
            if (width >= Threshold4KWidth || height >= Threshold4KHeight)
            {
                _logger.Trace("Resolution {0}x{1} considered 2160p (4K)", width, height);
                return AugmentQualityResult.SourceAndResolutionOnly(source, sourceConfidence, (int)Resolution.R2160p, Confidence.MediaInfo);
            }

            // 1080p
            if (width >= Threshold1080pWidth || height >= Threshold1080pHeight)
            {
                _logger.Trace("Resolution {0}x{1} considered 1080p", width, height);
                return AugmentQualityResult.SourceAndResolutionOnly(source, sourceConfidence, (int)Resolution.R1080p, Confidence.MediaInfo);
            }

            if (width >= Threshold720pWidth || height >= Threshold720pHeight)
            {
                _logger.Trace("Resolution {0}x{1} considered 720p", width, height);
                return AugmentQualityResult.SourceAndResolutionOnly(source, sourceConfidence, (int)Resolution.R720p, Confidence.MediaInfo);
            }

            if (width >= Threshold576pWidth || height >= Threshold576pHeight)
            {
                _logger.Trace("Resolution {0}x{1} considered 576p", width, height);
                return AugmentQualityResult.SourceAndResolutionOnly(source, sourceConfidence, (int)Resolution.R576p, Confidence.MediaInfo);
            }

            _logger.Trace("Resolution {0}x{1} considered 480p", width, height);
            return AugmentQualityResult.SourceAndResolutionOnly(source, sourceConfidence, (int)Resolution.R480p, Confidence.MediaInfo);
        }
    }
}
