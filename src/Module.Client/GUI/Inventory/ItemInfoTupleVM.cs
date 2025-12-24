using TaleWorlds.Library;

namespace Crpg.Module.GUI.Inventory;

public class ItemInfoTupleVM : ViewModel
{
    [DataSourceProperty]
    public string CategoryName { get; set => SetField(ref field, value, nameof(CategoryName)); } = string.Empty;

    [DataSourceProperty]
    public string ValueText { get; set => SetField(ref field, value, nameof(ValueText)); } = string.Empty;

    [DataSourceProperty]
    public bool IsGoldVisible { get; set => SetField(ref field, value, nameof(IsGoldVisible)); }

    [DataSourceProperty]
    public MBBindingList<ItemInfoIconVM> Icons { get; } = [];
}
