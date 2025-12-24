using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;

namespace Crpg.Module.GUI.Inventory;

public class ItemInfoIconVM : ViewModel
{
    public ItemInfoIconVM()
    {
        ItemHint = new BasicTooltipViewModel(() => HintText);
    }

    [DataSourceProperty]
    public string IconSprite { get; set => SetField(ref field, value, nameof(IconSprite)); } = string.Empty;

    [DataSourceProperty]
    public bool IsIconVisible => !string.IsNullOrEmpty(IconSprite);

    [DataSourceProperty]
    public string HintText { get; set => SetField(ref field, value, nameof(HintText)); } = string.Empty;

    [DataSourceProperty]
    public BasicTooltipViewModel? ItemHint { get; set => SetField(ref field, value, nameof(ItemHint)); }
}
