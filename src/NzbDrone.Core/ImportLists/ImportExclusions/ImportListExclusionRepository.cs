using System.Collections.Generic;
using System.Linq;
using Dapper;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.ImportLists.ImportExclusions
{
    public interface IImportListExclusionRepository : IBasicRepository<ImportListExclusion>
    {
        bool IsExcluded(string foreignId, ImportExclusionType type);
        ImportListExclusion GetByForeignId(string foreignId);
        List<ImportListExclusion> AllByType(ImportExclusionType type);
        List<string> AllForeignIds();
        List<int> AllIds();
    }

    public class ImportListListExclusionRepository : BasicRepository<ImportListExclusion>, IImportListExclusionRepository
    {
        public ImportListListExclusionRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public bool IsExcluded(string foreignId, ImportExclusionType type)
        {
            return Query(x => x.ForeignId == foreignId).Any(x => x.Type == type);
        }

        public ImportListExclusion GetByForeignId(string foreignId)
        {
            var exclusions = Query(x => x.ForeignId == foreignId).ToList();
            if (exclusions.Count == 0)
            {
                return null;
            }

            return exclusions.First();
        }

        public List<ImportListExclusion> AllByType(ImportExclusionType type)
        {
            return All().Where(x => x.Type == type).ToList();
        }

        public List<string> AllForeignIds()
        {
            using var conn = _database.OpenConnection();

            return conn.Query<string>("SELECT \"ForeignId\" FROM \"ImportExclusions\"").ToList();
        }

        public List<int> AllIds()
        {
            using (var conn = _database.OpenConnection())
            {
                return conn.Query<int>("SELECT \"Id\" FROM \"ImportExclusions\" ").ToList();
            }
        }
    }
}
