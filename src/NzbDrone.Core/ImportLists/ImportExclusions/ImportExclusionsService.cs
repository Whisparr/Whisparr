using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Events;

namespace NzbDrone.Core.ImportLists.ImportExclusions
{
    public interface IImportExclusionsService
    {
        List<ImportExclusion> GetAllExclusions();
        List<ImportExclusion> GetAllByType(ImportExclusionType type);
        List<int> AllIds();
        List<string> AllForeignIds();
        bool IsExcluded(string foreignId, ImportExclusionType type);
        ImportExclusion AddExclusion(ImportExclusion exclusion);
        List<ImportExclusion> AddExclusions(List<ImportExclusion> exclusions);
        void RemoveExclusion(ImportExclusion exclusion);
        ImportExclusion GetById(int id);
        ImportExclusion GetByForeignId(string foreignId);
        List<ImportExclusion> GetByIds(List<int> ids);
        ImportExclusion Update(ImportExclusion exclusion);
    }

    public class ImportExclusionsService : IImportExclusionsService, IHandleAsync<MoviesDeletedEvent>
    {
        private readonly IImportExclusionsRepository _exclusionRepository;
        private readonly Logger _logger;

        public ImportExclusionsService(IImportExclusionsRepository exclusionRepository,
                             Logger logger)
        {
            _exclusionRepository = exclusionRepository;
            _logger = logger;
        }

        public ImportExclusion AddExclusion(ImportExclusion exclusion)
        {
            if (_exclusionRepository.IsExcluded(exclusion.ForeignId, exclusion.Type))
            {
                return _exclusionRepository.GetByForeignId(exclusion.ForeignId);
            }

            return _exclusionRepository.Insert(exclusion);
        }

        public List<ImportExclusion> AddExclusions(List<ImportExclusion> exclusions)
        {
            _exclusionRepository.InsertMany(DeDupeExclusions(exclusions));

            return exclusions;
        }

        public List<int> AllIds()
        {
            return _exclusionRepository.AllIds();
        }

        public List<string> AllForeignIds()
        {
            return _exclusionRepository.AllForeignIds();
        }

        public List<ImportExclusion> GetAllExclusions()
        {
            return _exclusionRepository.All().ToList();
        }

        public List<ImportExclusion> GetAllByType(ImportExclusionType type)
        {
            return _exclusionRepository.AllByType(type);
        }

        public bool IsExcluded(string foreignId, ImportExclusionType type)
        {
            return _exclusionRepository.IsExcluded(foreignId, type);
        }

        public void RemoveExclusion(ImportExclusion exclusion)
        {
            _exclusionRepository.Delete(exclusion);
        }

        public ImportExclusion GetById(int id)
        {
            return _exclusionRepository.Get(id);
        }

        public List<ImportExclusion> GetByIds(List<int> ids)
        {
            return _exclusionRepository.Get(ids).ToList();
        }

        public ImportExclusion GetByForeignId(string foreignId)
        {
            return _exclusionRepository.GetByForeignId(foreignId);
        }

        public ImportExclusion Update(ImportExclusion exclusion)
        {
            int.TryParse(exclusion.ForeignId, out var tmbdId);
            if (exclusion.Type == ImportExclusionType.Scene && tmbdId != 0)
            {
                exclusion.Type = ImportExclusionType.Movie;
            }

            return _exclusionRepository.Update(exclusion);
        }

        public void HandleAsync(MoviesDeletedEvent message)
        {
            if (message.AddExclusion)
            {
                _logger.Debug("Adding {0} Deleted Movies to Import Exclusions", message.Movies.Count);

                var exclusions = message.Movies.Select(m => new ImportExclusion { ForeignId = m.ForeignId, Type = ToImportExclusionType(m.MovieMetadata.Value.ItemType), MovieTitle = m.Title, MovieYear = m.Year }).ToList();
                _exclusionRepository.InsertMany(DeDupeExclusions(exclusions));
            }
        }

        private List<ImportExclusion> DeDupeExclusions(List<ImportExclusion> exclusions)
        {
            var existingExclusions = _exclusionRepository.AllForeignIds();

            return exclusions
                .DistinctBy(x => x.ForeignId)
                .Where(x => !existingExclusions.Contains(x.ForeignId))
                .ToList();
        }

        private ImportExclusionType ToImportExclusionType(ItemType itemType)
        {
            if (itemType == ItemType.Movie)
            {
                return ImportExclusionType.Movie;
            }

            return ImportExclusionType.Scene;
        }
    }
}
