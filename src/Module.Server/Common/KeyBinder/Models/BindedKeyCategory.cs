namespace Crpg.Module.Common.KeyBinder.Models;

public class BindedKeyCategory
{
    public string CategoryId { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public ICollection<BindedKey> Keys { get; set; } = new List<BindedKey>();
}
