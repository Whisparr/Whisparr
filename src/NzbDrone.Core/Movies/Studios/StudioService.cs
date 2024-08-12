using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies.Studios.Events;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.Movies.Studios
{
    public interface IStudioService
    {
        Studio AddStudio(Studio studio);
        List<Studio> AddStudios(List<Studio> studios);
        List<Studio> GetStudios(IEnumerable<int> studioIds);
        Studio GetById(int id);
        Studio FindByForeignId(string foreignId);
        List<Studio> GetAllStudios();
        List<string> AllStudioForeignIds();
        Studio Update(Studio performer);
        List<Studio> Update(List<Studio> studios);
        Studio FindByTitle(string title);
        List<Studio> FindAllByTitle(string title);
        void RemoveStudio(Studio studio);
    }

    public class StudioService : IStudioService
    {
        private readonly IStudioRepository _studioRepo;
        private readonly IEventAggregator _eventAggregator;

        public StudioService(IStudioRepository studioRepo, IEventAggregator eventAggregator)
        {
            _studioRepo = studioRepo;
            _eventAggregator = eventAggregator;
        }

        public Studio AddStudio(Studio newStudio)
        {
            var studio = _studioRepo.Insert(newStudio);

            _eventAggregator.PublishEvent(new StudioAddedEvent(GetById(studio.Id)));

            return studio;
        }

        public List<Studio> AddStudios(List<Studio> studios)
        {
            _studioRepo.InsertMany(studios);

            _eventAggregator.PublishEvent(new StudiosAddedEvent(studios));

            return studios;
        }

        public Studio GetById(int id)
        {
            return _studioRepo.Get(id);
        }

        public List<Studio> GetStudios(IEnumerable<int> studioIds)
        {
            return _studioRepo.Get(studioIds).ToList();
        }

        public List<Studio> GetAllStudios()
        {
            return _studioRepo.All().ToList();
        }

        public Studio Update(Studio studio)
        {
            return _studioRepo.Update(studio);
        }

        public List<Studio> Update(List<Studio> studios)
        {
            _studioRepo.UpdateMany(studios);

            return studios;
        }

        public void RemoveStudio(Studio studio)
        {
            _studioRepo.Delete(studio);
        }

        public Studio FindByTitle(string title)
        {
            var cleanTitle = title.CleanStudioTitle();

            return _studioRepo.FindByTitle(cleanTitle);
        }

        public List<Studio> FindAllByTitle(string title)
        {
            var cleanTitle = title.CleanStudioTitle().ToLower();

            return _studioRepo.FindAllByTitle(cleanTitle);
        }

        public Studio FindByForeignId(string foreignId)
        {
            return _studioRepo.FindByForeignId(foreignId);
        }

        public List<string> AllStudioForeignIds()
        {
            return _studioRepo.AllStudioForeignIds();
        }
    }
}
