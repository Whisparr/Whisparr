using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.MetadataSource.SkyHook.Resource;
using NzbDrone.Core.Movies.Performers.Events;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.Movies.Performers
{
    public interface IPerformerService
    {
        Performer AddPerformer(Performer performer);
        List<Performer> AddPerformers(List<Performer> performers);
        List<Performer> GetPerformers(IEnumerable<int> performerIds);
        Performer GetById(int id);
        Performer FindByForeignId(string foreignId);
        List<Performer> FindByForeignIds(List<string> foreignIds);
        List<Performer> SearchPerformers(string query);
        List<Performer> GetAllPerformers();
        List<string> AllPerformerForeignIds();
        Performer Update(Performer performer);
        List<Performer> Update(List<Performer> performers);
        void RemovePerformer(Performer performer);
    }

    public class PerformerService : IPerformerService
    {
        private readonly IPerformerRepository _performerRepo;
        private readonly ICached<PerformerResource> _performerResourceCache;
        private readonly IEventAggregator _eventAggregator;

        public PerformerService(IPerformerRepository performerRepo, ICacheManager cacheManager, IEventAggregator eventAggregator)
        {
            _performerRepo = performerRepo;
            _eventAggregator = eventAggregator;
            _performerResourceCache = cacheManager.GetCache<PerformerResource>(typeof(PerformerResource), "performerResources");
        }

        public Performer AddPerformer(Performer newPerformer)
        {
            var performer = _performerRepo.Insert(newPerformer);

            _eventAggregator.PublishEvent(new PerformerAddedEvent(GetById(performer.Id)));

            return performer;
        }

        public List<Performer> AddPerformers(List<Performer> performers)
        {
            var allPerformers = _performerRepo.All();

            performers = performers.Where(p => p.ForeignId.IsNotNullOrWhiteSpace()).ToList();

            var existing = allPerformers.Where(x => performers.Any(a => a.ForeignId == x.ForeignId));
            var performersToAdd = performers.Where(x => !allPerformers.Any(a => a.ForeignId == x.ForeignId)).ToList();

            _performerRepo.InsertMany(performersToAdd);

            _eventAggregator.PublishEvent(new PerformersAddedEvent(performersToAdd));

            return performersToAdd.Concat(existing).ToList();
        }

        public Performer GetById(int id)
        {
            return _performerRepo.Get(id);
        }

        public Performer FindByForeignId(string foreignId)
        {
            return _performerRepo.FindByForeignId(foreignId);
        }

        public List<Performer> FindByForeignIds(List<string> foreignIds)
        {
            return _performerRepo.FindByForeignIds(foreignIds);
        }

        public List<Performer> SearchPerformers(string query)
        {
            var cleanName = query.CleanMovieTitle();

            return _performerRepo.SearchPerformers(cleanName, query);
        }

        public List<Performer> GetPerformers(IEnumerable<int> performerIds)
        {
            return _performerRepo.Get(performerIds).ToList();
        }

        public List<Performer> GetAllPerformers()
        {
            return _performerRepo.All().ToList();
        }

        public Performer Update(Performer performer)
        {
            var newPerformer = _performerRepo.Update(performer);
            _performerResourceCache.Remove(newPerformer.ForeignId);
            return newPerformer;
        }

        public List<Performer> Update(List<Performer> performers)
        {
            _performerRepo.UpdateMany(performers);

            foreach (var performer in performers)
            {
                _performerResourceCache.Remove(performer.ForeignId);
            }

            return performers;
        }

        public void RemovePerformer(Performer performer)
        {
            _performerRepo.Delete(performer);
            _performerResourceCache.Remove(performer.ForeignId);
        }

        public List<string> AllPerformerForeignIds()
        {
            return _performerRepo.AllPerformerForeignIds();
        }
    }
}
