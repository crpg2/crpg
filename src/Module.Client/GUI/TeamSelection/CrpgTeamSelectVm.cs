using JetBrains.Annotations;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using MathF = TaleWorlds.Library.MathF;

namespace Crpg.Module.GUI.TeamSelection;

public class CrpgTeamSelectVm : ViewModel
{
    private readonly Action _onClose;
    private readonly Action _onAutoAssign;
    private readonly MissionMultiplayerGameModeBaseClient _gameMode;
    private readonly MissionPeer _missionPeer = null!;
    private readonly string _gamemodeStr;
    private string _teamSelectTitle = null!;
    private bool _isRoundCountdownAvailable;
    private string _remainingRoundTime = null!;
    private string _gamemodeLbl = null!;
    private string _autoassignLbl = null!;
    private bool _isCancelDisabled;
    private CrpgTeamSelectInstanceVm? _team1;
    private CrpgTeamSelectInstanceVm? _team2;
    private CrpgTeamSelectInstanceVm? _teamSpectators;

    public CrpgTeamSelectVm(Mission mission, Action<Team> onChangeTeamTo, Action onAutoAssign, Action onClose,
        Mission.TeamCollection teams, string gamemode)
    {
        _onClose = onClose;
        _onAutoAssign = onAutoAssign;
        _gamemodeStr = gamemode;

        _gameMode = mission.GetMissionBehavior<MissionMultiplayerGameModeBaseClient>();
        MissionScoreboardComponent missionBehavior = mission.GetMissionBehavior<MissionScoreboardComponent>();

        IsRoundCountdownAvailable = _gameMode.IsGameModeUsingRoundCountdown;

        Team spectatorTeam = teams.First(t => t.Side == BattleSideEnum.None);
        TeamSpectators = new CrpgTeamSelectInstanceVm(missionBehavior, spectatorTeam, null, null, onChangeTeamTo, false,
            new TextObject("{=pSheKLB4}Spectator").ToString());

        Team team1 = teams.First(t => t.Side == BattleSideEnum.Attacker);
        BasicCultureObject culture1 =
            MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam2
                .GetStrValue());

        var banners = Mission.Current.GetMissionBehavior<CrpgCustomTeamBannersAndNamesClient>();
        Banner attackerBanner = new(string.Empty);
        Banner defenderBanner = new(string.Empty);
        if (Mission.Current.Teams.Count > 0)
        {
            attackerBanner = new(Mission.Current.Teams.Attacker.Banner);
            defenderBanner = new(Mission.Current.Teams.Defender.Banner);
        }

        Team1 = new CrpgTeamSelectInstanceVm(
            missionBehavior,
            team1,
            culture1,
            banners?.AttackerBannerCode != null ? new(new Banner(banners.AttackerBannerCode)) : new(attackerBanner),
            onChangeTeamTo,
            false,
            banners?.AttackerName ?? string.Empty);

        Team2 = new CrpgTeamSelectInstanceVm(
            missionBehavior,
            team1,
            culture1,
            banners?.DefenderBannerCode != null ? new(new Banner(banners.DefenderBannerCode)) : new(defenderBanner),
            onChangeTeamTo,
            false,
            banners?.DefenderName ?? string.Empty);

        if (GameNetwork.IsMyPeerReady)
        {
            _missionPeer = GameNetwork.MyPeer.GetComponent<MissionPeer>();
            IsCancelDisabled = _missionPeer.Team == null;
        }

