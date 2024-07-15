using Crpg.Application.ActivityLogs.Models;

namespace Crpg.Application.Notifications.Models;

public record UserNotificationsWithDictViewModel
{
    public IList<UserNotificationViewModel> Notifications { get; init; } = Array.Empty<UserNotificationViewModel>();
    public ActivityLogMetadataEnrichedViewModel Dict { get; init; } = new();
}
