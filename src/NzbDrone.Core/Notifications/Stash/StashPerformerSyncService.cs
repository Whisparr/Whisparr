using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Notifications.Stash
{
    public class StashPerformerSyncService : IExecute<SyncStashPerformersCommand>
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IStashService _stashService;
        private readonly Logger _logger;

        public StashPerformerSyncService(INotificationRepository notificationRepository, IStashService stashService, Logger logger)
        {
            _notificationRepository = notificationRepository;
            _stashService = stashService;
            _logger = logger;
        }

        public void Execute(SyncStashPerformersCommand message)
        {
            var definitions = _notificationRepository.All()
                .Where(definition => definition.Implementation == nameof(Stash))
                .Where(definition => definition.Settings is StashSettings settings && settings.PerformerSyncMode != StashPerformerSyncMode.Disabled)
                .ToList();
            var failures = new List<Exception>();

            foreach (var definition in definitions)
            {
                var settings = (StashSettings)definition.Settings;

                try
                {
                    _stashService.SyncPerformers(settings, () => _notificationRepository.UpdateSettings(definition));
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Unable to synchronize performers with Stash connection {0}", definition.Name);
                    failures.Add(ex);
                }
            }

            if (failures.Any())
            {
                throw new AggregateException("One or more Stash performer syncs failed", failures);
            }
        }
    }
}
