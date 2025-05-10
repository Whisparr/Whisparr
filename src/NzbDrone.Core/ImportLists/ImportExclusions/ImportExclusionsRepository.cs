using System.Collections.Generic;
using System.Linq;
using Dapper;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.ImportLists.ImportExclusions
{
    public interface IImportExclusionsRepository : IBasicRepository<ImportExclusion>
    {
        bool IsExcluded(string foreignId, ImportExclusionType type);
        ImportExclusion GetByForeignId(string foreignId);
        List<ImportExclusion> AllByType(ImportExclusionType type);
        List<string> AllForeignIds();
        List<int> AllIds();
    }

    public class ImportExclusionsRepository : BasicRepository<ImportExclusion>, IImportExclusionsRepository
    {
        public ImportExclusionsRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public bool IsExcluded(string foreignId, ImportExclusionType type)
        {
            return Query(x => x.ForeignId == foreignId).Any(x => x.Type == type);
        }

        public ImportExclusion GetByForeignId(string foreignId)
        {
            return Query(x => x.ForeignId == foreignId).First();
        }

        public List<ImportExclusion> AllByType(ImportExclusionType type)
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
