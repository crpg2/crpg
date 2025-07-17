using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace Crpg.Module.GUI.Inventory;

public class CrpgInventoryMissionView : MissionView
{
    private bool _initialized;
    private GauntletLayer? _gauntletLayer;
    private CrpgInventoryViewModel? _dataSource;

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
            _gauntletLayer.LoadMovie("CrpgInventoryScreen", _dataSource);
            MissionScreen.AddLayer(_gauntletLayer);
        }
        catch (Exception ex)
        {
            InformationManager.DisplayMessage(new InformationMessage($"UI crash CrpgInventoryScreen: {ex.Message}"));
            Debug.Print($"{ex.Message}\n{ex.StackTrace}", 0, Debug.DebugColor.DarkBlue);
        }
    }
}
