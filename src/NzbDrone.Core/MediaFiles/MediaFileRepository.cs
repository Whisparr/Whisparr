using System.Collections.Generic;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.MediaFiles
{
    public interface IMediaFileRepository : IBasicRepository<MediaFile>
    {
        List<MediaFile> GetFilesByMovie(int movieId);
        List<MediaFile> GetFilesWithoutMediaInfo();
        void DeleteForMovies(List<int> movieIds);

        List<MediaFile> GetFilesWithRelativePath(int movieId, string relativePath);
    }

    public class MediaFileRepository : BasicRepository<MediaFile>, IMediaFileRepository
    {
        public MediaFileRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public List<MediaFile> GetFilesByMovie(int movieId)
        {
            return Query(x => x.MovieId == movieId);
        }

        public List<MediaFile> GetFilesWithoutMediaInfo()
        {
            return Query(x => x.MediaInfo == null);
        }

        public void DeleteForMovies(List<int> movieIds)
        {
            Delete(x => movieIds.Contains(x.MovieId));
        }

        public List<MediaFile> GetFilesWithRelativePath(int movieId, string relativePath)
        {
            return Query(c => c.MovieId == movieId && c.RelativePath == relativePath);
        }
    }
}
