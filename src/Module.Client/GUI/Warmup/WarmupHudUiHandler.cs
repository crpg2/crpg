using Crpg.Module.Modes.Warmup;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace Crpg.Module.GUI.Warmup;

internal class WarmupHudUiHandler : MissionView
{
    private WarmupHudVm? _dataSource;
    private GauntletLayer? _gauntletLayer;
    private CrpgWarmupComponent? _warmupComponent;

    public override void AfterStart()
    {
        base.AfterStart();
        _warmupComponent = Mission.GetMissionBehavior<CrpgWarmupComponent>();
        if (_warmupComponent != null)
        {
            _warmupComponent.OnUpdatePlayerCount += OnUpdatePlayerCount;
        }
    }

    public override void OnMissionScreenInitialize()
    {
        base.OnMissionScreenInitialize();

        _dataSource = new WarmupHudVm(Mission);
        _gauntletLayer = new GauntletLayer("WarmupHud", ViewOrderPriority);
        _gauntletLayer.LoadMovie("WarmupHud", _dataSource);
        MissionScreen.AddLayer(_gauntletLayer);
    }

    public override void OnMissionScreenFinalize()
    {
        MissionScreen.RemoveLayer(_gauntletLayer);
        _dataSource!.OnFinalize();
        base.OnMissionScreenFinalize();
        if (_warmupComponent != null)
        {
            _warmupComponent.OnUpdatePlayerCount -= OnUpdatePlayerCount;
        }
    }

    public override void OnMissionScreenTick(float dt)
    {
        base.OnMissionScreenTick(dt);
        _dataSource!.Tick(dt);
    }

    private void OnUpdatePlayerCount(int requiredPlayers)
    {
        _dataSource!.OnUpdateRequiredPlayers(requiredPlayers);
    }
}
