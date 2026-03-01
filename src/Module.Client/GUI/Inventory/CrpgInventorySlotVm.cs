using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Crpg.Module.GUI.Inventory;

internal class CrpgInventorySlotVm : ViewModel
{
    public EquipmentIndex Slot { get; }
    public Action<CrpgInventorySlotVm>? OnUnequipRequested { get; set; }

    public CrpgInventorySlotVm(EquipmentIndex slot)
    {
        Slot = slot;
    }

    [DataSourceProperty]
    public string SlotName => Slot.ToString();

    [DataSourceProperty]
    public int SlotIndex => (int)Slot;

    [DataSourceProperty]
    public CrpgInventoryItemVm? Item
    {
        get;
        set
        {
            field = value;
            IsEmpty = value == null;
            OnPropertyChangedWithValue(value);
        }
    }

    [DataSourceProperty]
    public bool IsEmpty
    {
        get;
        set
        {
            field = value;
            OnPropertyChangedWithValue(value);
        }
#pragma warning disable SA1500, SA1513
    } = true;
#pragma warning restore SA1513, SA1500

    [DataSourceProperty]
    public bool IsDropTarget
    {
        get;
        set
        {
            field = value;
            OnPropertyChangedWithValue(value);
        }
    }

    public void ExecuteUnequip()
    {
        OnUnequipRequested?.Invoke(this);
    }
}
