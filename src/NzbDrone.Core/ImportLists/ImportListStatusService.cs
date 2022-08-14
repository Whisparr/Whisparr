using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;
using NzbDrone.Core.ThingiProvider.Status;

namespace NzbDrone.Core.ImportLists
{
    public interface IImportListStatusService : IProviderStatusServiceBase<ImportListStatus>
    {
        Media GetLastSyncListInfo(int importListId);

        void UpdateListSyncStatus(int importListId, Media listItemInfo);
    }

    public class ImportListStatusService : ProviderStatusServiceBase<IImportList, ImportListStatus>, IImportListStatusService
    {
        public ImportListStatusService(IImportListStatusRepository providerStatusRepository, IEventAggregator eventAggregator, IRuntimeInfo runtimeInfo, Logger logger)
            : base(providerStatusRepository, eventAggregator, runtimeInfo, logger)
        {
        }

        public Media GetLastSyncListInfo(int importListId)
        {
            return GetProviderStatus(importListId).LastSyncListInfo;
        }

        public void UpdateListSyncStatus(int importListId, Media listItemInfo)
        {
            lock (_syncRoot)
            {
                var status = GetProviderStatus(importListId);

                status.LastSyncListInfo = listItemInfo;

                _providerStatusRepository.Upsert(status);
            }
        }
    }
}
