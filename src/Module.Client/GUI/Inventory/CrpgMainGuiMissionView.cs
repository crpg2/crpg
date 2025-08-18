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

namespace Crpg.Module.GUI.Inventory;

public class CrpgMainGuiMissionView : MissionView
{
    private GauntletLayer? _mainGuiLayer;
    private CrpgMainGuiVM? _mainGuiVm;
    private IGauntletMovie? _mainGuiMovie;

    private GauntletLayer? _inventoryLayer;
    private CrpgInventoryViewModel? _inventoryVm;
    private IGauntletMovie? _inventoryMovie;

    public override void OnMissionScreenInitialize()
    {
        base.OnMissionScreenInitialize();

        try
        {
            // Initialize main GUI bar
            _mainGuiVm = new CrpgMainGuiVM();
            _mainGuiVm.OpenInventoryRequested += () => OpenInventory();
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
            string hotkeyMessage = $"Hotkeys in GenericPanelGameKeyCategory: {string.Join(", ", category.RegisteredHotKeys.Select(h => h.Id))}";
            InformationManager.DisplayMessage(new InformationMessage(hotkeyMessage));
            Debug.Print(hotkeyMessage, 0, Debug.DebugColor.DarkBlue);
        }
        catch (Exception ex)
        {
            string errorMessage = $"UI crash CrpgMainGuiBarPrefab: {ex.Message}";
            InformationManager.DisplayMessage(new InformationMessage(errorMessage));
            Debug.Print($"{errorMessage}\n{ex.StackTrace}", 0, Debug.DebugColor.DarkBlue);
        }
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

        // Debug layer and input state
        /*
        string tickMessage = $"Tick: invOpen={inventoryOpen} mainGuiOpen={mainGuiOpen} esc={escapePressed} " +
                            $"invFocus={_inventoryLayer?.IsFocusLayer} mainGuiFocus={_mainGuiLayer?.IsFocusLayer}";
        InformationManager.DisplayMessage(new InformationMessage(tickMessage));
        Debug.Print(tickMessage, 0, Debug.DebugColor.DarkBlue);
        */

        // 1️⃣ Priority: If inventory is open and Escape hotkey is pressed, close inventory
        if (inventoryOpen && escapePressed)
        {
            //CloseInventory();
            HideInventory();
            return;
        }

        // 2️⃣ If no inventory is open and main GUI is open and Escape hotkey is pressed, close main GUI
        if (!inventoryOpen && mainGuiOpen && escapePressed)
        {
            CloseMainGui();
            return;
        }

        // 3️⃣ If M is pressed and no inventory is open, open main GUI
        if (!inventoryOpen && Input.IsKeyReleased(InputKey.M))
        {
            OpenMainGui();
            return;
        }

        // 4️⃣ Other hotkeys
        if (Input.IsKeyReleased(InputKey.I))
        {
            ToggleInventory();
            return;
        }

        // 5️⃣ Tick VMs
        _mainGuiVm?.Tick();
    }

    // Main GUI bar open/close/toggle methods
    private void ToggleMainGui()
    {
        if (_mainGuiLayer == null || _mainGuiVm == null)
            return;

        if (_mainGuiVm.IsVisible)
            CloseMainGui();
        else
            OpenMainGui();
    }

    private void OpenMainGui()
    {
        if (_mainGuiLayer == null || _mainGuiVm == null)
            return;

        _mainGuiVm.IsVisible = true;
        _mainGuiLayer.IsFocusLayer = true;
        _mainGuiLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
        _mainGuiLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
        ScreenManager.TrySetFocus(_mainGuiLayer);
        string message = "Main GUI opened";
        InformationManager.DisplayMessage(new InformationMessage(message));
        Debug.Print(message, 0, Debug.DebugColor.DarkBlue);
    }

