using Crpg.Module.Common;
using Crpg.Module.Common.KeyBinder;
using Crpg.Module.Common.KeyBinder.Models;
using Crpg.Module.Common.Network;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Engine.Screens;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.GauntletUI.ExtraWidgets;
using TaleWorlds.GauntletUI.PrefabSystem;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.ScreenSystem;
using TaleWorlds.TwoDimension;

namespace Crpg.Module.GUI.Inventory;

public class CrpgMainGuiMissionView : MissionView, IUseKeyBinder
{
    public enum InventoryOpenSource
    {
        MainGuiButton,
        Hotkey,
    }

    private static readonly string KeyCategoryId = KeyBinder.Categories.CrpgGeneral.CategoryId;
    private GauntletLayer? _mainGuiLayer;
    private CrpgMainGuiVM? _mainGuiVm;
    private IGauntletMovie? _mainGuiMovie;

    private GauntletLayer? _inventoryLayer;
    private CrpgInventoryViewModel? _inventoryVm;
    private IGauntletMovie? _inventoryMovie;
    private bool _inventoryOpenedFromMainGui = false;

    BindedKeyCategory IUseKeyBinder.BindedKeys => new()
    {
        CategoryId = KeyCategoryId,
        Category = KeyBinder.Categories.CrpgGeneral.CategoryName,
        Keys = new List<BindedKey>
        {
            new()
            {
                Id = "key_toggle_equip_select",
                Name = "Open Equipment Manager",
                Description = "Open equipment manager gui",
                DefaultInputKey = InputKey.I,
            },
        },
    };

    private GameKey? toggleEquipmentSelectKey;

    public override void OnMissionScreenInitialize()
    {
        base.OnMissionScreenInitialize();

        try
        {
            // Initialize main GUI bar
            _mainGuiVm = new CrpgMainGuiVM();
            _mainGuiVm.OpenInventoryRequested += reason => ToggleInventory(reason);
            _mainGuiLayer = new GauntletLayer(100);
            _mainGuiMovie = _mainGuiLayer.LoadMovie("CrpgMainGuiBarPrefab", _mainGuiVm);
            _mainGuiVm.IsVisible = false;
            MissionScreen.AddLayer(_mainGuiLayer);

            // Inventory UI starts closed — don't create layer until needed
            _inventoryLayer = null;
            _inventoryVm = null;
            _inventoryMovie = null;

            // Debug hotkey registration
            var category = HotKeyManager.GetCategory("GenericPanelGameKeyCategory");
            // string hotkeyMessage = $"Hotkeys in GenericPanelGameKeyCategory: {string.Join(", ", category.RegisteredHotKeys.Select(h => h.Id))}";
            // InformationManager.DisplayMessage(new InformationMessage(hotkeyMessage));
            // Debug.Print(hotkeyMessage, 0, Debug.DebugColor.DarkBlue);
        }
        catch (Exception ex)
        {
            string errorMessage = $"UI crash CrpgMainGuiBarPrefab: {ex.Message}";
            InformationManager.DisplayMessage(new InformationMessage(errorMessage));
            Debug.Print($"{errorMessage}\n{ex.StackTrace}", 0, Debug.DebugColor.DarkBlue);
        }
    }

    public override void EarlyStart()
    {
        base.EarlyStart();

        toggleEquipmentSelectKey = HotKeyManager.GetCategory(KeyCategoryId).GetGameKey("key_toggle_equip_select");
    }

