using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;

namespace Crpg.Module.GUI.Inventory;

/// <summary>
/// Draggable widget representing an item in the inventory grid.
/// Supports double-click to auto-equip and drag to equip in a specific slot.
/// </summary>
internal class CrpgInventoryItemWidget : ButtonWidget
{
    private const float DoubleClickTimeMs = 400f;
    private float _lastClickTimeInventory;

    public CrpgInventoryItemWidget(UIContext context)
        : base(context)
    {
        AcceptDrag = true;
        DropEventHandledManually = true;
    }

    /// <summary>Bound to the item's <see cref="CrpgInventoryItemVm.ItemTypeInt"/>.</summary>
    [Editor(false)]
    public int ItemTypeInt { get; set; } = -1;

    /// <summary>Bound to the item's <see cref="CrpgInventoryItemVm.MbGuid"/> (string representation of uint).</summary>
    [Editor(false)]
    public string MbGuid { get; set; } = "0";

    protected override void OnMouseReleased()
    {
        base.OnMouseReleased();

        float now = Context.EventManager.Time;
        if (now - _lastClickTimeInventory < DoubleClickTimeMs)
        {
            if (uint.TryParse(MbGuid, out uint guid))
            {
                CrpgInventoryScreenWidget.RaiseItemDoubleClicked(guid);
            }

            _lastClickTimeInventory = 0f;
        }
        else
        {
            _lastClickTimeInventory = now;
        }
    }

    protected override void OnDragBegin()
    {
        base.OnDragBegin();
        var screenWidget = FindScreenWidget();
        if (screenWidget != null && uint.TryParse(MbGuid, out uint guid))
        {
            screenWidget.OnItemDragBegin(this, ItemTypeInt, guid);
        }
    }

    protected override void OnDragEnd()
    {
        base.OnDragEnd();
        var screenWidget = FindScreenWidget();
        screenWidget?.OnItemDrop();
    }

    private CrpgInventoryScreenWidget? FindScreenWidget()
    {
        Widget? current = this;
        while (current != null)
        {
            if (current is CrpgInventoryScreenWidget screen)
            {
                return screen;
            }

            current = current.ParentWidget;
        }

        return null;
    }
}