    private void CloseMainGui()
    {
        if (_mainGuiLayer == null || _mainGuiVm == null)
            return;

        _mainGuiVm.IsVisible = false;
        _mainGuiLayer.IsFocusLayer = false;
        _mainGuiLayer.InputRestrictions.ResetInputRestrictions();
        ScreenManager.TryLoseFocus(_mainGuiLayer);
        string message = "Main GUI closed";
        InformationManager.DisplayMessage(new InformationMessage(message));
        Debug.Print(message, 0, Debug.DebugColor.DarkBlue);
    }

    // Inventory UI open/close/toggle methods
    public void OpenInventory()
    {
        // If main GUI currently has focus, release it so inventory can receive input
        if (_mainGuiLayer != null)
        {
            _mainGuiLayer.IsFocusLayer = false;
            _mainGuiLayer.InputRestrictions.ResetInputRestrictions();
            string message = "Main GUI focus released for inventory";
            InformationManager.DisplayMessage(new InformationMessage(message));
            Debug.Print(message, 0, Debug.DebugColor.DarkBlue);
        }

        if (_inventoryLayer == null)
        {
            // Create and add layer once
            _inventoryVm = new CrpgInventoryViewModel();
            _inventoryVm.IsVisible = true;
            _inventoryLayer = new GauntletLayer(110);
            _inventoryLayer.IsFocusLayer = true;

            // Give layer input control
            _inventoryLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
            _inventoryLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));

            try
            {
                _inventoryMovie = _inventoryLayer.LoadMovie("CrpgInventoryScreen", _inventoryVm);
            }
            catch (Exception ex)
            {
                string errorMessage = $"Failed to load inventory UI: {ex.Message}";
                InformationManager.DisplayMessage(new InformationMessage(errorMessage));
                Debug.Print(errorMessage, 0, Debug.DebugColor.DarkBlue);
                return;
            }

            if (ScreenManager.TopScreen is MissionScreen missionScreen)
            {
                missionScreen.AddLayer(_inventoryLayer);
                string message = "Inventory layer added to MissionScreen";
                InformationManager.DisplayMessage(new InformationMessage(message));
                Debug.Print(message, 0, Debug.DebugColor.DarkBlue);
            }
            else
            {
                ScreenManager.TopScreen?.AddLayer(_inventoryLayer);
                string message = "Inventory layer added to TopScreen";
                InformationManager.DisplayMessage(new InformationMessage(message));
                Debug.Print(message, 0, Debug.DebugColor.DarkBlue);
            }

