using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.MediaFiles.MovieImport.Aggregation;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.MovieImport
{
    public interface IMakeImportDecision
    {
        List<ImportDecision> GetImportDecisions(List<string> videoFiles, Movie movie);
        (List<Movie> MoviesToAdd, List<MovieFile> UnmappedFiles, List<ImportDecision> Decisions) GetSceneImportDecisions(List<string> videoFiles);
        List<ImportDecision> GetImportDecisions(List<string> videoFiles, Movie movie, bool filterExistingFiles);
        List<ImportDecision> GetImportDecisions(List<string> videoFiles, Movie movie, DownloadClientItem downloadClientItem, ParsedMovieInfo folderInfo, bool sceneSource);
        List<ImportDecision> GetImportDecisions(List<string> videoFiles, Movie movie, DownloadClientItem downloadClientItem, ParsedMovieInfo folderInfo, bool sceneSource, bool filterExistingFiles);
        ImportDecision GetDecision(LocalMovie localMovie, DownloadClientItem downloadClientItem);
    }

    public class ImportDecisionMaker : IMakeImportDecision
    {
        private readonly IEnumerable<IImportDecisionEngineSpecification> _specifications;
        private readonly IMediaFileService _mediaFileService;
        private readonly IMovieService _movieService;
        private readonly IAggregationService _aggregationService;
        private readonly ISceneIdentificationService _identificationService;
        private readonly IDiskProvider _diskProvider;
        private readonly IDetectSample _detectSample;
        private readonly IParsingService _parsingService;
        private readonly ICustomFormatCalculationService _formatCalculator;
        private readonly IVideoFileInfoReader _videoFileInfoReader;
        private readonly Logger _logger;

        public ImportDecisionMaker(IEnumerable<IImportDecisionEngineSpecification> specifications,
                                   IMediaFileService mediaFileService,
                                   IMovieService movieService,
                                   IAggregationService aggregationService,
                                   ISceneIdentificationService identificationService,
                                   IDiskProvider diskProvider,
                                   IDetectSample detectSample,
                                   IParsingService parsingService,
                                   ICustomFormatCalculationService formatCalculator,
                                   IVideoFileInfoReader videoFileInfoReader,
                                   Logger logger)
        {
            _specifications = specifications;
            _mediaFileService = mediaFileService;
            _movieService = movieService;
            _aggregationService = aggregationService;
            _diskProvider = diskProvider;
            _detectSample = detectSample;
            _identificationService = identificationService;
            _parsingService = parsingService;
            _formatCalculator = formatCalculator;
            _videoFileInfoReader = videoFileInfoReader;
            _logger = logger;
        }

        public List<ImportDecision> GetImportDecisions(List<string> videoFiles, Movie movie)
        {
            return GetImportDecisions(videoFiles, movie, null, null, false);
        }

        public (List<Movie> MoviesToAdd, List<MovieFile> UnmappedFiles, List<ImportDecision> Decisions) GetSceneImportDecisions(List<string> videoFiles)
        {
            return GetSceneImportDecisions(videoFiles, null, null, false, true);
        }

        public List<ImportDecision> GetImportDecisions(List<string> videoFiles, Movie movie, bool filterExistingFiles)
        {
            return GetImportDecisions(videoFiles, movie, null, null, false, filterExistingFiles);
        }

        public List<ImportDecision> GetImportDecisions(List<string> videoFiles, Movie movie, DownloadClientItem downloadClientItem, ParsedMovieInfo folderInfo, bool sceneSource)
        {
            return GetImportDecisions(videoFiles, movie, downloadClientItem, folderInfo, sceneSource, true);
        }

        public List<ImportDecision> GetImportDecisions(List<string> videoFiles, Movie movie, DownloadClientItem downloadClientItem, ParsedMovieInfo folderInfo, bool sceneSource, bool filterExistingFiles)
        {
            var newFiles = filterExistingFiles ? _mediaFileService.FilterExistingFiles(videoFiles.ToList(), movie) : videoFiles.ToList();

            _logger.Debug("Analyzing {0}/{1} files.", newFiles.Count, videoFiles.Count);

            ParsedMovieInfo downloadClientItemInfo = null;

            if (downloadClientItem != null)
            {
                downloadClientItemInfo = Parser.Parser.ParseMovieTitle(downloadClientItem.Title);
            }

            var nonSampleVideoFileCount = GetNonSampleVideoFileCount(newFiles, movie.MovieMetadata);

            var decisions = new List<ImportDecision>();

            foreach (var file in newFiles)
            {
                var localMovie = new LocalMovie
                {
                    Movie = movie,
                    DownloadClientMovieInfo = downloadClientItemInfo,
                    DownloadItem = downloadClientItem,
                    FolderMovieInfo = folderInfo,
                    Path = file,
                    SceneSource = sceneSource,
                    ExistingFile = movie.Path.IsParentPath(file),
                    OtherVideoFiles = nonSampleVideoFileCount > 1
                };

                decisions.AddIfNotNull(GetDecision(localMovie, downloadClientItem, nonSampleVideoFileCount > 1));
            }

            return decisions;
        }

        public (List<Movie> MoviesToAdd, List<MovieFile> UnmappedFiles, List<ImportDecision> Decisions) GetSceneImportDecisions(List<string> videoFiles, DownloadClientItem downloadClientItem, ParsedMovieInfo folderInfo, bool sceneSource, bool filterExistingFiles)
        {
            var newFiles = filterExistingFiles ? _mediaFileService.FilterExistingFiles(videoFiles.ToList()) : videoFiles.ToList();

            _logger.Debug("Analyzing {0}/{1} files.", newFiles.Count, videoFiles.Count);

            ParsedMovieInfo downloadClientItemInfo = null;

            if (downloadClientItem != null)
            {
                downloadClientItemInfo = Parser.Parser.ParseMovieTitle(downloadClientItem.Title);
            }

            var moviesToAdd = new List<Movie>();
            var unmappedFiles = new List<MovieFile>();
            var decisions = new List<ImportDecision>();

            var i = 0;
            foreach (var file in newFiles)
            {
                i++;
                _logger.ProgressInfo($"Identifying scene {i}/{newFiles.Count}");
                _logger.Debug($"Identifying scene - {file}");
                var movie = GetRemoteMovie(file.ToString());

                var fileMovieInfo = Parser.Parser.ParseMoviePath(file) ?? new ParsedMovieInfo();
                var localMovie = new LocalMovie
                {
                    Movie = movie,
                    DownloadClientMovieInfo = downloadClientItemInfo,
                    DownloadItem = downloadClientItem,
                    FolderMovieInfo = folderInfo,
                    FileMovieInfo = fileMovieInfo,
                    Path = movie.Path,
                    SceneSource = sceneSource,
                };

                if (movie.ForeignId == null)
                {
                    // Add the movieFile if no match to a movie
                    var unmappedFile = new MovieFile();

                    unmappedFile.DateAdded = DateTime.UtcNow;
                    unmappedFile.Languages = new List<Languages.Language>();
                    unmappedFile.MediaInfo = _videoFileInfoReader.GetMediaInfo(file.ToString());
                    unmappedFile.OriginalFilePath = file.ToString();
                    unmappedFile.Size = _diskProvider.GetFileSize(file.ToString());

                    localMovie.Movie.MovieFile = unmappedFile;
                    localMovie.Path = file.ToString();

                    // Augment movie file so imported files have all additional information an automatic import would
                    localMovie = _aggregationService.Augment(localMovie, null);
                    unmappedFile.Quality = localMovie.Quality;

                    unmappedFiles.Add(unmappedFile);
                    continue;
                }

                if (movie.Id == 0)
                {
                    moviesToAdd.Add(movie);
                    continue;
                }

                decisions.AddIfNotNull(GetDecision(localMovie, downloadClientItem, false));
            }

            return (moviesToAdd, unmappedFiles, decisions);
        }

        public ImportDecision GetDecision(LocalMovie localMovie, DownloadClientItem downloadClientItem)
        {
            var reasons = _specifications.Select(c => EvaluateSpec(c, localMovie, downloadClientItem))
                                         .Where(c => c != null);

            return new ImportDecision(localMovie, reasons.ToArray());
        }

        private Movie GetRemoteMovie(string file)
        {
            var scene = new Movie();
            var fileMovieInfo = Parser.Parser.ParseMoviePath(file);

            if (fileMovieInfo == null)
            {
                return scene;
            }

            // Try to get match by searching for the scene
            scene = _identificationService.Identify(file, fileMovieInfo);

            return scene;
        }

        private ImportDecision GetDecision(LocalMovie localMovie, DownloadClientItem downloadClientItem, bool otherFiles)
        {
            ImportDecision decision = null;

            var fileMovieInfo = Parser.Parser.ParseMoviePath(localMovie.Path);

            localMovie.FileMovieInfo = fileMovieInfo;
            localMovie.Size = _diskProvider.GetFileSize(localMovie.Path);

            try
            {
                _aggregationService.Augment(localMovie, downloadClientItem);

                if (localMovie.Movie.MovieMetadata.Value.ItemType == ItemType.Movie)
                {
                    decision = new ImportDecision(localMovie, new Rejection("Invalid movie"));
                    if (localMovie.Movie == null)
                    {
                        decision = new ImportDecision(localMovie, new Rejection("Invalid movie"));
                    }
                    else
                    {
                        localMovie.CustomFormats = _formatCalculator.ParseCustomFormat(localMovie);
                        localMovie.CustomFormatScore = localMovie.Movie.QualityProfile?.CalculateCustomFormatScore(localMovie.CustomFormats) ?? 0;

                        decision = GetDecision(localMovie, downloadClientItem);
                    }
                }
                else
                {
                    var matchedMovie = new Movie();

                    if (localMovie.Movie != null)
                    {
                        matchedMovie = _movieService.GetMovie(localMovie.Movie.Id);

                        // Switch the movie so that the QualityProfile is populated
                        localMovie.Movie = matchedMovie;
                    }
                    else
                    {
                        matchedMovie = _movieService.FindScene(fileMovieInfo);
                        if (matchedMovie.Title != localMovie.Movie.Title)
                        {
                            decision = new ImportDecision(localMovie, new Rejection("Invalid movie"));
                        }
                    }

                    localMovie.CustomFormats = _formatCalculator.ParseCustomFormat(localMovie);
                    localMovie.CustomFormatScore = localMovie.Movie.QualityProfile?.CalculateCustomFormatScore(localMovie.CustomFormats) ?? 0;

                    decision = GetDecision(localMovie, downloadClientItem);
                }
            }
            catch (AugmentingFailedException)
            {
                decision = new ImportDecision(localMovie, new Rejection("Unable to parse file"));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Couldn't import file. {0}", localMovie.Path);

                decision = new ImportDecision(localMovie, new Rejection("Unexpected error processing file"));
            }

            if (decision == null)
            {
                _logger.Error("Unable to make a decision on {0}", localMovie.Path);
            }
            else if (decision.Rejections.Any())
            {
                _logger.Debug("File rejected for the following reasons: {0}", string.Join(", ", decision.Rejections));
            }
            else
            {
                _logger.Debug("File accepted");
            }

            return decision;
        }

        private Rejection EvaluateSpec(IImportDecisionEngineSpecification spec, LocalMovie localMovie, DownloadClientItem downloadClientItem)
        {
            try
            {
                var result = spec.IsSatisfiedBy(localMovie, downloadClientItem);

                if (!result.Accepted)
                {
                    return new Rejection(result.Reason);
                }
            }
            catch (NotImplementedException e)
            {
                _logger.Warn(e, "Spec " + spec.ToString() + " currently does not implement evaluation for movies.");
                return null;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Couldn't evaluate decision on {0}", localMovie.Path);
                return new Rejection($"{spec.GetType().Name}: {ex.Message}");
            }

            return null;
        }

        private int GetNonSampleVideoFileCount(List<string> videoFiles, MovieMetadata movie)
        {
            return videoFiles.Count(file =>
            {
                var sample = _detectSample.IsSample(movie, file);

                if (sample == DetectSampleResult.Sample)
                {
                    return false;
                }

                return true;
            });
        }
    }
}
