using System.Collections.Generic;
using System.Linq;
using Dapper;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.ImportLists.ImportExclusions
{
    public interface IImportExclusionsRepository : IBasicRepository<ImportExclusion>
    {
        bool IsMovieExcluded(string tmdbid);
        ImportExclusion GetByTmdbid(string tmdbid);
        List<string> AllExcludedTmdbIds();
    }

    public class ImportExclusionsRepository : BasicRepository<ImportExclusion>, IImportExclusionsRepository
    {
        public ImportExclusionsRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public bool IsMovieExcluded(string tmdbid)
        {
            return Query(x => x.ForeignId == tmdbid).Any();
        }

        public ImportExclusion GetByTmdbid(string tmdbid)
        {
            return Query(x => x.ForeignId == tmdbid).First();
        }

        public List<string> AllExcludedTmdbIds()
        {
            using var conn = _database.OpenConnection();

            return conn.Query<string>("SELECT \"ForeignId\" FROM \"ImportExclusions\"").ToList();
        }
    }
}
