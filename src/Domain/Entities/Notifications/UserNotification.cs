using Crpg.Domain.Common;
using Crpg.Domain.Entities.ActivityLogs;
using Crpg.Domain.Entities.Users;

namespace Crpg.Domain.Entities.Notification;

public class UserNotification : AuditableEntity
{
    public int Id { get; set; }
    public NotificationType Type { get; set; }

    public NotificationState State { get; set; }
    public int UserId { get; set; }
    public int ActivityLogId { get; set; }

    public User? User { get; set; }
    public ActivityLog? ActivityLog { get; set; }
}
