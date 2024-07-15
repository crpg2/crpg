using Crpg.Application.ActivityLogs.Models;
using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities.Notification;

namespace Crpg.Application.Notifications.Models;

public record UserNotificationViewModel : IMapFrom<UserNotification>
{
    public int Id { get; init; }
    public NotificationState State { get; init; }
    public NotificationType Type { get; init; }
    public ActivityLogViewModel? ActivityLog { get; init; }
    public DateTime CreatedAt { get; init; }
}
