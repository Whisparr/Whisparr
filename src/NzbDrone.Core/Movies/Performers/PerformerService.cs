using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.Movies.Performers
{
    public interface IPerformerService
    {
        Performer AddPerformer(Performer performer);
        List<Performer> AddPerformers(List<Performer> performers);
        Performer GetById(int id);
        List<Performer> GetAllPerformers();
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
            _performerRepo.InsertMany(performers);
            return performers;
        }

        public Performer GetById(int id)
        {
            return _performerRepo.Get(id);
        }

        public List<Performer> GetAllPerformers()
        {
            return _performerRepo.All().ToList();
        }

        public void RemovePerformer(Performer performer)
        {
            _performerRepo.Delete(performer);
        }
    }
}
