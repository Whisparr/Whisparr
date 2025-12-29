using System;
using System.IO;
using NLog;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.History
{
    public class MovieFileScannedImportHistoryHandler : IHandle<MovieFileImportedEvent>
    {
        private readonly IHistoryRepository _historyRepository;
        private readonly Logger _logger;

        public MovieFileScannedImportHistoryHandler(IHistoryRepository historyRepository, Logger logger)
        {
            _historyRepository = historyRepository;
            _logger = logger;
        }

        public void Handle(MovieFileImportedEvent message)
        {
            // Only record history for imports that are NOT new downloads (i.e. picked up by disk scan)
            if (message.NewDownload)
            {
                return;
            }

            var movie = message.MovieInfo.Movie;

            var history = new MovieHistory
            {
                EventType = MovieHistoryEventType.DiskScanImported,
                Date = DateTime.UtcNow,
                Quality = message.MovieInfo.Quality,
                Languages = message.MovieInfo.Languages,
                SourceTitle = message.ImportedMovie.Path ?? message.ImportedMovie.RelativePath,
                MovieId = movie.Id
            };

            history.Data.Add("FileId", message.ImportedMovie.Id.ToString());
            history.Data.Add("DroppedPath", message.MovieInfo.Path);
            history.Data.Add("ImportedPath", Path.Combine(movie.Path, message.ImportedMovie.RelativePath));
            history.Data.Add("ReleaseGroup", message.ImportedMovie.ReleaseGroup ?? message.MovieInfo.ReleaseGroup);
            history.Data.Add("CustomFormatScore", message.MovieInfo.CustomFormatScore.ToString());
            history.Data.Add("Size", (message.ImportedMovie?.Size ?? message.MovieInfo.Size).ToString());
            history.Data.Add("IndexerFlags", message.ImportedMovie.IndexerFlags.ToString());

            try
            {
                _historyRepository.Insert(history);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to insert history for scanned import of {0}", movie.Title);
            }
        }
    }
}
