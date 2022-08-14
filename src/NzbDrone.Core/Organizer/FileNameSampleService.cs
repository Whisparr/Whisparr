using System.Collections.Generic;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Organizer
{
    public interface IFilenameSampleService
    {
        SampleResult GetMovieSample(NamingConfig nameSpec);
        string GetMovieFolderSample(NamingConfig nameSpec);
    }

    public class FileNameSampleService : IFilenameSampleService
    {
        private readonly IBuildFileNames _buildFileNames;

        private static MediaFile _movieFile;
        private static Media _movie;
        private static MediaMetadata _movieMetadata;
        private static List<CustomFormat> _customFormats;

        public FileNameSampleService(IBuildFileNames buildFileNames)
        {
            _buildFileNames = buildFileNames;

            var mediaInfo = new MediaInfoModel()
            {
                VideoFormat = "AVC",
                VideoBitDepth = 10,
                VideoMultiViewCount = 2,
                VideoColourPrimaries = "bt2020",
                VideoTransferCharacteristics = "HLG",
                AudioFormat = "DTS",
                AudioChannels = 6,
                AudioChannelPositions = "5.1",
                AudioLanguages = new List<string> { "ger" },
                Subtitles = new List<string> { "eng", "ger" }
            };

            _movieFile = new MediaFile
            {
                Quality = new QualityModel(Quality.Bluray1080p, new Revision(2)),
                RelativePath = "The.Movie.Title.2010.1080p.BluRay.DTS.x264-EVOLVE.mkv",
                SceneName = "The.Movie.Title.2010.1080p.BluRay.DTS.x264-EVOLVE",
                ReleaseGroup = "EVOLVE",
                MediaInfo = mediaInfo,
                Edition = "Ultimate extended edition",
            };

            _movieMetadata = new MediaMetadata
            {
                Title = "The Movie: Title",
                OriginalTitle = "The Original Movie Title",
                Collection = new MovieCollection { Name = "The Movie Collection", TmdbId = 123654 },
                Certification = "R",
                Year = 2010,
                ForiegnId = 345691
            };

            _movie = new Media
            {
                MovieFile = _movieFile,
                MovieFileId = 1,
                MediaMetadata = _movieMetadata,
                MovieMetadataId = 1
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
        }

        public SampleResult GetMovieSample(NamingConfig nameSpec)
        {
            var result = new SampleResult
            {
                FileName = BuildSample(_movie, _movieFile, nameSpec),
            };

            return result;
        }

        public string GetMovieFolderSample(NamingConfig nameSpec)
        {
            return _buildFileNames.GetMovieFolder(_movie, nameSpec);
        }

        private string BuildSample(Media movie, MediaFile movieFile, NamingConfig nameSpec)
        {
            try
            {
                return _buildFileNames.BuildFileName(movie, movieFile, nameSpec, _customFormats);
            }
            catch (NamingFormatException)
            {
                return string.Empty;
            }
        }
    }
}