    public override void OnMissionScreenTick(float dt)
    {
        base.OnMissionScreenTick(dt);

        bool inventoryOpen = _inventoryVm?.IsVisible ?? false;
        bool mainGuiOpen = _mainGuiVm?.IsVisible ?? false;

        // Check hotkeys on active layer
        bool escapePressed = false;
        if (_inventoryLayer != null && _inventoryLayer.IsFocusLayer)
        {
            escapePressed = _inventoryLayer.Input.IsHotKeyReleased("ToggleEscapeMenu") ||
                            _inventoryLayer.Input.IsHotKeyReleased("Exit");
        }
        else if (_mainGuiLayer != null && _mainGuiLayer.IsFocusLayer && mainGuiOpen)
        {
            escapePressed = _mainGuiLayer.Input.IsHotKeyReleased("ToggleEscapeMenu") ||
                            _mainGuiLayer.Input.IsHotKeyReleased("Exit");
        }

        // Debug to confirm hotkey detection
        if (escapePressed)
        {
            string message = "Escape hotkey detected";
            InformationManager.DisplayMessage(new InformationMessage(message));
            Debug.Print(message, 0, Debug.DebugColor.DarkBlue);
        }

        // Priority: If inventory is open and Escape hotkey is pressed, close inventory
        if (inventoryOpen && escapePressed)
        {
            // HideInventory();
            CloseInventory();
            return;
        }

        // If no inventory is open and main GUI is open and Escape hotkey is pressed, close main GUI
        if (!inventoryOpen && mainGuiOpen && escapePressed)
        {
            CloseMainGui();
            return;
        }

        // If M is pressed and no inventory is open, open main GUI
        if (!inventoryOpen && Input.IsKeyReleased(InputKey.M))
        {
            OpenMainGui();
            return;
        }

        // Other hotkeys
        // if (Input.IsKeyReleased(InputKey.I))
        if (toggleEquipmentSelectKey != null && (Input.IsKeyPressed(toggleEquipmentSelectKey.KeyboardKey.InputKey) || Input.IsKeyDown(toggleEquipmentSelectKey.ControllerKey.InputKey)))
        {
            ToggleInventory(InventoryOpenSource.Hotkey);
            return;
        }

        // Tick VMs
        _mainGuiVm?.Tick();
        // _inventoryVm?.Tick();
    }

    // Main GUI bar open/close/toggle methods
    private void ToggleMainGui()
    {
        if (_mainGuiLayer == null || _mainGuiVm == null)
        {
            return;
        }

        if (_mainGuiVm.IsVisible)
        {
            CloseMainGui();
        }
        else
        {
            OpenMainGui();
        }
    }

    private void OpenMainGui()
    {
        if (_mainGuiLayer == null || _mainGuiVm == null)
        {
            return;
        }

        _mainGuiVm.IsVisible = true;
        _mainGuiLayer.IsFocusLayer = true;
        _mainGuiLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
        _mainGuiLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
        ScreenManager.TrySetFocus(_mainGuiLayer);
        string message = "Main GUI opened";
        InformationManager.DisplayMessage(new InformationMessage(message));
        Debug.Print(message, 0, Debug.DebugColor.DarkBlue);
    }

    private void HideMainGui()
    {
        if (_mainGuiLayer == null || _mainGuiVm == null)
        {
            InformationManager.DisplayMessage(new InformationMessage($"HideMainGui() -- _mainGuiLayer or _mainGuiVm is null"));
            return;
        }

        _mainGuiVm.IsVisible = false;
        _mainGuiLayer.IsFocusLayer = false;
        _mainGuiLayer.InputRestrictions.ResetInputRestrictions();
        ScreenManager.TryLoseFocus(_mainGuiLayer);
        string message = "Main GUI bar Hidden";
        InformationManager.DisplayMessage(new InformationMessage(message));
        Debug.Print(message, 0, Debug.DebugColor.DarkBlue);
    }

    private void CloseMainGui()
    {
        if (_mainGuiLayer == null || _mainGuiVm == null)
        {
            InformationManager.DisplayMessage(new InformationMessage($"CloseMainGui() -- _mainGuiLayer or _mainGuiVm is null"));
            return;
        }

        _mainGuiVm.IsVisible = false;
        _mainGuiLayer.IsFocusLayer = false;
        _mainGuiLayer.InputRestrictions.ResetInputRestrictions();
        ScreenManager.TryLoseFocus(_mainGuiLayer);
        string message = "Main GUI closed";
        InformationManager.DisplayMessage(new InformationMessage(message));
        Debug.Print(message, 0, Debug.DebugColor.DarkBlue);
    }

    // Inventory UI open/close/toggle methods
    private void OpenInventory(bool openedFromMainGui = false)
    {
        _inventoryOpenedFromMainGui = openedFromMainGui;

        // Hide main GUI if it has focus
        HideMainGui();

        ActivateLayer(ref _inventoryLayer, ref _inventoryMovie, _inventoryVm ??= new CrpgInventoryViewModel(), "CrpgInventoryScreen", 110);
        _inventoryVm.IsVisible = true;

        _inventoryVm.Movie = _inventoryMovie;

        var rootWidget = _inventoryVm?.Movie?.RootWidget;
        if (_inventoryVm != null && rootWidget != null && _inventoryLayer != null)
        {
            _inventoryVm.SetRootWidget(rootWidget);
            _inventoryVm.SetContext(_inventoryLayer.UIContext);
        }

        // Fetch items from server
        var loadout = Mission.Current.GetMissionBehavior<CrpgCharacterLoadoutBehaviorClient>();
        loadout?.RequestGetUpdatedEquipmentAndItems();
    }

