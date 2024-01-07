using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.Movies.Studios
{
    public interface IStudioService
    {
        Studio AddStudio(Studio studio);
        List<Studio> AddStudios(List<Studio> studios);
        Studio GetById(int id);
        List<Studio> GetAllStudios();
        Studio Update(Studio performer);
        Studio FindByTitle(string title);
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
            Ensure.That(studio, () => studio).IsNotNull();

            var existingStudio = _studioRepo.FindStudioByForeignId(studio.ForeignId);

            if (existingStudio != null)
            {
                return existingStudio;
            }

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

        public Studio Update(Studio studio)
        {
            return _studioRepo.Update(studio);
        }

        public void RemoveStudio(Studio studio)
        {
            _studioRepo.Delete(studio);
        }

        public Studio FindByTitle(string title)
        {
            var cleanTitle = title.CleanMovieTitle();

            return _studioRepo.FindByTitle(cleanTitle);
        }
    }
}
