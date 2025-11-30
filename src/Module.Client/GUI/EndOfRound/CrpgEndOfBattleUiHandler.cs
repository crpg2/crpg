using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace Crpg.Module.GUI.EndOfRound;

public class CrpgEndOfBattleUIHandler : MissionView
{
    public override void OnMissionScreenInitialize()
    {
        base.OnMissionScreenInitialize();
        ViewOrderPriority = 30;
        _dataSource = new CrpgEndOfBattleVM();
        _gauntletLayer = new GauntletLayer("MultiplayerEndOfBattle", ViewOrderPriority, false);
        _gauntletLayer.LoadMovie("MultiplayerEndOfBattle", _dataSource);
        _lobbyComponent = Mission.GetMissionBehavior<MissionLobbyComponent>();
        _lobbyComponent.OnPostMatchEnded += OnPostMatchEnded;
        MissionScreen.AddLayer(_gauntletLayer);
    }

    public override void OnMissionScreenFinalize()
    {
        base.OnMissionScreenFinalize();
        _lobbyComponent.OnPostMatchEnded -= OnPostMatchEnded;
    }

    public override void OnMissionScreenTick(float dt)
    {
        base.OnMissionScreenTick(dt);
        _dataSource.OnTick(dt);
    }

    private void OnPostMatchEnded()
    {
        _dataSource.OnBattleEnded();
    }

    private CrpgEndOfBattleVM _dataSource = default!;

    private GauntletLayer _gauntletLayer = default!;

    private MissionLobbyComponent _lobbyComponent = default!;
}