    // Hides the inventory UI (sets IsVisible = false, removes focus/input)
    private void HideInventory()
    {
        if (_inventoryLayer == null || _inventoryVm == null)
        {
            return;
        }

        _inventoryVm.IsVisible = false;
        _inventoryLayer.IsFocusLayer = false;
        _inventoryLayer.InputRestrictions.ResetInputRestrictions();
        ScreenManager.TryLoseFocus(_inventoryLayer);

        // Restore main GUI only if inventory was opened from it
        if (_inventoryOpenedFromMainGui && _mainGuiVm != null)
        {
            OpenMainGui();
        }

        // Reset the flag
        _inventoryOpenedFromMainGui = false;
    }

    // Fully closes inventory UI (removes the layer and disposes ViewModel)
    private void CloseInventory()
    {
        if (_inventoryLayer == null)
        {
            return;
        }

        try
        {
            // Hide the VM first
            if (_inventoryVm != null)
            {
                _inventoryVm.IsVisible = false;
                _inventoryVm.OnFinalize();
                _inventoryVm = null;
            }

            // Deactivate and remove the layer
            DeactivateLayer(ref _inventoryLayer, ref _inventoryMovie);

            // Reset the flag
            _inventoryOpenedFromMainGui = false;

            // Restore main GUI focus if it should be visible
            if (_mainGuiLayer != null && _mainGuiVm != null && _mainGuiVm.IsVisible)
            {
                _mainGuiLayer.IsFocusLayer = true;
                _mainGuiLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
                ScreenManager.TrySetFocus(_mainGuiLayer);
            }
        }
        catch (Exception ex)
        {
            string errorMessage = $"CloseInventory exception: {ex.Message}";
            InformationManager.DisplayMessage(new InformationMessage(errorMessage));
            Debug.Print(errorMessage, 0, Debug.DebugColor.DarkBlue);
        }
    }

    // Toggle between open and hide (keep layer loaded)
    private void ToggleInventory(InventoryOpenSource reason)
    {
        if (_inventoryVm == null || !_inventoryVm.IsVisible)
        {
            if (reason == InventoryOpenSource.MainGuiButton)
            {
                OpenInventory(true); // track that we came from main gui
            }
            else if (reason == InventoryOpenSource.Hotkey)
            {
                OpenInventory(false); // came from hotkey, don’t restore main gui
            }
        }
        else
        {
            // HideInventory();
            CloseInventory();
        }
    }

    private void ActivateLayer(ref GauntletLayer? layer, ref IGauntletMovie? movie, ViewModel vm, string prefabName, int layerOrder = 100)
    {
        if (layer == null)
        {
            layer = new GauntletLayer(layerOrder) { IsFocusLayer = true };
            layer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
            layer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));

            movie = layer.LoadMovie(prefabName, vm);

            if (ScreenManager.TopScreen is MissionScreen missionScreen)
            {
                missionScreen.AddLayer(layer);
            }
            else
            {
                ScreenManager.TopScreen?.AddLayer(layer);
            }
        }
        else
        {
            layer.IsFocusLayer = true;
            layer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
            ScreenManager.TrySetFocus(layer);
        }
    }

    private void DeactivateLayer(ref GauntletLayer? layer, ref IGauntletMovie? movie)
    {
        if (layer == null)
        {
            return;
        }

        try
        {
            // Remove focus and reset input
            layer.IsFocusLayer = false;
            layer.InputRestrictions.ResetInputRestrictions();
            ScreenManager.TryLoseFocus(layer);

            // Release movie if exists
            if (movie != null)
            {
                layer.ReleaseMovie(movie);
                movie = null;
            }

            // Remove the layer from the screen
            if (ScreenManager.TopScreen is MissionScreen missionScreen)
            {
                missionScreen.RemoveLayer(layer);
            }
            else
            {
                ScreenManager.TopScreen?.RemoveLayer(layer);
            }

            // Nullify the layer reference
            layer = null;
        }
        catch (Exception ex)
        {
            string errorMessage = $"DeactivateLayer exception: {ex.Message}";
            InformationManager.DisplayMessage(new InformationMessage(errorMessage));
            Debug.Print(errorMessage, 0, Debug.DebugColor.DarkBlue);
        }
    }
}
