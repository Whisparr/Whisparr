using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles.Commands;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.MediaFiles.MovieImport;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Commands;
using NzbDrone.Core.Movies.Studios;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.RootFolders;

namespace NzbDrone.Core.MediaFiles
{
    public interface ISceneDiskScanService
    {
        void Scan(List<string> folders = null);
        string[] GetVideoFiles(string path, bool allDirectories = true);
        string[] GetNonVideoFiles(string path, bool allDirectories = true);
        List<string> FilterFiles(string basePath, List<string> files);
        List<string> FilterPaths(string basePath, IEnumerable<string> paths, bool filterExtras = true);
    }

    public class SceneDiskScanService :
        ISceneDiskScanService,
        IExecute<RescanScenesCommand>
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IAddStudioService _addStudioService;
        private readonly IMakeImportDecision _importDecisionMaker;
        private readonly IImportApprovedMovie _importApprovedMovies;
        private readonly IConfigService _configService;
        private readonly IManageCommandQueue _commandQueueManager;
        private readonly IMovieService _movieService;
        private readonly IMediaFileService _mediaFileService;
        private readonly IMediaFileTableCleanupService _mediaFileTableCleanupService;
        private readonly INamingConfigService _namingConfigService;
        private readonly IRootFolderService _rootFolderService;
        private readonly IUpdateMediaInfo _updateMediaInfoService;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public SceneDiskScanService(IDiskProvider diskProvider,
                               IAddStudioService addStudioService,
                               IMakeImportDecision importDecisionMaker,
                               IImportApprovedMovie importApprovedMovies,
                               IConfigService configService,
                               IManageCommandQueue commandQueueManager,
                               IMovieService movieService,
                               IMediaFileService mediaFileService,
                               IMediaFileTableCleanupService mediaFileTableCleanupService,
                               INamingConfigService namingConfigService,
                               IRootFolderService rootFolderService,
                               IUpdateMediaInfo updateMediaInfoService,
                               IEventAggregator eventAggregator,
                               Logger logger)
        {
            _diskProvider = diskProvider;
            _addStudioService = addStudioService;
            _importDecisionMaker = importDecisionMaker;
            _importApprovedMovies = importApprovedMovies;
            _configService = configService;
            _commandQueueManager = commandQueueManager;
            _movieService = movieService;
            _mediaFileService = mediaFileService;
            _mediaFileTableCleanupService = mediaFileTableCleanupService;
            _namingConfigService = namingConfigService;
            _rootFolderService = rootFolderService;
            _updateMediaInfoService = updateMediaInfoService;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        private static readonly Regex ExcludedExtrasSubFolderRegex = new Regex(@"(?:\\|\/|^)(?:extras|extrafanart|behind the scenes|deleted scenes|featurettes|interviews|scenes|sample[s]?|shorts|trailers)(?:\\|\/)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex ExcludedSubFoldersRegex = new Regex(@"(?:\\|\/|^)(?:@eadir|\.@__thumb|plex versions|\.[^\\/]+)(?:\\|\/)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex ExcludedExtraFilesRegex = new Regex(@"(-(trailer|other|behindthescenes|deleted|featurette|interview|scene|short)\.[^.]+$)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex ExcludedFilesRegex = new Regex(@"^\._|^Thumbs\.db$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public void Scan(List<string> folders = null)
        {
            if (folders == null)
            {
                folders = _rootFolderService.All().Select(x => x.Path).ToList();
            }

            var movieIds = new List<int>();

            var mediaFileList = new List<string>();

            var videoFilesStopwatch = Stopwatch.StartNew();

            foreach (var folder in folders)
            {
                var rootFolder = folder;
                var scenesImportFolder = GetImportFolder(folder);

                if (rootFolder == null)
                {
                    _logger.Error("Not scanning {0}, it's not a subdirectory of a defined root folder", scenesImportFolder);
                    return;
                }

                var folderExists = _diskProvider.FolderExists(scenesImportFolder);

                if (!folderExists)
                {
                    if (!_diskProvider.FolderExists(rootFolder))
                    {
                        _logger.Warn("Movie's root folder ({0}) doesn't exist.", rootFolder);
                        var skippedMovies = _movieService.GetMovies(movieIds);
                        skippedMovies.ForEach(x => _eventAggregator.PublishEvent(new MovieScanSkippedEvent(x, MovieScanSkippedReason.RootFolderDoesNotExist)));
                        return;
                    }

                    if (_diskProvider.FolderEmpty(rootFolder))
                    {
                        _logger.Warn("Movie's root folder ({0}) is empty. Rescan will not update movies as a failsafe.", rootFolder);
                        var skippedMovies = _movieService.GetMovies(movieIds);
                        skippedMovies.ForEach(x => _eventAggregator.PublishEvent(new MovieScanSkippedEvent(x, MovieScanSkippedReason.RootFolderDoesNotExist)));
                        return;
                    }
                }

                if (!folderExists)
                {
                    _logger.Debug("Specified scan folder ({0}) doesn't exist.", scenesImportFolder);

                    continue;
                }

                _logger.ProgressInfo("Scanning {0}", scenesImportFolder);

                var files = FilterFiles(scenesImportFolder, GetVideoFiles(scenesImportFolder).ToList());

                if (!files.Any())
                {
                    _logger.Warn("Scan folder {0} is empty.", scenesImportFolder);
                    continue;
                }

                mediaFileList.AddRange(files);
            }

            videoFilesStopwatch.Stop();

            _logger.Trace("Finished getting movie files for: {0} [{1}]", folders.ConcatToString("\n"), videoFilesStopwatch.Elapsed);

            var decisionsStopwatch = Stopwatch.StartNew();
            var decisions = _importDecisionMaker.GetSceneImportDecisions(mediaFileList);

            _importApprovedMovies.Import(decisions.Decisions, false);
            decisionsStopwatch.Stop();
            _logger.Trace("Import decisions complete [{0}]", decisionsStopwatch.Elapsed);

            CompletedScanning(Path.Combine(folders.FirstOrDefault(), "import"));

            var chunkSize = 10;

            foreach (var unmapped_list in decisions.UnmappedFiles.Chunk(chunkSize))
            {
                _mediaFileService.AddMany(unmapped_list.ToList());
            }

            foreach (var scene_list in decisions.MoviesToAdd.Chunk(chunkSize))
            {
                // Add the studios to prevent a race condition with the movie import
                var studios = scene_list.ToList().DistinctBy(m => m.MovieMetadata.Value.StudioForeignId);
                _addStudioService.AddStudios(studios.Select(m => new Studio
                {
                    ForeignId = m.MovieMetadata.Value.StudioForeignId,
                    QualityProfileId = m.QualityProfileId,
                    Title = m.MovieMetadata.Value.StudioTitle,
                    RootFolderPath = m.RootFolderPath,
                    Monitored = false,
                }).ToList());

                _commandQueueManager.PushMany(scene_list.Select(s => new AddMoviesCommand(new List<Movie> { s })).ToList());
            }

            foreach (var folder in folders)
            {
                var scenesImportFolder = GetImportFolder(folder);
                RemoveEmptyMovieFolder(scenesImportFolder);
            }
        }

        private string GetImportFolder(string folder)
        {
            var rootFolder = folder;
            var namingConfig = _namingConfigService.GetConfig();
            var pattern = namingConfig.SceneImportFolderFormat;
            var sceneImportFolderName = pattern.Split(new char[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries)[0];
            return Path.Combine(rootFolder, sceneImportFolderName);
        }

        private void CompletedScanning(string folder)
        {
            _logger.Info("Completed scanning disk for {0}", folder);
            _eventAggregator.PublishEvent(new RescanCompletedEvent());
        }

        public string[] GetVideoFiles(string path, bool allDirectories = true)
        {
            _logger.Debug("Scanning '{0}' for video files", path);

            var filesOnDisk = _diskProvider.GetFiles(path, allDirectories).ToList();

            var mediaFileList = filesOnDisk.Where(file => MediaFileExtensions.Extensions.Contains(Path.GetExtension(file)))
                                           .ToList();

            _logger.Trace("{0} files were found in {1}", filesOnDisk.Count, path);
            _logger.Debug("{0} video files were found in {1}", mediaFileList.Count, path);

            return mediaFileList.ToArray();
        }

        public string[] GetNonVideoFiles(string path, bool allDirectories = true)
        {
            _logger.Debug("Scanning '{0}' for non-video files", path);

            var filesOnDisk = _diskProvider.GetFiles(path, allDirectories).ToList();

            var mediaFileList = filesOnDisk.Where(file => !MediaFileExtensions.Extensions.Contains(Path.GetExtension(file)))
                                           .ToList();

            _logger.Trace("{0} files were found in {1}", filesOnDisk.Count, path);
            _logger.Debug("{0} non-video files were found in {1}", mediaFileList.Count, path);

            return mediaFileList.ToArray();
        }

        public List<string> FilterPaths(string basePath, IEnumerable<string> paths, bool filterExtras = true)
        {
            var filteredPaths =  paths.Where(path => !ExcludedSubFoldersRegex.IsMatch(basePath.GetRelativePath(path)))
                                      .Where(path => !ExcludedFilesRegex.IsMatch(Path.GetFileName(path)))
                                      .ToList();

            if (filterExtras)
            {
                filteredPaths = filteredPaths.Where(path => !ExcludedExtrasSubFolderRegex.IsMatch(basePath.GetRelativePath(path)))
                                             .Where(path => !ExcludedExtraFilesRegex.IsMatch(Path.GetFileName(path)))
                                             .ToList();
            }

            return filteredPaths;
        }

        public List<string> FilterFiles(string basePath, List<string> files)
        {
            return files.Where(file => !ExcludedSubFoldersRegex.IsMatch(basePath.GetRelativePath(file)))
                        .Where(file => !ExcludedFilesRegex.IsMatch(file))
                        .ToList();
        }

        private void RemoveEmptyMovieFolder(string path)
        {
            if (_configService.DeleteEmptyFolders)
            {
                _diskProvider.RemoveEmptySubfolders(path);
            }
        }

        public void Execute(RescanScenesCommand message)
        {
            Scan(message.Folders);

            _eventAggregator.PublishEvent(new RescanCompletedEvent());
        }
    }
}
