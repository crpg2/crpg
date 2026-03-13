namespace Crpg.Domain.Entities.Items;

public class UserItemPresetSlot
{
    public int UserItemPresetId { get; set; }
    public ItemSlot Slot { get; set; }
    public string? ItemId { get; set; }

    public UserItemPreset? UserItemPreset { get; set; }
    public Item? Item { get; set; }
}
