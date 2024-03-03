using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Events;

namespace NzbDrone.Core.ImportLists.ImportExclusions
{
    public interface IImportListExclusionService
    {
        List<ImportListExclusion> GetAllExclusions();
        List<ImportListExclusion> GetAllByType(ImportExclusionType type);
        List<int> AllIds();
        List<string> AllForeignIds();
        bool IsExcluded(string foreignId, ImportExclusionType type);
        ImportListExclusion AddExclusion(ImportListExclusion exclusion);
        List<ImportListExclusion> AddExclusions(List<ImportListExclusion> exclusions);
        PagingSpec<ImportListExclusion> Paged(PagingSpec<ImportListExclusion> pagingSpec);
        void RemoveExclusion(ImportListExclusion exclusion);
        ImportListExclusion GetById(int id);
        ImportListExclusion GetByForeignId(string foreignId);
        List<ImportListExclusion> GetByIds(List<int> ids);
        ImportListExclusion Update(ImportListExclusion exclusion);
    }

    public class ImportListExclusionService : IImportListExclusionService, IHandleAsync<MoviesDeletedEvent>
    {
        private readonly IImportListExclusionRepository _exclusionRepository;
        private readonly Logger _logger;

        public ImportListExclusionService(IImportListExclusionRepository exclusionRepository,
                             Logger logger)
        {
            _exclusionRepository = exclusionRepository;
            _logger = logger;
        }

        public ImportListExclusion AddExclusion(ImportListExclusion exclusion)
        {
            if (_exclusionRepository.IsExcluded(exclusion.ForeignId, exclusion.Type))
            {
                return _exclusionRepository.GetByForeignId(exclusion.ForeignId);
            }

            return _exclusionRepository.Insert(exclusion);
        }

        public List<ImportListExclusion> AddExclusions(List<ImportListExclusion> exclusions)
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

        public List<ImportListExclusion> GetAllExclusions()
        {
            return _exclusionRepository.All().ToList();
        }

        public List<ImportListExclusion> GetAllByType(ImportExclusionType type)
        {
            return _exclusionRepository.AllByType(type) ?? new List<ImportListExclusion>();
        }

        public bool IsExcluded(string foreignId, ImportExclusionType type)
        {
            return _exclusionRepository.IsExcluded(foreignId, type);
        }

        public void RemoveExclusion(ImportListExclusion exclusion)
        {
            _exclusionRepository.Delete(exclusion);
        }

        public ImportListExclusion GetById(int id)
        {
            return _exclusionRepository.Get(id);
        }

        public List<ImportListExclusion> GetByIds(List<int> ids)
        {
            return _exclusionRepository.Get(ids).ToList();
        }

        public ImportListExclusion GetByForeignId(string foreignId)
        {
            return _exclusionRepository.GetByForeignId(foreignId);
        }

        public ImportListExclusion Update(ImportListExclusion exclusion)
        {
            int.TryParse(exclusion.ForeignId, out var tmbdId);
            if (exclusion.Type == ImportExclusionType.Scene && tmbdId != 0)
            {
                exclusion.Type = ImportExclusionType.Movie;
            }

            return _exclusionRepository.Update(exclusion);
        }

        public PagingSpec<ImportListExclusion> Paged(PagingSpec<ImportListExclusion> pagingSpec)
        {
            return _exclusionRepository.GetPaged(pagingSpec);
        }

        public void HandleAsync(MoviesDeletedEvent message)
        {
            if (message.AddImportListExclusion)
            {
                _logger.Debug("Adding {0} Deleted Movies to Import Exclusions", message.Movies.Count);

                var exclusions = message.Movies.Select(m => new ImportListExclusion { ForeignId = m.ForeignId, Type = ToImportExclusionType(m.MovieMetadata.Value.ItemType), MovieTitle = m.Title, MovieYear = m.Year }).ToList();
                _exclusionRepository.InsertMany(DeDupeExclusions(exclusions));
            }
        }

        private List<ImportListExclusion> DeDupeExclusions(List<ImportListExclusion> exclusions)
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
