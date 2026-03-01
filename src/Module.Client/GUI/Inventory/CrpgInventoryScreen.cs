using Crpg.Module.Common;
using Crpg.Module.Common.Network;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.ObjectSystem;

namespace Crpg.Module.GUI.Inventory;

internal class CrpgInventoryScreen : MissionView
{
    private GauntletLayer _gauntletLayer = null!;
    private CrpgInventoryVm _dataSource = null!;
    private GauntletMovieIdentifier _movie = null!;
    private CrpgInventoryClient _inventoryClient = null!;

    public override void OnMissionScreenInitialize()
    {
        base.OnMissionScreenInitialize();

        _dataSource = new CrpgInventoryVm();
        _gauntletLayer = new GauntletLayer("CrpgInventory", ViewOrderPriority + 100);
        _movie = _gauntletLayer.LoadMovie("CrpgInventory", _dataSource);
        MissionScreen.AddLayer(_gauntletLayer);

        _inventoryClient = Mission.GetRequiredMissionBehavior<CrpgInventoryClient>();
        _inventoryClient.OnInventoryReceived += OnInventoryReceived;

        CrpgInventoryScreenWidget.ItemDroppedOnSlot += OnItemDroppedOnSlot;
        CrpgInventoryScreenWidget.ItemDoubleClicked += OnItemDoubleClicked;
    }

    public override void OnMissionScreenFinalize()
    {
        CrpgInventoryScreenWidget.ItemDoubleClicked -= OnItemDoubleClicked;
        CrpgInventoryScreenWidget.ItemDroppedOnSlot -= OnItemDroppedOnSlot;
        _inventoryClient.OnInventoryReceived -= OnInventoryReceived;
        _gauntletLayer.ReleaseMovie(_movie);
        MissionScreen.RemoveLayer(_gauntletLayer);
        _dataSource.OnFinalize();

        base.OnMissionScreenFinalize();
    }

    public override void OnMissionScreenTick(float dt)
    {
        base.OnMissionScreenTick(dt);

        // Toggle with I key. Skip when an EditableTextWidget has focus to avoid
        // closing the inventory while typing in the search box.
        if (Input.IsKeyPressed(InputKey.I) && !CrpgInventoryScreenWidget.IsTextInputFocused)
        {
            ToggleVisibility();
        }

        var myAgent = Mission?.MainAgent;
        _dataSource.IsAlive = myAgent != null && myAgent.IsActive();

        if (_dataSource.IsVisible)
        {
            RefreshEquippedState();
        }
    }

    public override bool OnEscape()
    {
        if (_dataSource.IsVisible)
        {
            HideInventory();
            return true;
        }

        return base.OnEscape();
    }

    /// <summary>Called from escape menu button.</summary>
    public void ToggleVisibility()
    {
        if (_dataSource.IsVisible)
        {
            HideInventory();
        }
        else
        {
            ShowInventory();
        }
    }

    private void OnItemDroppedOnSlot(uint mbGuid, int slotIndex)
    {
        var item = MBObjectManager.Instance.GetObject(new MBGUID(mbGuid)) as ItemObject;
        if (item != null)
        {
            _dataSource?.EquipToSlot(item, (EquipmentIndex)slotIndex);
        }
    }

    private void OnItemDoubleClicked(uint mbGuid)
    {
        var item = MBObjectManager.Instance.GetObject(new MBGUID(mbGuid)) as ItemObject;
        if (item != null)
        {
            _dataSource?.HandleAutoEquip(item);
        }
    }

    private void OnInventoryReceived(IList<CrpgOwnedItem> items)
    {
        _dataSource.PopulateItems(items);
        RefreshEquippedState();
    }

    private void ShowInventory()
    {
        _dataSource.IsVisible = true;
        _gauntletLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.Mouse);
        RefreshEquippedState();
    }

    private void HideInventory()
    {
        // Clear search text so the EditableTextWidget loses focus.
        _dataSource.SearchText = "";
        _dataSource.IsVisible = false;
        _gauntletLayer.InputRestrictions.ResetInputRestrictions();
        // Ensure no widget retains keyboard focus after closing.
        _gauntletLayer.IsFocusLayer = false;
    }

    private void RefreshEquippedState()
    {
        var myPeer = GameNetwork.MyPeer;
        if (myPeer == null)
        {
            return;
        }

        var crpgPeer = myPeer.GetComponent<CrpgPeer>();
        if (crpgPeer?.User != null)
        {
            _dataSource?.RefreshEquippedState(crpgPeer.User.Character.EquippedItems);
        }
    }
}
