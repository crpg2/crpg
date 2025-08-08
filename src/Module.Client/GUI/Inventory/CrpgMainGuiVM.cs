using System;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.GauntletUI;
using TaleWorlds.ScreenSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade.View.Screens;

namespace Crpg.Module.GUI.Inventory;

public class CrpgMainGuiVM : ViewModel
{
    public GauntletLayer? InventoryLayer => _inventoryLayer;
    private GauntletLayer? _inventoryLayer;
    private CrpgInventoryViewModel? _inventoryVm;
    private bool _isVisible;
    private bool _isActive;
    private ViewModel? _currentContentViewModel;
    private bool _isContentVisible;

    public CrpgMainGuiVM()
    {
    }

    public void Tick()
    {
        if (_inventoryLayer != null && _inventoryLayer.IsActive)
        {
            if (_inventoryLayer.Input.IsHotKeyReleased("ToggleEscapeMenu") || _inventoryLayer.Input.IsHotKeyReleased("Exit"))
            {
                CloseInventory();
            }
        }
    }

    public void OnOpenInventoryButtonClicked()
    {
        InformationManager.DisplayMessage(new InformationMessage("OpenInventory button clicked!"));
        // Call your inventory opening logic here, for example:
        OpenInventory();
    }

    // Called by the prefab button ClickEvent="OpenInventory"

    public void TryShowInventory()
    {

    }
    public void OpenInventory()
    {
        try
        {
            // If already open, close it instead (toggle behavior)
            if (_inventoryLayer != null)
            {
                CloseInventory();
                return;
            }

            InventoryVm = new CrpgInventoryViewModel();
            InventoryVm.IsVisible = true;

            _inventoryLayer = new GauntletLayer(150);
            _inventoryLayer.IsFocusLayer = true;
            _inventoryLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);

            // Register hotkey category if you want keyboard handling
            _inventoryLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));

            // Load the prefab movie using the exact prefab name
            _inventoryLayer.LoadMovie("CrpgInventoryScreen", InventoryVm);

            // Add layer to mission UI if MissionScreen exists; otherwise try ScreenManager
            // Replace the broken MissionScreen.Instance check with:
            if (ScreenManager.TopScreen is MissionScreen missionScreen)
            {
                missionScreen.AddLayer(_inventoryLayer);
            }
            else
            {
                ScreenManager.TopScreen?.AddLayer(_inventoryLayer);
            }

            ScreenManager.TrySetFocus(_inventoryLayer);
        }
        catch (Exception ex)
        {
            InformationManager.DisplayMessage(new InformationMessage($"Failed to open inventory UI: {ex.Message}"));
        }
    }

    public void CloseInventory()
    {
        if (_inventoryLayer == null)
            return;

        try
        {
            _inventoryLayer.InputRestrictions.ResetInputRestrictions();

            if (ScreenManager.TopScreen is MissionScreen missionScreen)
            {
                missionScreen.RemoveLayer(_inventoryLayer);
            }
            else
            {
                ScreenManager.TopScreen?.RemoveLayer(_inventoryLayer);
            }
        }

        catch { }

        _inventoryLayer = null;

        if (InventoryVm != null)
        {
            InventoryVm.OnFinalize();
            InventoryVm = null;
        }
    }
    public override void OnFinalize()
    {
        CloseInventory();
        base.OnFinalize();
    }

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

    [DataSourceProperty]
    public ViewModel? CurrentContentViewModel
    {
        get => _currentContentViewModel;
        set
        {
            if (_currentContentViewModel != value)
            {
                _currentContentViewModel = value;
                OnPropertyChanged(nameof(CurrentContentViewModel));
            }
        }
    }

    [DataSourceProperty]
    public bool IsContentVisible
    {
        get => _isContentVisible;
        set
        {
            if (_isContentVisible != value)
            {
                _isContentVisible = value;
                OnPropertyChanged(nameof(IsContentVisible));
            }
        }
    }

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
}
