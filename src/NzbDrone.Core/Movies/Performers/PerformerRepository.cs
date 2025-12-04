using System.Collections.Generic;
using System.Linq;
using Dapper;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Movies.Performers
{
    public interface IPerformerRepository : IBasicRepository<Performer>
    {
        Performer FindByForeignId(string foreignId);
        List<Performer> FindByForeignIds(List<string> foreignIds);
        List<Performer> SearchPerformers(string cleanName, string foreignId);
        List<string> AllPerformerForeignIds();
    }

    public class PerformerRepository : BasicRepository<Performer>, IPerformerRepository
    {
        public PerformerRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public Performer FindByForeignId(string foreignId)
        {
            return Query(x => x.ForeignId == foreignId).FirstOrDefault();
        }

        public List<Performer> FindByForeignIds(List<string> foreignIds)
        {
            return Query(x => foreignIds.Contains(x.ForeignId)).ToList();
        }

        public List<Performer> SearchPerformers(string cleanName, string foreignId)
        {
            return Query(x => x.CleanName.Contains(cleanName) || x.ForeignId == foreignId).ToList();
        }

        public List<string> AllPerformerForeignIds()
        {
            using (var conn = _database.OpenConnection())
            {
                return conn.Query<string>("SELECT \"ForeignId\" FROM \"Performers\"").ToList();
            }
        }
    }
}
