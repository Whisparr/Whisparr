using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.Movies.Studios
{
    public interface IStudioService
    {
        Studio AddStudio(Studio studio);
        List<Studio> AddStudios(List<Studio> studios);
        Studio GetById(int id);
        List<Studio> GetAllStudios();
        void RemoveStudio(Studio studio);
    }

    public class StudioService : IStudioService
    {
        private readonly IStudioRepository _studioRepo;

        public StudioService(IStudioRepository studioRepo)
        {
            _studioRepo = studioRepo;
        }

        public Studio AddStudio(Studio studio)
        {
            return _studioRepo.Insert(studio);
        }

        public List<Studio> AddStudios(List<Studio> studios)
        {
            _studioRepo.InsertMany(studios);
            return studios;
        }

        public Studio GetById(int id)
        {
            return _studioRepo.Get(id);
        }

        public List<Studio> GetAllStudios()
        {
            return _studioRepo.All().ToList();
        }

        public void RemoveStudio(Studio studio)
        {
            _studioRepo.Delete(studio);
        }
    }
}
