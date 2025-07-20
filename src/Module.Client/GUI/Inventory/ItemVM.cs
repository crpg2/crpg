using TaleWorlds.Core;
using TaleWorlds.Library;

public class ItemVM : ViewModel
{
    public string Name { get; }
    public ItemObject.ItemTypeEnum Type { get; }

    public ImageIdentifierVM Image { get; }

    public ItemVM(ItemObject item)
    {
        Name = item?.Name?.ToString() ?? "";
        Type = item?.ItemType ?? ItemObject.ItemTypeEnum.Invalid;
        Image = new ImageIdentifierVM(item); // ✅ Just pass the ItemObject directly
    }
}
