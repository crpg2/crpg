using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;

namespace Crpg.Module.GUI.Inventory;

/// <summary>
/// Root widget for the inventory screen. Coordinates drag-and-drop state
/// between <see cref="CrpgInventoryItemWidget"/> (draggable items) and
/// <see cref="CrpgInventorySlotWidget"/> (drop targets).
/// </summary>
internal class CrpgInventoryScreenWidget : Widget
{
    private Widget? _currentDraggedWidget;

    /// <summary>True when an <see cref="EditableTextWidget"/> inside the inventory has keyboard focus.</summary>
    public static bool IsTextInputFocused { get; private set; }

    /// <summary>Raised when an item is dropped on an equipment slot. Args: mbGuid, slotIndex.</summary>
    public static event Action<uint, int>? ItemDroppedOnSlot;

    /// <summary>Raised when an item is double-clicked for auto-equip. Arg: mbGuid.</summary>
    public static event Action<uint>? ItemDoubleClicked;

    public CrpgInventoryScreenWidget(UIContext context)
        : base(context)
    {
    }

    /// <summary>ItemTypeEnum int of the item currently being dragged (-1 if none).</summary>
    [Editor(false)]
    public int DraggedItemType { get; set; } = -1;

    /// <summary>MBGUID InternalValue of the currently dragged item (0 if none).</summary>
    public uint DraggedMbGuid { get; set; }

    public void OnItemDragBegin(Widget draggedWidget, int itemType, uint mbGuid)
    {
        _currentDraggedWidget = draggedWidget;
        DraggedItemType = itemType;
        DraggedMbGuid = mbGuid;
    }

    public void OnItemDrop()
    {
        _currentDraggedWidget = null;
        DraggedItemType = -1;
        DraggedMbGuid = 0;
    }

    public void HandleItemDropOnSlot(uint mbGuid, int slotIndex)
    {
        ItemDroppedOnSlot?.Invoke(mbGuid, slotIndex);
        OnItemDrop();
    }

    public static void RaiseItemDoubleClicked(uint mbGuid)
    {
        ItemDoubleClicked?.Invoke(mbGuid);
    }

    protected override void OnUpdate(float dt)
    {
        base.OnUpdate(dt);

        // Track whether a text input widget currently has keyboard focus.
        IsTextInputFocused = Context.EventManager.FocusedWidget is EditableTextWidget;

        // Reset drag state if the engine drag ended without a drop.
        if (_currentDraggedWidget != null && EventManager.DraggedWidget == null)
        {
            OnItemDrop();
        }
    }
}
