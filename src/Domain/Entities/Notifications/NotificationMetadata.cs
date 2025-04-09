namespace Crpg.Domain.Entities.Notifications;

public class NotificationMetadata
{
    public NotificationMetadata(string key, string value)
    {
        Key = key;
        Value = value;
    }

    public int NotificationId { get; set; }
    public string Key { get; set; }
    public string Value { get; set; }
}
