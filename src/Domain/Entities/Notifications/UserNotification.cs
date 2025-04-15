using Crpg.Domain.Common;
using Crpg.Domain.Entities.Users;

namespace Crpg.Domain.Entities.Notifications;

public class UserNotification : AuditableEntity
{
    public int Id { get; set; }
    public NotificationType Type { get; set; }
    public NotificationState State { get; set; }
    public int UserId { get; set; }
    public List<UserNotificationMetadata> Metadata { get; set; } = new();

    public User? User { get; set; }
}