            ScreenManager.TrySetFocus(_inventoryLayer);
        }
        else
        {
            // Already created, just show and enable input focus
            _inventoryVm!.IsVisible = true;
            _inventoryLayer!.IsFocusLayer = true;
            _inventoryLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
            ScreenManager.TrySetFocus(_inventoryLayer);
        }

        // update available items
        GameNetwork.BeginModuleEventAsClient();
        GameNetwork.WriteMessage(new RequestCrpgUserInventoryItems());
        GameNetwork.EndModuleEventAsClient();

        string openMessage = $"Inventory opened: VM={_inventoryVm != null}, IsVisible={_inventoryVm?.IsVisible}, Focus={_inventoryLayer?.IsFocusLayer}";
        InformationManager.DisplayMessage(new InformationMessage(openMessage));
        Debug.Print(openMessage, 0, Debug.DebugColor.DarkBlue);
    }

    // Hides the inventory UI (sets IsVisible = false, removes focus/input)
    public void HideInventory()
    {
        if (_inventoryLayer == null || _inventoryVm == null)
            return;

        _inventoryVm.IsVisible = false;
        _inventoryLayer.IsFocusLayer = false;
        _inventoryLayer.InputRestrictions.ResetInputRestrictions();
        ScreenManager.TryLoseFocus(_inventoryLayer);
        string message = "Inventory hidden";
        InformationManager.DisplayMessage(new InformationMessage(message));
        Debug.Print(message, 0, Debug.DebugColor.DarkBlue);
    }

    // Fully closes inventory UI (removes the layer and disposes ViewModel)
    public void CloseInventory()
    {
        if (_inventoryLayer == null)
        {
            string message = "CloseInventory called but no layer exists";
            InformationManager.DisplayMessage(new InformationMessage(message));
            Debug.Print(message, 0, Debug.DebugColor.DarkBlue);
            return;
        }

        try
        {
            // Release focus/input from inventory layer
            _inventoryLayer.InputRestrictions.ResetInputRestrictions();
            _inventoryLayer.IsFocusLayer = false;
            ScreenManager.TryLoseFocus(_inventoryLayer);
            string focusMessage = "CloseInventory: Focus released";
            InformationManager.DisplayMessage(new InformationMessage(focusMessage));
            Debug.Print(focusMessage, 0, Debug.DebugColor.DarkBlue);

            // Explicitly release the movie
            _inventoryLayer?.ReleaseMovie(_inventoryMovie);
            _inventoryMovie = null;
            string movieMessage = "CloseInventory: Movie released";
            InformationManager.DisplayMessage(new InformationMessage(movieMessage));
            Debug.Print(movieMessage, 0, Debug.DebugColor.DarkBlue);

            // Remove the layer from the screen
            if (ScreenManager.TopScreen is MissionScreen missionScreen)
            {
                missionScreen.RemoveLayer(_inventoryLayer);
                string layerMessage = "CloseInventory: Layer removed from MissionScreen";
                InformationManager.DisplayMessage(new InformationMessage(layerMessage));
                Debug.Print(layerMessage, 0, Debug.DebugColor.DarkBlue);
            }
            else
            {
                ScreenManager.TopScreen?.RemoveLayer(_inventoryLayer);
                string layerMessage = "CloseInventory: Layer removed from TopScreen";
                InformationManager.DisplayMessage(new InformationMessage(layerMessage));
                Debug.Print(layerMessage, 0, Debug.DebugColor.DarkBlue);
            }

            // Finalize VM
            if (_inventoryVm != null)
            {
                // Hide first
                _inventoryVm.IsVisible = false;
                string hideMessage = "CloseInventory: VM hidden";
                InformationManager.DisplayMessage(new InformationMessage(hideMessage));
                Debug.Print(hideMessage, 0, Debug.DebugColor.DarkBlue);

                _inventoryVm.OnFinalize();
                _inventoryVm = null;
                string finalizeMessage = "CloseInventory: VM finalized";
                InformationManager.DisplayMessage(new InformationMessage(finalizeMessage));
                Debug.Print(finalizeMessage, 0, Debug.DebugColor.DarkBlue);
            }

            _inventoryLayer = null;
            string clearMessage = "CloseInventory: Layer cleared";
            InformationManager.DisplayMessage(new InformationMessage(clearMessage));
            Debug.Print(clearMessage, 0, Debug.DebugColor.DarkBlue);

            // If the main GUI should get focus back, restore it
            if (_mainGuiLayer != null && _mainGuiVm != null && _mainGuiVm.IsVisible)
            {
                _mainGuiLayer.IsFocusLayer = true;
                _mainGuiLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
                ScreenManager.TrySetFocus(_mainGuiLayer);
                string restoreMessage = "CloseInventory: Main GUI focus restored";
                InformationManager.DisplayMessage(new InformationMessage(restoreMessage));
                Debug.Print(restoreMessage, 0, Debug.DebugColor.DarkBlue);
            }

            string closeMessage = "Inventory closed";
            InformationManager.DisplayMessage(new InformationMessage(closeMessage));
            Debug.Print(closeMessage, 0, Debug.DebugColor.DarkBlue);
        }
        catch (Exception ex)
        {
            string errorMessage = $"CloseInventory exception: {ex.Message}";
            InformationManager.DisplayMessage(new InformationMessage(errorMessage));
            Debug.Print(errorMessage, 0, Debug.DebugColor.DarkBlue);
        }
    }

    // Toggle between open and hide (keep layer loaded)
    public void ToggleInventory()
    {
        if (_inventoryVm == null || !_inventoryVm.IsVisible)
            OpenInventory();
        else
            HideInventory();
    }
}