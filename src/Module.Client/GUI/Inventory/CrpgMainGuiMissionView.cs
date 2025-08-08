using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.ScreenSystem;
using TaleWorlds.Core;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.Engine.Screens;
using TaleWorlds.GauntletUI.ExtraWidgets;
using TaleWorlds.GauntletUI.PrefabSystem;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.GauntletUI.Data;

namespace Crpg.Module.GUI.Inventory;

public class CrpgMainGuiMissionView : MissionView
{
    private GauntletLayer? _gauntletLayer;
    private CrpgMainGuiVM? _dataSource;
    private IGauntletMovie? _movie;
    private Widget? _rootWidget;
    private bool _isHidden;
    private bool _isActive;

    public override void OnMissionScreenInitialize()
    {
        base.OnMissionScreenInitialize();

        try
        {
            _dataSource = new CrpgMainGuiVM();
            _gauntletLayer = new GauntletLayer(100);
            _movie = _gauntletLayer.LoadMovie("CrpgMainGuiBarPrefab", _dataSource);
            _rootWidget = _movie.RootWidget;

            _dataSource.IsVisible = false;
            MissionScreen.AddLayer(_gauntletLayer);
        }
        catch (Exception ex)
        {
            InformationManager.DisplayMessage(new InformationMessage($"UI crash CrpgMainGuiBarPrefab: {ex.Message}"));
            Debug.Print($"{ex.Message}\n{ex.StackTrace}", 0, Debug.DebugColor.DarkBlue);
        }
    }

    public override void OnMissionScreenTick(float dt)
    {
        base.OnMissionScreenTick(dt);

        if (_gauntletLayer == null)
            return;

        if (_gauntletLayer != null && _gauntletLayer.IsActive && (_gauntletLayer.Input.IsHotKeyReleased("ToggleEscapeMenu") || _gauntletLayer.Input.IsHotKeyReleased("Exit")))
        {
            ToggleMainGui();
        }

        _dataSource?.Tick();

        if (Input.IsKeyReleased(InputKey.M))
        {
            ToggleMainGui();
        }
    }

    private void ToggleMainGui()
    {
        if (_gauntletLayer == null || _dataSource == null)
            return;

        if (_dataSource.IsVisible)
        {
            _dataSource.IsVisible = false;
            _gauntletLayer.IsFocusLayer = false; // keep gameplay focus
                                                 // Optional: release input restrictions when hidden
            _gauntletLayer.InputRestrictions.ResetInputRestrictions();
            ScreenManager.TryLoseFocus(_gauntletLayer);
        }
        else
        {
            _dataSource.IsVisible = true;
            _gauntletLayer.IsFocusLayer = true;
            // Optional: restrict input to UI when shown
            _gauntletLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
            _gauntletLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
            ScreenManager.TrySetFocus(_gauntletLayer);

        }
    }

    public override void OnMissionScreenFinalize()
    {
        base.OnMissionScreenFinalize();

        if (_gauntletLayer != null)
        {
            MissionScreen.RemoveLayer(_gauntletLayer);
            _gauntletLayer = null;
        }

        _dataSource?.OnFinalize();
        _dataSource = null;
    }
}


