namespace Crpg.Domain.Entities.ActivityLogs;

public class ActivityLogMetadata(string key, string value)
{
    public int ActivityLogId { get; set; }
    public string Key { get; set; } = key;
    public string Value { get; set; } = value;
}
