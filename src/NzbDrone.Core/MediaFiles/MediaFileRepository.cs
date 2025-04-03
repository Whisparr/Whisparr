using System.Collections.Generic;
using System.IO;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.MediaFiles
{
    public interface IMediaFileRepository : IBasicRepository<MovieFile>
    {
        List<MovieFile> GetFilesByMovie(int movieId);
        List<MovieFile> GetFilesByMovies(IEnumerable<int> movieIds);
        List<MovieFile> GetFilesWithoutMediaInfo();
        List<MovieFile> GetAllFiles();
        List<MovieFile> GetUnmappedFiles();
        void DeleteForMovies(List<int> movieIds);
        List<MovieFile> GetFilesWithBasePath(string path);
        List<MovieFile> GetFilesWithRelativePath(int movieId, string relativePath);
    }

    public class MediaFileRepository : BasicRepository<MovieFile>, IMediaFileRepository
    {
        public MediaFileRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public List<MovieFile> GetFilesByMovie(int movieId)
        {
            return Query(x => x.MovieId == movieId);
        }

        public List<MovieFile> GetFilesByMovies(IEnumerable<int> movieIds)
        {
            return Query(x => movieIds.Contains(x.MovieId));
        }

        public List<MovieFile> GetFilesWithoutMediaInfo()
        {
            return Query(x => x.MediaInfo == null);
        }

        public List<MovieFile> GetAllFiles()
        {
            return Query(x => x.Size > 0);
        }

        public List<MovieFile> GetUnmappedFiles()
        {
            return Query(x => x.MovieId == 0);
        }

        public void DeleteForMovies(List<int> movieIds)
        {
            Delete(x => movieIds.Contains(x.MovieId));
        }

        public List<MovieFile> GetFilesWithBasePath(string path)
        {
            // ensure path ends with a single trailing path separator to avoid matching partial paths
            var safePath = path.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            return _database.Query<MovieFile>(new SqlBuilder(_database.DatabaseType).Where<MovieFile>(x => x.OriginalFilePath.StartsWith(safePath))).ToList();
        }

        public List<MovieFile> GetFilesWithRelativePath(int movieId, string relativePath)
        {
            return Query(c => c.MovieId == movieId && c.RelativePath == relativePath);
        }
    }
}
