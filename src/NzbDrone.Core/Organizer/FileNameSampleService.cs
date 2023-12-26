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
        SampleResult GetSceneSample(NamingConfig nameSpec);
        string GetSceneFolderSample(NamingConfig nameSpec);
    }

    public class FileNameSampleService : IFilenameSampleService
    {
        private readonly IBuildFileNames _buildFileNames;

        private static MovieFile _movieFile;
        private static Movie _movie;
        private static Movie _scene;
        private static MovieMetadata _movieMetadata;
        private static MovieMetadata _sceneMetadata;
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

            _movieFile = new MovieFile
            {
                Quality = new QualityModel(Quality.Bluray1080p, new Revision(2)),
                RelativePath = "The.Movie.Title.2010.1080p.BluRay.DTS.x264-EVOLVE.mkv",
                SceneName = "The.Movie.Title.2010.1080p.BluRay.DTS.x264-EVOLVE",
                ReleaseGroup = "EVOLVE",
                MediaInfo = mediaInfo,
                Edition = "Ultimate extended edition",
            };

            _movieMetadata = new MovieMetadata
            {
                Title = "The Movie: Title",
                Year = 2010,
                ImdbId = "tt0066921",
                TmdbId = 345691,
                ForeignId = "345691",
                ItemType = ItemType.Movie
            };

            _sceneMetadata = new MovieMetadata
            {
                Title = "The Scene: Title",
                Year = 2010,
                ImdbId = "tt0066921",
                StashId = "d8f9b8b4-7801-4fa6-bd18-0a4dbd0ce598",
                ForeignId = "d8f9b8b4-7801-4fa6-bd18-0a4dbd0ce598",
                ItemType = ItemType.Scene
            };

            _movie = new Movie
            {
                MovieFile = _movieFile,
                MovieFileId = 1,
                MovieMetadata = _movieMetadata,
                MovieMetadataId = 1
            };

            _scene = new Movie
            {
                MovieFile = _movieFile,
                MovieFileId = 1,
                MovieMetadata = _sceneMetadata,
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

        public SampleResult GetSceneSample(NamingConfig nameSpec)
        {
            var result = new SampleResult
            {
                FileName = BuildSample(_movie, _movieFile, nameSpec),
            };

            return result;
        }

        public string GetSceneFolderSample(NamingConfig nameSpec)
        {
            return _buildFileNames.GetMovieFolder(_movie, nameSpec);
        }

        private string BuildSample(Movie movie, MovieFile movieFile, NamingConfig nameSpec)
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
