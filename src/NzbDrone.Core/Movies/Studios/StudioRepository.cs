using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Movies.Studios
{
    public interface IStudioRepository : IBasicRepository<Studio>
    {
        Studio FindStudioByForeignId(string foreignId);
        Studio FindByTitle(string title);
    }

    public class StudioRepository : BasicRepository<Studio>, IStudioRepository
    {
        public StudioRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public Studio FindByTitle(string title)
        {
            return Query(x => x.CleanTitle == title).FirstOrDefault();
        }

        public Studio FindStudioByForeignId(string foreignId)
        {
            return Query(x => x.ForeignId == foreignId).FirstOrDefault();
        }
    }
}
