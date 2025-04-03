using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.MediaFiles.Commands;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.MediaFiles
{
    public interface IMediaFileTableCleanupService
    {
        void Clean(Movie movie, List<string> filesOnDisk);
        void Clean(string folder, List<string> filesOnDisk);
    }

    public class MediaFileTableCleanupService :
        IMediaFileTableCleanupService,
        IExecute<CleanUnmappedFilesCommand>
    {
        private readonly IMediaFileService _mediaFileService;
        private readonly IMovieService _movieService;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public MediaFileTableCleanupService(IMediaFileService mediaFileService,
                                            IMovieService movieService,
                                            IEventAggregator eventAggregator,
                                            Logger logger)
        {
            _mediaFileService = mediaFileService;
            _movieService = movieService;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public void Clean(Movie movie, List<string> filesOnDisk)
        {
            var movieFiles = _mediaFileService.GetFilesByMovie(movie.Id);

            var filesOnDiskKeys = new HashSet<string>(filesOnDisk, PathEqualityComparer.Instance);

            foreach (var movieFile in movieFiles)
            {
                var movieFilePath = Path.Combine(movie.Path, movieFile.RelativePath);

                try
                {
                    if (!filesOnDiskKeys.Contains(movieFilePath))
                    {
                        _logger.Debug("File [{0}] no longer exists on disk, removing from db", movieFilePath);
                        _mediaFileService.Delete(movieFile, DeleteMediaFileReason.MissingFromDisk);
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    var errorMessage = string.Format("Unable to cleanup MovieFile in DB: {0}", movieFile.Id);
                    _logger.Error(ex, errorMessage);
                }
            }
        }

        public void Clean(string folder, List<string> filesOnDisk)
        {
            var dbFiles = _mediaFileService.GetFilesWithBasePath(folder);

            // get files in database that are missing on disk and remove from database
            var missingFiles = dbFiles.ExceptBy(x => x.OriginalFilePath, filesOnDisk, x => x, PathEqualityComparer.Instance).ToList();

            _logger.Debug("The following files no longer exist on disk, removing from db:\n{0}",
                          string.Join("\n", missingFiles.Select(x => x.OriginalFilePath)));

            _mediaFileService.DeleteMany(missingFiles, DeleteMediaFileReason.MissingFromDisk);

            // get any movies matched to these moviefiles and unlink them
            var orphanedMovies = _movieService.GetMoviesByFileId(missingFiles.Select(x => x.Id));
            orphanedMovies.ForEach(x => x.MovieFileId = 0);
            _movieService.SetFileIds(orphanedMovies);
        }

        public void Execute(CleanUnmappedFilesCommand message)
        {
            var unmappedFiles = _mediaFileService.GetUnmappedFiles();
            _mediaFileService.DeleteMany(unmappedFiles, DeleteMediaFileReason.Manual);
            _eventAggregator.PublishEvent(new CleanCompletedEvent());
        }
    }
}
