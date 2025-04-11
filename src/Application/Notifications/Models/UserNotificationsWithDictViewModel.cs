namespace Crpg.Application.Notifications.Models;

public record UserNotificationsWithDictViewModel
{
    public IList<UserNotificationViewModel> Notifications { get; init; } = Array.Empty<UserNotificationViewModel>();
    public UserNotificationMetadataEntitiesDictViewModel Dict { get; init; } = new();
}
