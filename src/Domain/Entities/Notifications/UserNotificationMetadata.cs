namespace Crpg.Domain.Entities.Notifications;

public class UserNotificationMetadata
{
    public UserNotificationMetadata(string key, string value)
    {
        Key = key;
        Value = value;
    }

    public int UserNotificationId { get; set; }
    public string Key { get; set; }
    public string Value { get; set; }
}
