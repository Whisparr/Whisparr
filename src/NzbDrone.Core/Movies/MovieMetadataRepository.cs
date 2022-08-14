using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Movies
{
    public interface IMovieMetadataRepository : IBasicRepository<MediaMetadata>
    {
        MediaMetadata FindByTmdbId(int tmdbId);
        List<MediaMetadata> FindById(List<int> tmdbIds);
        bool UpsertMany(List<MediaMetadata> data);
    }

    public class MovieMetadataRepository : BasicRepository<MediaMetadata>, IMovieMetadataRepository
    {
        private readonly Logger _logger;

        public MovieMetadataRepository(IMainDatabase database, IEventAggregator eventAggregator, Logger logger)
            : base(database, eventAggregator)
        {
            _logger = logger;
        }

        public MediaMetadata FindByTmdbId(int tmdbId)
        {
            return Query(x => x.ForiegnId == tmdbId).FirstOrDefault();
        }

        public List<MediaMetadata> FindById(List<int> tmdbIds)
        {
            return Query(x => Enumerable.Contains(tmdbIds, x.ForiegnId));
        }

        public bool UpsertMany(List<MediaMetadata> data)
        {
            var existingMetadata = FindById(data.Select(x => x.ForiegnId).ToList());
            var updateMetadataList = new List<MediaMetadata>();
            var addMetadataList = new List<MediaMetadata>();
            int upToDateMetadataCount = 0;

            foreach (var meta in data)
            {
                var existing = existingMetadata.SingleOrDefault(x => x.ForiegnId == meta.ForiegnId);
                if (existing != null)
                {
                    meta.UseDbFieldsFrom(existing);
                    if (!meta.Equals(existing))
                    {
                        updateMetadataList.Add(meta);
                    }
                    else
                    {
                        upToDateMetadataCount++;
                    }
                }
                else
                {
                    addMetadataList.Add(meta);
                }
            }

            UpdateMany(updateMetadataList);
            InsertMany(addMetadataList);

            _logger.Debug($"{upToDateMetadataCount} movie metadata up to date; Updating {updateMetadataList.Count}, Adding {addMetadataList.Count} movie metadata entries.");

            return updateMetadataList.Count > 0 || addMetadataList.Count > 0;
        }
    }
}
