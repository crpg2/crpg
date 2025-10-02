using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.ScreenSystem;

namespace Crpg.Module.GUI.Inventory;

public class CrpgInventoryMissionView : MissionView
{
    private bool _initialized;
    private GauntletLayer? _gauntletLayer;
    private CrpgInventoryViewModel? _dataSource;
    private IGauntletMovie? _movie;

    public override void OnMissionTick(float dt)
    {
        base.OnMissionTick(dt);
        if (_gauntletLayer != null && _gauntletLayer.IsActive && (_gauntletLayer.Input.IsHotKeyReleased("ToggleEscapeMenu") || _gauntletLayer.Input.IsHotKeyReleased("Exit")))
        {
            Hide();
        }
    }

    public void Close()
    {
        if (_gauntletLayer != null)
        {
            _gauntletLayer.InputRestrictions.ResetInputRestrictions();
            MissionScreen.RemoveLayer(_gauntletLayer);
            _gauntletLayer = null;
        }
    }

    public void Open()
    {
        if (_initialized)
        {
            Show();
        }
        else
        {
            InitializeInventoryUI();
        }
    }

    public void Show()
    {
        if (_gauntletLayer != null && _movie != null)
        {
            _gauntletLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
            ScreenManager.TrySetFocus(_gauntletLayer);

            _movie.RootWidget.IsVisible = true;
        }
    }

    public void Hide()
    {
        if (_gauntletLayer != null && _movie != null)
        {
            _gauntletLayer.InputRestrictions.ResetInputRestrictions();
            ScreenManager.TryLoseFocus(_gauntletLayer);

            _movie.RootWidget.IsVisible = false;
        }
    }

    public override void OnMissionScreenActivate()
    {
        base.OnMissionScreenActivate();
    }

    public override void OnMissionScreenTick(float dt)
    {
        base.OnMissionScreenTick(dt);
        if (!_initialized && Mission.Current?.MainAgent != null)
        {
            // _initialized = true;
            // InitializeInventoryUI();
        }
    }

    public override void OnMissionScreenFinalize()
    {
        base.OnMissionScreenFinalize();
        CleanupUI();
    }

    private void InitializeInventoryUI()
    {
        if (_initialized || MissionScreen == null)
        {
            return;
        }

        try
        {
            // _dataSource = new CrpgInventoryViewModel();
            _gauntletLayer = new GauntletLayer(150)
            {
                IsFocusLayer = true,
            };

            _gauntletLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
            _gauntletLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
            _movie = _gauntletLayer.LoadMovie("CrpgInventoryScreen", _dataSource);
            MissionScreen.AddLayer(_gauntletLayer);
            ScreenManager.TrySetFocus(_gauntletLayer);

            _initialized = true;
        }
        catch (Exception ex)
        {
            InformationManager.DisplayMessage(new InformationMessage($"UI crash CrpgInventoryScreen: {ex.Message}"));
            Debug.Print($"{ex.Message}\n{ex.StackTrace}", 0, Debug.DebugColor.DarkBlue);
        }
    }

    private void CleanupUI()
    {
        if (_gauntletLayer != null)
        {
            _gauntletLayer.InputRestrictions.ResetInputRestrictions();
            if (MissionScreen != null)
            {
                MissionScreen.RemoveLayer(_gauntletLayer);
                _gauntletLayer = null;
            }
        }

        _dataSource?.OnFinalize();
        _dataSource = null;

        _initialized = false;
    }
}
