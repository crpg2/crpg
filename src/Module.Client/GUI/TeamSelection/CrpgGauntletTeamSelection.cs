using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Multiplayer.GauntletUI.Mission;
using TaleWorlds.MountAndBlade.Multiplayer.View.MissionViews;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace Crpg.Module.GUI.TeamSelection;

[OverrideView(typeof(MultiplayerTeamSelectUIHandler))]
public class CrpgGauntletTeamSelection : MissionView
{
    public CrpgGauntletTeamSelection()
    {
        ViewOrderPriority = 22;
    }

    public override void OnMissionScreenInitialize()
    {
        base.OnMissionScreenInitialize();
        _missionNetworkComponent = Mission.GetMissionBehavior<MissionNetworkComponent>();
        _multiplayerTeamSelectComponent = Mission.GetMissionBehavior<MultiplayerTeamSelectComponent>();
        _classLoadoutGauntletComponent = Mission.GetMissionBehavior<MissionGauntletClassLoadout>();
        _lobbyComponent = Mission.GetMissionBehavior<MissionLobbyComponent>();
        _missionNetworkComponent.OnMyClientSynchronized += OnMyClientSynchronized;
        _lobbyComponent.OnPostMatchEnded += OnClose;
        _multiplayerTeamSelectComponent.OnSelectingTeam += MissionLobbyComponentOnSelectingTeam;
        _multiplayerTeamSelectComponent.OnUpdateTeams += MissionLobbyComponentOnUpdateTeams;
        _multiplayerTeamSelectComponent.OnUpdateFriendsPerTeam += MissionLobbyComponentOnFriendsUpdated;
        _scoreboardGauntletComponent = Mission.GetMissionBehavior<MissionGauntletMultiplayerScoreboard>();
        if (_scoreboardGauntletComponent != null)
        {
            MissionGauntletMultiplayerScoreboard scoreboardGauntletComponent = _scoreboardGauntletComponent;
            scoreboardGauntletComponent.OnScoreboardToggled = (Action<bool>)Delegate.Combine(scoreboardGauntletComponent.OnScoreboardToggled, new Action<bool>(OnScoreboardToggled));
        }

        _multiplayerTeamSelectComponent.OnMyTeamChange += OnMyTeamChanged;
    }

    public override void OnMissionScreenFinalize()
    {
        _missionNetworkComponent.OnMyClientSynchronized -= OnMyClientSynchronized;
        _lobbyComponent.OnPostMatchEnded -= OnClose;
        _multiplayerTeamSelectComponent.OnSelectingTeam -= MissionLobbyComponentOnSelectingTeam;
        _multiplayerTeamSelectComponent.OnUpdateTeams -= MissionLobbyComponentOnUpdateTeams;
        _multiplayerTeamSelectComponent.OnUpdateFriendsPerTeam -= MissionLobbyComponentOnFriendsUpdated;
        _multiplayerTeamSelectComponent.OnMyTeamChange -= OnMyTeamChanged;
        if (_gauntletLayer != null)
        {
            _gauntletLayer.InputRestrictions.ResetInputRestrictions();
            MissionScreen.RemoveLayer(_gauntletLayer);
            _gauntletLayer = null;
        }

        if (_dataSource != null)
        {
            _dataSource.OnFinalize();
            _dataSource = null;
        }

        if (_scoreboardGauntletComponent != null)
        {
            MissionGauntletMultiplayerScoreboard scoreboardGauntletComponent = _scoreboardGauntletComponent;
            scoreboardGauntletComponent.OnScoreboardToggled = (Action<bool>?)Delegate.Remove(scoreboardGauntletComponent.OnScoreboardToggled, new Action<bool>(OnScoreboardToggled));
        }

        base.OnMissionScreenFinalize();
    }

    public override bool OnEscape()
    {
        if (_dataSource == null)
        {
            return false;
        }

        if (_isActive && !_dataSource.IsCancelDisabled)
        {
            OnClose();
            return true;
        }

        return base.OnEscape();
    }

    public override void OnMissionScreenTick(float dt)
    {
        base.OnMissionScreenTick(dt);
        if (_isSynchronized && _toOpen && MissionScreen.SetDisplayDialog(true))
        {
            _toOpen = false;
            OnOpen();
        }

        CrpgTeamSelectVm? dataSource = _dataSource;
        if (dataSource == null)
        {
            return;
        }

        dataSource.Tick(dt);
    }

