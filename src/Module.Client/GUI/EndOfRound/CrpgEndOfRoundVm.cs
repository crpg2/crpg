using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Missions.Multiplayer;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.EndOfRound;
using TaleWorlds.ObjectSystem;

namespace Crpg.Module.GUI.EndOfRound;

public class CrpgEndOfRoundVm : ViewModel
{
    private readonly MissionScoreboardComponent _scoreboardComponent;
    private readonly IRoundComponent _multiplayerRoundComponent;
    private readonly string _victoryText;
    private readonly string _defeatText;
    private readonly TextObject _roundEndReasonAllyTeamSideDepletedTextObject;
    private readonly TextObject _roundEndReasonEnemyTeamSideDepletedTextObject;
    private readonly TextObject _roundEndReasonAllyTeamRoundTimeEndedTextObject;
    private readonly TextObject _roundEndReasonEnemyTeamRoundTimeEndedTextObject;
    private readonly TextObject _roundEndReasonAllyTeamGameModeSpecificEndedTextObject;
    private readonly TextObject _roundEndReasonEnemyTeamGameModeSpecificEndedTextObject;
    private readonly TextObject _roundEndReasonRoundTimeEndedWithDrawTextObject;
    private bool _isShown;
    private string _title = string.Empty;
    private string _description = string.Empty;
    private string _cultureId = string.Empty;
    private bool _isRoundWinner;
    private MultiplayerEndOfRoundSideVM _attackerSide = null!;
    private MultiplayerEndOfRoundSideVM _defenderSide = null!;
    private BannerImageIdentifierVM? _allyBanner;
    private BannerImageIdentifierVM? _enemyBanner;

    public CrpgEndOfRoundVm(MissionScoreboardComponent scoreboardComponent,
        MissionLobbyComponent missionLobbyComponent, IRoundComponent multiplayerRoundComponent)
    {
        _scoreboardComponent = scoreboardComponent;
        _multiplayerRoundComponent = multiplayerRoundComponent;
        _victoryText = new TextObject("{=RCuCoVgd}ROUND WON").ToString();
        _defeatText = new TextObject("{=Dbkx4v90}ROUND LOST").ToString();
        _roundEndReasonAllyTeamSideDepletedTextObject = new TextObject("{=9M4G8DDd}Your team was wiped out");
        _roundEndReasonEnemyTeamSideDepletedTextObject =
            new TextObject("{=jPXglGWT}Enemy team was wiped out");
        _roundEndReasonAllyTeamRoundTimeEndedTextObject =
            new TextObject("{=x1HZy70i}Your team had the upper hand at timeout");
        _roundEndReasonEnemyTeamRoundTimeEndedTextObject =
            new TextObject("{=Dc3fFblo}Enemy team had the upper hand at timeout");
        _roundEndReasonRoundTimeEndedWithDrawTextObject =
            new TextObject("{=i3dJSlD0}No team had the upper hand at timeout");
        if (missionLobbyComponent.MissionType == MultiplayerGameType.Battle ||
            missionLobbyComponent.MissionType == MultiplayerGameType.Captain ||
            missionLobbyComponent.MissionType == MultiplayerGameType.Skirmish)
        {
            _roundEndReasonAllyTeamGameModeSpecificEndedTextObject =
                new TextObject("{=xxuzZJ3G}Your team ran out of morale");
            _roundEndReasonEnemyTeamGameModeSpecificEndedTextObject =
                new TextObject("{=c6c9eYrD}Enemy team ran out of morale");
        }
        else
        {
            _roundEndReasonAllyTeamGameModeSpecificEndedTextObject = new TextObject(string.Empty);
            _roundEndReasonEnemyTeamGameModeSpecificEndedTextObject = new TextObject(string.Empty);
        }

        AttackerSide = new MultiplayerEndOfRoundSideVM();
        DefenderSide = new MultiplayerEndOfRoundSideVM();
        var customBanners = Mission.Current.GetMissionBehavior<CrpgCustomTeamBannersAndNamesClient>();
        if (customBanners != null)
        {
            customBanners.BannersChanged += HandleBannerChange;
        }
    }

    public override void RefreshValues()
    {
        base.RefreshValues();
        Refresh();
    }

