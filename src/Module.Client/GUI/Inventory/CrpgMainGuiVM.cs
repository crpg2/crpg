using TaleWorlds.Library;

namespace Crpg.Module.GUI.Inventory;

public class CrpgMainGuiVM : ViewModel
{
    private bool _isVisible;
    private CrpgInventoryViewModel? _inventoryVm;

    public enum InventoryOpenSource
    {
        MainGuiButton,
        Hotkey,
    }

    public event Action<CrpgMainGuiMissionView.InventoryOpenSource>? OpenInventoryRequested;
    public CrpgMainGuiVM()
    {
    }

    public void OpenInventoryFromButton()
    {
        OpenInventory(CrpgMainGuiMissionView.InventoryOpenSource.MainGuiButton);
    }

    public void OpenInventory(CrpgMainGuiMissionView.InventoryOpenSource reason)
    {
        OpenInventoryRequested?.Invoke(reason);
    }

    // This reflects the visibility of the main GUI bar prefab,
    // managed by MissionView toggling IsVisible
    [DataSourceProperty]
    public bool IsVisible
    {
        get => _isVisible;
        set
        {
            if (value != _isVisible)
            {
                _isVisible = value;
                OnPropertyChanged(nameof(IsVisible));
            }
        }
    }

    // Optional: you can expose InventoryVm here to bind inventory UI data,
    // but creation and lifecycle of this is managed by MissionView now
    [DataSourceProperty]
    public CrpgInventoryViewModel? InventoryVm
    {
        get => _inventoryVm;
        set
        {
            if (_inventoryVm != value)
            {
                _inventoryVm = value;
                OnPropertyChanged(nameof(InventoryVm));
            }
        }
    }

    // You can keep this for any per-frame updates your VM needs,
    // but input and layer toggling moves to MissionView
    public void Tick()
    {
        // Any ViewModel-only updates here
        // _inventoryVm?.CharacterInfo?.
    }

    public override void OnFinalize()
    {
        base.OnFinalize();
        // Clean up VM resources (but not UI layers)
        _inventoryVm?.OnFinalize();
        _inventoryVm = null;

        base.OnFinalize();
    }
}