    private void OnClose()
    {
        if (!_isActive)
        {
            return;
        }

        _isActive = false;
        _disabledTeams = null;
        MissionScreen.RemoveLayer(_gauntletLayer);
        MissionScreen.SetCameraLockState(false);
        MissionScreen.SetDisplayDialog(false);
        _gauntletLayer?.InputRestrictions.ResetInputRestrictions();
        _gauntletLayer = null;
        _dataSource?.OnFinalize();
        _dataSource = null;
        if (_classLoadoutGauntletComponent != null && _classLoadoutGauntletComponent.IsForceClosed)
        {
            _classLoadoutGauntletComponent.OnTryToggle(true);
        }
    }

    private void OnOpen()
    {
        if (_isActive)
        {
            return;
        }

        _isActive = true;
        string strValue = MultiplayerOptions.OptionType.GameType.GetStrValue();
        _dataSource = new CrpgTeamSelectVm(Mission, OnChangeTeamTo, OnAutoassign, OnClose, Mission.Teams, strValue);
        _dataSource.RefreshDisabledTeams(_disabledTeams ?? new List<Team>());
        _gauntletLayer = new GauntletLayer("MultiplayerTeamSelection", ViewOrderPriority);
        _gauntletLayer.LoadMovie("MultiplayerTeamSelection", _dataSource);
        _gauntletLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.Mouse);
        MissionScreen.AddLayer(_gauntletLayer);
        MissionScreen.SetCameraLockState(true);
        MissionLobbyComponentOnUpdateTeams();
        MissionLobbyComponentOnFriendsUpdated();
    }

    private void OnChangeTeamTo(Team targetTeam)
    {
        _multiplayerTeamSelectComponent.ChangeTeam(targetTeam);
    }

    private void OnMyTeamChanged()
    {
        OnClose();
    }

    private void OnAutoassign()
    {
        _multiplayerTeamSelectComponent.AutoAssignTeam(GameNetwork.MyPeer);
    }

    private void MissionLobbyComponentOnSelectingTeam(List<Team> disabledTeams)
    {
        _disabledTeams = disabledTeams;
        _toOpen = true;
    }

    private void MissionLobbyComponentOnFriendsUpdated()
    {
        if (!_isActive)
        {
            return;
        }

        IEnumerable<MissionPeer> friendsTeamOne = _multiplayerTeamSelectComponent.GetFriendsForTeam(Mission.AttackerTeam).Select(x => x.GetComponent<MissionPeer>());
        IEnumerable<MissionPeer> friendsTeamTwo = _multiplayerTeamSelectComponent.GetFriendsForTeam(Mission.DefenderTeam).Select(x => x.GetComponent<MissionPeer>());
        _dataSource?.RefreshFriendsPerTeam(friendsTeamOne, friendsTeamTwo);
    }

    private void MissionLobbyComponentOnUpdateTeams()
    {
        if (!_isActive)
        {
            return;
        }

        List<Team> disabledTeams = _multiplayerTeamSelectComponent.GetDisabledTeams();
        _dataSource?.RefreshDisabledTeams(disabledTeams);
        int playerCountForTeam = _multiplayerTeamSelectComponent.GetPlayerCountForTeam(Mission.AttackerTeam);
        int playerCountForTeam2 = _multiplayerTeamSelectComponent.GetPlayerCountForTeam(Mission.DefenderTeam);
        int intValue = MultiplayerOptions.OptionType.NumberOfBotsTeam1.GetIntValue();
        int intValue2 = MultiplayerOptions.OptionType.NumberOfBotsTeam2.GetIntValue();
        _dataSource?.RefreshPlayerAndBotCount(playerCountForTeam, playerCountForTeam2, intValue, intValue2);
    }

    private void OnScoreboardToggled(bool isEnabled)
    {
        if (isEnabled)
        {
            GauntletLayer? gauntletLayer = _gauntletLayer;
            if (gauntletLayer == null)
            {
                return;
            }

            gauntletLayer.InputRestrictions.ResetInputRestrictions();
        }
        else
        {
            GauntletLayer? gauntletLayer2 = _gauntletLayer;
            if (gauntletLayer2 == null)
            {
                return;
            }

            gauntletLayer2.InputRestrictions.SetInputRestrictions();
        }
    }

    private void OnMyClientSynchronized()
    {
        _isSynchronized = true;
    }

    private GauntletLayer? _gauntletLayer;

    private CrpgTeamSelectVm? _dataSource;

    private MissionNetworkComponent _missionNetworkComponent = null!;

    private MultiplayerTeamSelectComponent _multiplayerTeamSelectComponent = null!;

    private MissionGauntletMultiplayerScoreboard? _scoreboardGauntletComponent;

    private MissionGauntletClassLoadout? _classLoadoutGauntletComponent;

    private MissionLobbyComponent _lobbyComponent = null!;

    private List<Team>? _disabledTeams;

    private bool _toOpen;

    private bool _isSynchronized;

    private bool _isActive;
}
