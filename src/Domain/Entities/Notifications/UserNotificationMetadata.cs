namespace Crpg.Domain.Entities.Notifications;

public class UserNotificationMetadata(string key, string value)
{
    public int UserNotificationId { get; set; }
    public string Key { get; set; } = key;
    public string Value { get; set; } = value;
}
