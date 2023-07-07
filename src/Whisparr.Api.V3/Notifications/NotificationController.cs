using NzbDrone.Core.Notifications;
using Whisparr.Http;

namespace Whisparr.Api.V3.Notifications
{
    [V3ApiController]
    public class NotificationController : ProviderControllerBase<NotificationResource, NotificationBulkResource, INotification, NotificationDefinition>
    {
        public static readonly NotificationResourceMapper ResourceMapper = new NotificationResourceMapper();
        public static readonly NotificationBulkResourceMapper BulkResourceMapper = new NotificationBulkResourceMapper();

        public NotificationController(NotificationFactory notificationFactory)
            : base(notificationFactory, "notification", ResourceMapper, BulkResourceMapper)
        {
        }
    }
}
