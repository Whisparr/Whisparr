using System.Collections.Generic;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Organizer
{
    public interface IFilenameSampleService
    {
        SampleResult GetStandardSample(NamingConfig nameSpec);
        string GetSeriesFolderSample(NamingConfig nameSpec);
    }

    public class FileNameSampleService : IFilenameSampleService
    {
        private readonly IBuildFileNames _buildFileNames;
        private static Series _standardSeries;
        private static Episode _episode1;
        private static Episode _episode2;
        private static Episode _episode3;
        private static List<Episode> _singleEpisode;
        private static EpisodeFile _singleEpisodeFile;
        private static List<CustomFormat> _customFormats;

        public FileNameSampleService(IBuildFileNames buildFileNames)
        {
            _buildFileNames = buildFileNames;

            _standardSeries = new Series
            {
                Title = "The Site Title!",
                Year = 2010,
                TvdbId = 12345,
            };

            _episode1 = new Episode
            {
                SeasonNumber = 2022,
                EpisodeNumber = 1,
                Title = "Episode Title (1)",
                AirDate = "2022-05-01",
                AbsoluteEpisodeNumber = 1,
            };

            _episode2 = new Episode
            {
                SeasonNumber = 2022,
                EpisodeNumber = 2,
                Title = "Episode Title (2)",
                AbsoluteEpisodeNumber = 2
            };

            _episode3 = new Episode
            {
                SeasonNumber = 2022,
                EpisodeNumber = 3,
                Title = "Episode Title (3)",
                AbsoluteEpisodeNumber = 3
            };

            _singleEpisode = new List<Episode> { _episode1 };

            var mediaInfo = new MediaInfoModel()
            {
                VideoFormat = "AVC",
                VideoBitDepth = 10,
                VideoColourPrimaries = "bt2020",
                VideoTransferCharacteristics = "HLG",
                AudioFormat = "DTS",
                AudioChannels = 6,
                AudioChannelPositions = "5.1",
                AudioLanguages = new List<string> { "ger" },
                Subtitles = new List<string> { "eng", "ger" }
            };

            var mediaInfoAnime = new MediaInfoModel()
            {
                VideoFormat = "AVC",
                VideoBitDepth = 10,
                VideoColourPrimaries = "BT.2020",
                VideoTransferCharacteristics = "HLG",
                AudioFormat = "DTS",
                AudioChannels = 6,
                AudioChannelPositions = "5.1",
                AudioLanguages = new List<string> { "jpn" },
                Subtitles = new List<string> { "jpn", "eng" }
            };

            _customFormats = new List<CustomFormat>
            {
                new CustomFormat
                {
                    Name = "Surround Sound",
                    IncludeCustomFormatWhenRenaming = true
                },
                new CustomFormat
                {
                    Name = "x264",
                    IncludeCustomFormatWhenRenaming = true
                }
            };

            _singleEpisodeFile = new EpisodeFile
            {
                Quality = new QualityModel(Quality.HDTV720p, new Revision(2)),
                RelativePath = "Site.Title.2022-05-01.720p.HDTV.x264-EVOLVE.mkv",
                SceneName = "Site.Title.2022-05-01.720p.HDTV.x264-EVOLVE",
                ReleaseGroup = "RlsGrp",
                MediaInfo = mediaInfo
            };
        }

        public SampleResult GetStandardSample(NamingConfig nameSpec)
        {
            var result = new SampleResult
            {
                FileName = BuildSample(_singleEpisode, _standardSeries, _singleEpisodeFile, nameSpec, _customFormats),
                Series = _standardSeries,
                Episodes = _singleEpisode,
                EpisodeFile = _singleEpisodeFile
            };

            return result;
        }

        public string GetSeriesFolderSample(NamingConfig nameSpec)
        {
            return _buildFileNames.GetSeriesFolder(_standardSeries, nameSpec);
        }

        private string BuildSample(List<Episode> episodes, Series series, EpisodeFile episodeFile, NamingConfig nameSpec, List<CustomFormat> customFormats)
        {
            try
            {
                return _buildFileNames.BuildFileName(episodes, series, episodeFile, "", nameSpec, customFormats);
            }
            catch (NamingFormatException)
            {
                return string.Empty;
            }
        }
    }
}