    public void Refresh()
    {
        NetworkCommunicator myPeer = GameNetwork.MyPeer;
        MissionPeer missionPeer = myPeer.GetComponent<MissionPeer>();
        BattleSideEnum allyBattleSide = missionPeer.Team?.Side ?? BattleSideEnum.None;

        BattleSideEnum battleSideEnum = allyBattleSide == BattleSideEnum.Attacker
            ? BattleSideEnum.Defender
            : BattleSideEnum.Attacker;
        BasicCultureObject @object = MBObjectManager.Instance.GetObject<BasicCultureObject>(
            MultiplayerOptions.OptionType.CultureTeam1.GetStrValue());
        BasicCultureObject object2 = MBObjectManager.Instance.GetObject<BasicCultureObject>(
            MultiplayerOptions.OptionType.CultureTeam2.GetStrValue());
        MissionScoreboardComponent.MissionScoreboardSide missionScoreboardSide =
            _scoreboardComponent.Sides.First(s =>
                s != null && s.Side == BattleSideEnum.Attacker);
        MissionScoreboardComponent.MissionScoreboardSide missionScoreboardSide2 =
            _scoreboardComponent.Sides.First(s =>
                s != null && s.Side == BattleSideEnum.Defender);
        bool isWinner = _multiplayerRoundComponent.RoundWinner == BattleSideEnum.Attacker;
        bool isWinner2 = _multiplayerRoundComponent.RoundWinner == BattleSideEnum.Defender;
        Team? team = missionPeer.Team;
        if (team != null && team.Side == BattleSideEnum.Attacker)
        {
            AttackerSide.SetData(@object, missionScoreboardSide.SideScore, isWinner,
                new MultiplayerBattleColors.MultiplayerCultureColorInfo(@object, false));

            DefenderSide.SetData(object2, missionScoreboardSide2.SideScore, isWinner2,
                new MultiplayerBattleColors.MultiplayerCultureColorInfo(object2, @object == object2));
        }
        else
        {
            DefenderSide.SetData(@object, missionScoreboardSide.SideScore, isWinner,
                new MultiplayerBattleColors.MultiplayerCultureColorInfo(object2, @object == object2));
            AttackerSide.SetData(object2, missionScoreboardSide2.SideScore, isWinner2,
                new MultiplayerBattleColors.MultiplayerCultureColorInfo(object2, false));
        }

        if (_scoreboardComponent.Sides.FirstOrDefault(s => s != null && s.Side == allyBattleSide) != null)
        {
            bool flag = false;
            if (_multiplayerRoundComponent.RoundWinner == allyBattleSide)
            {
                IsRoundWinner = true;
                Title = _victoryText;
            }
            else if (_multiplayerRoundComponent.RoundWinner == battleSideEnum)
            {
                IsRoundWinner = false;
                Title = _defeatText;
            }
            else
            {
                flag = true;
            }

            RoundEndReason roundEndReason = _multiplayerRoundComponent.RoundEndReason;
            if (roundEndReason == RoundEndReason.SideDepleted)
            {
                Description = IsRoundWinner
                    ? _roundEndReasonEnemyTeamSideDepletedTextObject.ToString()
                    : _roundEndReasonAllyTeamSideDepletedTextObject.ToString();
                return;
            }

            if (roundEndReason == RoundEndReason.GameModeSpecificEnded)
            {
                Description = IsRoundWinner
                    ? _roundEndReasonEnemyTeamGameModeSpecificEndedTextObject.ToString()
                    : _roundEndReasonAllyTeamGameModeSpecificEndedTextObject.ToString();
                return;
            }

            if (roundEndReason == RoundEndReason.RoundTimeEnded)
            {
                Description = IsRoundWinner
                    ? _roundEndReasonAllyTeamRoundTimeEndedTextObject.ToString()
                    : flag
                        ? _roundEndReasonRoundTimeEndedWithDrawTextObject.ToString()
                        : _roundEndReasonEnemyTeamRoundTimeEndedTextObject.ToString();
            }
        }
    }

    private void HandleBannerChange(string attackerBanner, string defenderBanner, string attackerName, string defenderName)
    {
        AllyBanner = new(GameNetwork.MyPeer.GetComponent<MissionPeer>()?.Team?.Side == BattleSideEnum.Attacker ? new Banner(attackerBanner) : new Banner(defenderBanner), true);
        EnemyBanner = new(GameNetwork.MyPeer.GetComponent<MissionPeer>()?.Team?.Side == BattleSideEnum.Attacker ? new Banner(defenderBanner) : new Banner(attackerBanner), true);
    }

    [DataSourceProperty]
    public bool IsShown
    {
        get
        {
            return _isShown;
        }
        set
        {
            if (value != _isShown)
            {
                _isShown = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public string Title
    {
        get
        {
            return _title;
        }
        set
        {
            if (value != _title)
            {
                _title = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public string Description
    {
        get
        {
            return _description;
        }
        set
        {
            if (value != _description)
            {
                _description = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public string CultureId
    {
        get
        {
            return _cultureId;
        }
        set
        {
            if (value != _cultureId)
            {
                _cultureId = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public bool IsRoundWinner
    {
        get
        {
            return _isRoundWinner;
        }
        set
        {
            if (value != _isRoundWinner)
            {
                _isRoundWinner = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public MultiplayerEndOfRoundSideVM AttackerSide
    {
        get
        {
            return _attackerSide;
        }
        set
        {
            if (value != _attackerSide)
            {
                _attackerSide = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public MultiplayerEndOfRoundSideVM DefenderSide
    {
        get
        {
            return _defenderSide;
        }
        set
        {
            if (value != _defenderSide)
            {
                _defenderSide = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public BannerImageIdentifierVM? AllyBanner
    {
        get
        {
            return _allyBanner;
        }
        set
        {
            if (value == _allyBanner)
            {
                return;
            }

            _allyBanner = value;
            OnPropertyChangedWithValue(value);
        }
    }

    [DataSourceProperty]
    public BannerImageIdentifierVM? EnemyBanner
    {
        get
        {
            return _enemyBanner;
        }
        set
        {
            if (value == _enemyBanner)
            {
                return;
            }

            _enemyBanner = value;
            OnPropertyChangedWithValue(value);
        }
    }
}
