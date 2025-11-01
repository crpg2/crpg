using Crpg.Module.Common;
using Crpg.Module.Common.KeyBinder;
using Crpg.Module.Common.KeyBinder.Models;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.ScreenSystem;

namespace Crpg.Module.GUI.Inventory;

public class CrpgMainGuiMissionView : MissionView, IUseKeyBinder
{
    private static readonly TimeSpan ApiRefreshCooldown = TimeSpan.FromSeconds(10); // how often to update from API when opening inventory, otherwise use cached data
    private DateTime _lastApiRefreshTime = DateTime.MinValue;
    private Action<object?>? _onVmCloseHandler;
    public enum CharacterEquipOpenedFromSource
    {
        MainGuiButton,
        Hotkey,
    }

    private static readonly string KeyCategoryId = KeyBinder.Categories.CrpgGeneral.CategoryId;
    private GauntletLayer? _mainGuiLayer;
    private CrpgMainGuiVM? _mainGuiVm;
    private IGauntletMovie? _mainGuiMovie;

    private GauntletLayer? _characterEquipLayer;
    private CrpgCharacterEquipVM? _characterEquipVm;
    private IGauntletMovie? _characterEquipMovie;
    private bool _characterEquipOpenedFromMainGui = false;

    BindedKeyCategory IUseKeyBinder.BindedKeys => new()
    {
        CategoryId = KeyCategoryId,
        Category = KeyBinder.Categories.CrpgGeneral.CategoryName,
        Keys = new List<BindedKey>
        {
            new()
            {
                Id = "key_toggle_equip_select",
                Name = "Open Character/Equipment Manager",
                Description = "Open character/equipment manager gui",
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
            _onVmCloseHandler ??= OnVmCloseRequested;

            // Initialize main GUI bar
            _mainGuiVm = new CrpgMainGuiVM();
            _mainGuiVm.OpenCharacterEquipRequested += reason => ToggleCharacterEquip(reason);
            _mainGuiLayer = new GauntletLayer(100);
            _mainGuiMovie = _mainGuiLayer.LoadMovie("CrpgMainGuiBarPrefab", _mainGuiVm);
            _mainGuiVm.IsVisible = false;
            MissionScreen.AddLayer(_mainGuiLayer);
        }
        catch (Exception ex)
        {
            string errorMessage = $"UI crash CrpgMainGuiBarPrefab: {ex.Message}";
            InformationManager.DisplayMessage(new InformationMessage(errorMessage));
            Debug.Print($"{errorMessage}\n{ex.StackTrace}", 0, Debug.DebugColor.DarkBlue);
        }
    }

    public override void OnMissionScreenFinalize()
    {
        base.OnMissionScreenFinalize();

        if (_characterEquipVm != null)
        {
            _characterEquipVm.OnCloseButtonClicked -= _onVmCloseHandler;
        }

        DeactivateLayer(ref _characterEquipLayer, ref _characterEquipMovie);
        DeactivateLayer(ref _mainGuiLayer, ref _mainGuiMovie);
    }

    public override void EarlyStart()
    {
        base.EarlyStart();

        toggleEquipmentSelectKey = HotKeyManager.GetCategory(KeyCategoryId).GetGameKey("key_toggle_equip_select");
    }

    public override void OnMissionScreenTick(float dt)
    {
        base.OnMissionScreenTick(dt);

        bool characterEquipOpen = _characterEquipVm?.IsVisible ?? false;
        bool mainGuiOpen = _mainGuiVm?.IsVisible ?? false;

        // Check hotkeys on active layer
        bool escapePressed = false;
        if (_characterEquipLayer != null && _characterEquipLayer.IsFocusLayer)
        {
            escapePressed = _characterEquipLayer.Input.IsHotKeyReleased("ToggleEscapeMenu") ||
                            _characterEquipLayer.Input.IsHotKeyReleased("Exit");
        }
        else if (_mainGuiLayer != null && _mainGuiLayer.IsFocusLayer && mainGuiOpen)
        {
            escapePressed = _mainGuiLayer.Input.IsHotKeyReleased("ToggleEscapeMenu") ||
                            _mainGuiLayer.Input.IsHotKeyReleased("Exit");
        }

        // Debug to confirm hotkey detection
        if (escapePressed)
        {
            // InformationManager.DisplayMessage(new InformationMessage("Escape hotkey detected"));
        }

        // Priority: If inventory is open and Escape hotkey is pressed, close inventory
        if (characterEquipOpen && escapePressed)
        {
            HideCharacterEquip();
            // CloseCharacterEquip();
            return;
        }

        // If no inventory is open and main GUI is open and Escape hotkey is pressed, close main GUI
        if (!characterEquipOpen && mainGuiOpen && escapePressed)
        {
            CloseMainGui();
            return;
        }

        // If M is pressed and no inventory is open, open main GUI
        if (!characterEquipOpen && Input.IsKeyReleased(InputKey.M))
        {
            // OpenMainGui(); // Disable for now until i add more gui elements to open from main bar
            return;
        }

        // Other hotkeys
        // if (Input.IsKeyReleased(InputKey.I))
        if (toggleEquipmentSelectKey != null && (Input.IsKeyPressed(toggleEquipmentSelectKey.KeyboardKey.InputKey) || Input.IsKeyDown(toggleEquipmentSelectKey.ControllerKey.InputKey)))
        {
            ToggleCharacterEquip(CharacterEquipOpenedFromSource.Hotkey);
            return;
        }

        // Tick VMs?
        // _mainGuiVm?.Tick();
        // _characterEquipVm?.Tick();
    }

    // Close button on GUI clicked
    private void OnVmCloseRequested(object? sender)
    {
        switch (sender)
        {
            case CrpgCharacterEquipVM:
                HideCharacterEquip();
                break;
            default:
                Debug.Print($"Unhandled VM close event from {sender?.GetType().Name}", 0, Debug.DebugColor.Red);
                break;
        }
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

    // CharacterEquip UI open/close/toggle methods
    private void OpenCharacterEquip(bool openedFromMainGui = false)
    {
        _characterEquipOpenedFromMainGui = openedFromMainGui;

        // Hide main GUI if it has focus
        HideMainGui();

        ActivateLayer(ref _characterEquipLayer, ref _characterEquipMovie, _characterEquipVm ??= new CrpgCharacterEquipVM(), "CrpgCharacterEquipPrefab", 110);
        _characterEquipVm.IsVisible = true;

        _characterEquipVm.OnCloseButtonClicked += _onVmCloseHandler;

        // Fetch items from server
        var now = DateTime.UtcNow;
        if (now - _lastApiRefreshTime < ApiRefreshCooldown)
        {
            InformationManager.DisplayMessage(new InformationMessage("_lastApiRefreshTime was recent, not updating from API", Colors.Red));
            return;
        }

        _lastApiRefreshTime = now;
        var loadout = Mission.Current.GetMissionBehavior<CrpgCharacterLoadoutBehaviorClient>();
        loadout?.RequestGetUpdatedEquipmentAndItems();
    }

    // Hides the CharacterEquip UI (sets IsVisible = false, removes focus/input)
    private void HideCharacterEquip()
    {
        if (_characterEquipLayer == null || _characterEquipVm == null)
        {
            return;
        }

        _characterEquipVm.IsVisible = false;
        _characterEquipLayer.IsFocusLayer = false;
        _characterEquipLayer.InputRestrictions.ResetInputRestrictions();
        ScreenManager.TryLoseFocus(_characterEquipLayer);

        // Restore main GUI only if characterEquip was opened from it
        if (_characterEquipOpenedFromMainGui && _mainGuiVm != null)
        {
            OpenMainGui();
        }

        // Reset the flag
        _characterEquipOpenedFromMainGui = false;
    }

    // Fully closes CharacterEquip UI (removes the layer and disposes ViewModel)
    private void CloseCharacterEquip()
    {
        if (_characterEquipLayer == null)
        {
            return;
        }

        try
        {
            // Hide the VM first
            if (_characterEquipVm != null)
            {
                _characterEquipVm.OnCloseButtonClicked -= _onVmCloseHandler;
                _characterEquipVm.IsVisible = false;
                _characterEquipVm.OnFinalize();

                _characterEquipVm = null;
            }

            // Deactivate and remove the layer
            DeactivateLayer(ref _characterEquipLayer, ref _characterEquipMovie);

            // Reset the flag
            _characterEquipOpenedFromMainGui = false;

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
            string errorMessage = $"CloseCharacterEquip exception: {ex.Message}";
            InformationManager.DisplayMessage(new InformationMessage(errorMessage));
            Debug.Print(errorMessage, 0, Debug.DebugColor.DarkBlue);
        }
    }

    // Toggle between open and hide (keep layer loaded)
    private void ToggleCharacterEquip(CharacterEquipOpenedFromSource reason)
    {
        if (_characterEquipVm == null || !_characterEquipVm.IsVisible)
        {
            if (reason == CharacterEquipOpenedFromSource.MainGuiButton)
            {
                OpenCharacterEquip(true); // track that we came from main gui
            }
            else if (reason == CharacterEquipOpenedFromSource.Hotkey)
            {
                OpenCharacterEquip(false); // came from hotkey, don’t restore main gui
            }
        }
        else
        {
            HideCharacterEquip();
            // CloseCharacterEquip();
        }
    }

    // Bring to front/set focus/input settings
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

        layer.IsFocusLayer = true;
        layer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
        ScreenManager.TrySetFocus(layer);
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
