using TaleWorlds.Core;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;

namespace Crpg.Module.GUI.Inventory;

/// <summary>
/// Drop-target widget representing an equipment slot.
/// Fires the <c>Drop</c> event when a dragged item is dropped on it.
/// </summary>
internal class CrpgInventorySlotWidget : ButtonWidget
{
    public CrpgInventorySlotWidget(UIContext context)
        : base(context)
    {
        AcceptDrop = true;
        DropEventHandledManually = true;
    }

    /// <summary>Bound to the slot's <see cref="CrpgInventorySlotVm.SlotIndex"/>.</summary>
    [Editor(false)]
    public int SlotIndex { get; set; }

    /// <summary>Bound to <see cref="CrpgInventorySlotVm.IsDropTarget"/> for visual highlighting.</summary>
    [Editor(false)]
    public bool IsDropTarget { get; set; }

    protected override bool OnDrop()
    {
        var screenWidget = FindScreenWidget();
        if (screenWidget != null && screenWidget.DraggedMbGuid != 0)
        {
            screenWidget.HandleItemDropOnSlot(screenWidget.DraggedMbGuid, SlotIndex);
            return true;
        }

        return base.OnDrop();
    }

    protected override void OnUpdate(float dt)
    {
        base.OnUpdate(dt);

        // Highlight this slot when a compatible item is being dragged.
        var screenWidget = FindScreenWidget();
        if (screenWidget == null)
        {
            IsDropTarget = false;
            return;
        }

        if (screenWidget.DraggedItemType < 0)
        {
            IsDropTarget = false;
            return;
        }

        // Weapon slots (0-3) all highlight when dragging any weapon.
        bool isWeaponSlot = SlotIndex >= (int)EquipmentIndex.Weapon0 && SlotIndex <= (int)EquipmentIndex.Weapon3;
        bool isDraggingWeapon = screenWidget.DraggedItemType >= 0 && screenWidget.DraggedItemType <= 13;
        IsDropTarget = isWeaponSlot && isDraggingWeapon;
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