        RefreshValues();
    }

    [DataSourceProperty]
    public CrpgTeamSelectInstanceVm? Team1
    {
        get
        {
            return _team1;
        }
        set
        {
            if (value != _team1)
            {
                _team1 = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public CrpgTeamSelectInstanceVm? Team2
    {
        get
        {
            return _team2;
        }
        set
        {
            if (value != _team2)
            {
                _team2 = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public CrpgTeamSelectInstanceVm? TeamSpectators
    {
        get
        {
            return _teamSpectators;
        }
        set
        {
            if (value != _teamSpectators)
            {
                _teamSpectators = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public string TeamSelectTitle
    {
        get
        {
            return _teamSelectTitle;
        }
        set
        {
            _teamSelectTitle = value;
            OnPropertyChangedWithValue(value);
        }
    }

    [DataSourceProperty]
    public bool IsRoundCountdownAvailable
    {
        get
        {
            return _isRoundCountdownAvailable;
        }
        set
        {
            if (value != _isRoundCountdownAvailable)
            {
                _isRoundCountdownAvailable = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public string RemainingRoundTime
    {
        get
        {
            return _remainingRoundTime;
        }
        set
        {
            if (value != _remainingRoundTime)
            {
                _remainingRoundTime = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public string GamemodeLbl
    {
        get
        {
            return _gamemodeLbl;
        }
        set
        {
            _gamemodeLbl = value;
            OnPropertyChangedWithValue(value);
        }
    }

    [DataSourceProperty]
    public string AutoassignLbl
    {
        get
        {
            return _autoassignLbl;
        }
        set
        {
            _autoassignLbl = value;
            OnPropertyChangedWithValue(value);
        }
    }

    [DataSourceProperty]
    public bool IsCancelDisabled
    {
        get
        {
            return _isCancelDisabled;
        }
        set
        {
            _isCancelDisabled = value;
            OnPropertyChangedWithValue(value);
        }
    }

    public override void RefreshValues()
    {
        base.RefreshValues();
        AutoassignLbl = new TextObject("{=bON4Kn6B}Auto Assign").ToString();
        TeamSelectTitle = new TextObject("{=aVixswW5}Team Selection").ToString();
        GamemodeLbl = GameTexts.FindText("str_multiplayer_official_game_type_name", _gamemodeStr).ToString();
        Team1?.RefreshValues();
        Team2?.RefreshValues();
        _teamSpectators?.RefreshValues();
    }

    public void Tick(float dt)
    {
        RemainingRoundTime = TimeSpan.FromSeconds(MathF.Ceiling(_gameMode.RemainingTime)).ToString("mm':'ss");
    }

    public void RefreshDisabledTeams(List<Team>? disabledTeams)
    {
        if (disabledTeams == null)
        {
            TeamSpectators?.SetIsDisabled(false, false);
            Team1?.SetIsDisabled(false, false);
            Team2?.SetIsDisabled(false, false);
        }
        else
        {
            if (TeamSpectators != null)
            {
                bool disabledForBalance = TeamSpectators.Team != null && disabledTeams.Contains(TeamSpectators.Team);
                TeamSpectators.SetIsDisabled(false, disabledForBalance);
            }

            if (Team1 != null)
            {
                bool isCurrentTeam = Team1.Team == _missionPeer.Team;
                bool disabledForBalance = Team1.Team != null && disabledTeams.Contains(Team1.Team);
                Team1.SetIsDisabled(isCurrentTeam, disabledForBalance);
            }

            if (Team2 != null)
            {
                bool isCurrentTeam = Team2.Team == _missionPeer.Team;
                bool disabledForBalance = Team2.Team != null && disabledTeams.Contains(Team2.Team);
                Team2.SetIsDisabled(isCurrentTeam, disabledForBalance);
            }
        }
    }

    public void RefreshPlayerAndBotCount(int playersCountOne, int playersCountTwo, int botsCountOne, int botsCountTwo)
    {
        if (Team1 != null)
        {
            MBTextManager.SetTextVariable("PLAYER_COUNT", playersCountOne.ToString());
            MBTextManager.SetTextVariable("BOT_COUNT", botsCountOne.ToString());
            Team1.DisplayedSecondary = new TextObject("{=Etjqamlh}{PLAYER_COUNT} Players").ToString();
            Team1.DisplayedSecondarySub = new TextObject("{=eCOJSSUH}({BOT_COUNT} Bots)").ToString();
        }

        if (Team2 != null)
        {
            MBTextManager.SetTextVariable("PLAYER_COUNT", playersCountTwo.ToString());
            MBTextManager.SetTextVariable("BOT_COUNT", botsCountTwo.ToString());
            Team2.DisplayedSecondary = new TextObject("{=Etjqamlh}{PLAYER_COUNT} Players").ToString();
            Team2.DisplayedSecondarySub = new TextObject("{=eCOJSSUH}({BOT_COUNT} Bots)").ToString();
        }
    }

    public void RefreshFriendsPerTeam(IEnumerable<MissionPeer> friendsTeamOne, IEnumerable<MissionPeer> friendsTeamTwo)
    {
        Team1?.RefreshFriends(friendsTeamOne);
        Team2?.RefreshFriends(friendsTeamTwo);
    }

    [UsedImplicitly]
    public void ExecuteCancel()
    {
        _onClose();
    }

    [UsedImplicitly]
    public void ExecuteAutoAssign()
    {
        _onAutoAssign();
    }
}
