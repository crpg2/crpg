using System.Runtime.InteropServices;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Gameplay.Perks.Conditions;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.ScreenSystem;
using TaleWorlds.TwoDimension;

namespace Crpg.Module.GUI.Inventory;

public class CrpgInventoryMissionView : MissionView
{
    private bool _initialized;
    private GauntletLayer? _gauntletLayer;
    private CrpgInventoryViewModel? _dataSource;

    public override void OnMissionTick(float dt)
    {
        base.OnMissionTick(dt);
        if (_gauntletLayer != null && _gauntletLayer.IsActive && (_gauntletLayer.Input.IsHotKeyReleased("ToggleEscapeMenu") || _gauntletLayer.Input.IsHotKeyReleased("Exit")))
        {
            Close();
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
        InitializeInventoryUI();
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
            _initialized = true;
            InitializeInventoryUI();
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

    private void InitializeInventoryUI()
    {
        try
        {
            _dataSource = new CrpgInventoryViewModel();
            _gauntletLayer = new GauntletLayer(150);
            _gauntletLayer.IsFocusLayer = true;
            _gauntletLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
            _gauntletLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
            _gauntletLayer.LoadMovie("CrpgInventoryScreen", _dataSource);
            MissionScreen.AddLayer(_gauntletLayer);
            ScreenManager.TrySetFocus(_gauntletLayer);

        }
        catch (Exception ex)
        {
            InformationManager.DisplayMessage(new InformationMessage($"UI crash CrpgInventoryScreen: {ex.Message}"));
            Debug.Print($"{ex.Message}\n{ex.StackTrace}", 0, Debug.DebugColor.DarkBlue);
        }
    }

    private void file() => throw new NotImplementedException();
}
