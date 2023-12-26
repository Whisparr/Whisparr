using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Movies.Performers
{
    public interface IPerformerRepository : IBasicRepository<Performer>
    {
    }

    public class PerformerRepository : BasicRepository<Performer>, IPerformerRepository
    {
        public PerformerRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }
    }
}
