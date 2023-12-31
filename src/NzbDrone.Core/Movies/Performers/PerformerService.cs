using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Movies.Performers
{
    public interface IPerformerService
    {
        Performer AddPerformer(Performer performer);
        List<Performer> AddPerformers(List<Performer> performers);
        Performer GetById(int id);
        List<Performer> GetAllPerformers();
        Performer Update(Performer performer);
        void RemovePerformer(Performer performer);
    }

    public class PerformerService : IPerformerService
    {
        private readonly IPerformerRepository _performerRepo;

        public PerformerService(IPerformerRepository performerRepo)
        {
            _performerRepo = performerRepo;
        }

        public Performer AddPerformer(Performer performer)
        {
            return _performerRepo.Insert(performer);
        }

        public List<Performer> AddPerformers(List<Performer> performers)
        {
            // TODO: Use a foreignId only pull
            var allPerformers = _performerRepo.All();

            performers = performers.Where(p => p.ForeignId.IsNotNullOrWhiteSpace()).ToList();

            var existing = allPerformers.Where(x => performers.Any(a => a.ForeignId == x.ForeignId));
            var performersToAdd = performers.Where(x => !allPerformers.Any(a => a.ForeignId == x.ForeignId)).ToList();

            _performerRepo.InsertMany(performersToAdd);

            return performersToAdd.Concat(existing).ToList();
        }

        public Performer GetById(int id)
        {
            return _performerRepo.Get(id);
        }

        public List<Performer> GetAllPerformers()
        {
            return _performerRepo.All().ToList();
        }

        public Performer Update(Performer performer)
        {
            return _performerRepo.Update(performer);
        }

        public void RemovePerformer(Performer performer)
        {
            _performerRepo.Delete(performer);
        }
    }
}
