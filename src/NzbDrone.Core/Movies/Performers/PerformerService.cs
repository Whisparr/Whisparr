using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies.Performers.Events;

namespace NzbDrone.Core.Movies.Performers
{
    public interface IPerformerService
    {
        Performer AddPerformer(Performer performer);
        List<Performer> AddPerformers(List<Performer> performers);
        List<Performer> GetPerformers(IEnumerable<int> performerIds);
        Performer GetById(int id);
        Performer FindByForeignId(string foreignId);
        List<Performer> GetAllPerformers();
        List<string> AllPerformerForeignIds();
        Performer Update(Performer performer);
        List<Performer> Update(List<Performer> performers);
        void RemovePerformer(Performer performer);
    }

    public class PerformerService : IPerformerService
    {
        private readonly IPerformerRepository _performerRepo;
        private readonly IEventAggregator _eventAggregator;

        public PerformerService(IPerformerRepository performerRepo, IEventAggregator eventAggregator)
        {
            _performerRepo = performerRepo;
            _eventAggregator = eventAggregator;
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
            return _performerRepo.Update(performer);
        }

        public List<Performer> Update(List<Performer> performers)
        {
            _performerRepo.UpdateMany(performers);

            return performers;
        }

        public void RemovePerformer(Performer performer)
        {
            _performerRepo.Delete(performer);
        }

        public List<string> AllPerformerForeignIds()
        {
            return _performerRepo.AllPerformerForeignIds();
        }
    }
}
