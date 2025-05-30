namespace Crpg.Domain.Entities.Notifications;

/// <summary>
/// Represents state of a <see cref="Notifications"/>.
/// </summary>
public enum NotificationState
{
    /// <summary>
    /// Notification is not read by user yet.
    /// </summary>
    Unread = 0,

    /// <summary>
    /// Notification is read by user.
    /// </summary>
    Read,
}
