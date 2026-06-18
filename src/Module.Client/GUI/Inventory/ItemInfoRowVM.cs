using TaleWorlds.Library;

namespace Crpg.Module.GUI.Inventory;

public class ItemInfoRowVM : ViewModel
{
    [DataSourceProperty]
    public ItemInfoTupleVM? Left { get; set => SetField(ref field, value, nameof(Left)); }

    [DataSourceProperty]
    public ItemInfoTupleVM? Right { get; set => SetField(ref field, value, nameof(Right)); }

    public ItemInfoRowVM()
    {
    }

    public ItemInfoRowVM(ItemInfoTupleVM left, ItemInfoTupleVM right)
    {
        Left = left;
        Right = right;
    }
}
