using System;
using System.Linq;
using Crpg.Module.GUI.HudExtension;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Missions.Multiplayer;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Scoreboard;
using TaleWorlds.ObjectSystem;
using static TaleWorlds.MountAndBlade.MissionScoreboardComponent;

namespace Crpg.Module.Gui;

public class CrpgScoreboardEndOfBattleVM : ViewModel
{
    private MissionRepresentativeBase? missionRep
    {
        get
        {
            NetworkCommunicator myPeer = GameNetwork.MyPeer;
            if (myPeer == null)
            {
                return null;
            }

            VirtualPlayer virtualPlayer = myPeer.VirtualPlayer;
            if (virtualPlayer == null)
            {
                return null;
            }

            return virtualPlayer.GetComponent<MissionRepresentativeBase>();
        }
    }

    public CrpgScoreboardEndOfBattleVM(Mission mission, MissionScoreboardComponent missionScoreboardComponent, bool isSingleTeam)
    {
        _missionScoreboardComponent = missionScoreboardComponent;
        _gameMode = mission.GetMissionBehavior<MissionMultiplayerGameModeBaseClient>();
        _lobbyComponent = mission.GetMissionBehavior<MissionLobbyComponent>();
        _lobbyComponent.OnPostMatchEnded += OnPostMatchEnded;
        _isSingleTeam = isSingleTeam;
        RefreshValues();
        var customBanners = Mission.Current.GetMissionBehavior<CrpgCustomTeamBannersAndNamesClient>();
        if (customBanners != null)
        {
            customBanners.BannersChanged += HandleBannerChange;
        }

    }

    private void HandleBannerChange(string attackerBanner, string defenderBanner, string attackerName, string defenderName)
    {
        AllyBanner = new(GameNetwork.MyPeer.GetComponent<MissionPeer>()?.Team?.Side == BattleSideEnum.Attacker ? new Banner(attackerBanner) : new Banner(defenderBanner), true);
        EnemyBanner = new(GameNetwork.MyPeer.GetComponent<MissionPeer>()?.Team?.Side == BattleSideEnum.Attacker ? new Banner(defenderBanner) : new Banner(attackerBanner), true);
    }

    public override void RefreshValues()
    {
        base.RefreshValues();
        CountdownTitle = new TextObject("{=wGjQgQlY}Next Game begins in:", null).ToString();
        Header = new TextObject("{=HXxNfncd}End of Battle", null).ToString();
        MPEndOfBattleSideVM allySide = AllySide;
        allySide?.RefreshValues();

        MPEndOfBattleSideVM enemySide = EnemySide;
        if (enemySide == null)
        {
            return;
        }

        enemySide.RefreshValues();
    }

    public override void OnFinalize()
    {
        base.OnFinalize();
        _lobbyComponent.OnPostMatchEnded -= OnPostMatchEnded;
    }

    public void Tick(float dt)
    {
        Countdown = MathF.Ceiling(_gameMode.RemainingTime);
    }

    private void OnPostMatchEnded()
    {
        OnFinalRoundEnded();
    }

    private void OnFinalRoundEnded()
    {
        if (_isSingleTeam)
        {
            return;
        }

        IsAvailable = true;
        InitSides();
        MissionScoreboardComponent missionScoreboardComponent = _missionScoreboardComponent;
        BattleSideEnum battleSideEnum = (missionScoreboardComponent != null) ? missionScoreboardComponent.GetMatchWinnerSide() : BattleSideEnum.None;
        if (battleSideEnum == _enemyBattleSide)
        {
            BattleResult = 0;
            ResultText = GameTexts.FindText("str_defeat", null).ToString();
            return;
        }

        if (battleSideEnum == _allyBattleSide)
        {
            BattleResult = 1;
            ResultText = GameTexts.FindText("str_victory", null).ToString();
            return;
        }

        BattleResult = 2;
        ResultText = GameTexts.FindText("str_draw", null).ToString();
    }

private void InitSides()
    {
        _allyBattleSide = BattleSideEnum.Attacker;
        _enemyBattleSide = BattleSideEnum.Defender;
        NetworkCommunicator myPeer = GameNetwork.MyPeer;
        MissionPeer? missionPeer = myPeer?.GetComponent<MissionPeer>();
        if (missionPeer != null)
        {
            Team team = missionPeer.Team;
            if (team != null && team.Side == BattleSideEnum.Defender)
            {
                _allyBattleSide = BattleSideEnum.Defender;
                _enemyBattleSide = BattleSideEnum.Attacker;
            }
        }

        MissionScoreboardSide missionScoreboardSide = _missionScoreboardComponent.Sides.FirstOrDefault((MissionScoreboardSide s) => s != null && s.Side == _allyBattleSide);
        MissionScoreboardSide missionScoreboardSide2 = _missionScoreboardComponent.Sides.FirstOrDefault((MissionScoreboardSide s) => s != null && s.Side == _enemyBattleSide);

        string text = (missionScoreboardSide != null && missionScoreboardSide.Side == BattleSideEnum.Attacker) ? MultiplayerOptions.OptionType.CultureTeam1.GetStrValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions) : MultiplayerOptions.OptionType.CultureTeam2.GetStrValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions);
        string text2 = (missionScoreboardSide2 != null && missionScoreboardSide2.Side == BattleSideEnum.Attacker) ? MultiplayerOptions.OptionType.CultureTeam1.GetStrValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions) : MultiplayerOptions.OptionType.CultureTeam2.GetStrValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions);
        BasicCultureObject? basicCultureObject = string.IsNullOrEmpty(text) ? null : MBObjectManager.Instance.GetObject<BasicCultureObject>(text);
        BasicCultureObject? basicCultureObject2 = string.IsNullOrEmpty(text2) ? null : MBObjectManager.Instance.GetObject<BasicCultureObject>(text2);

        MultiplayerBattleColors multiplayerBattleColors = MultiplayerBattleColors.CreateWith(basicCultureObject, basicCultureObject2);
        if (missionScoreboardSide != null)
        {
            string objectName = (missionScoreboardSide.Side == BattleSideEnum.Attacker) ? MultiplayerOptions.OptionType.CultureTeam1.GetStrValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions) : MultiplayerOptions.OptionType.CultureTeam2.GetStrValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions);
            AllySide = new MPEndOfBattleSideVM(_missionScoreboardComponent, missionScoreboardSide, multiplayerBattleColors.AttackerColors);
        }

        if (missionScoreboardSide2 != null)
        {
            string objectName2 = (missionScoreboardSide2.Side == BattleSideEnum.Attacker) ? MultiplayerOptions.OptionType.CultureTeam1.GetStrValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions) : MultiplayerOptions.OptionType.CultureTeam2.GetStrValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions);
            EnemySide = new MPEndOfBattleSideVM(_missionScoreboardComponent, missionScoreboardSide, multiplayerBattleColors.DefenderColors);
        }


    }

    [DataSourceProperty]
    public bool IsAvailable
    {
        get
        {
            return _isAvailable;
        }
        set
        {
            if (value != _isAvailable)
            {
                _isAvailable = value;
                OnPropertyChangedWithValue(value, "IsAvailable");
            }
        }
    }

    [DataSourceProperty]
    public string CountdownTitle
    {
        get
        {
            return _countdownTitle;
        }
        set
        {
            if (value != _countdownTitle)
            {
                _countdownTitle = value;
                OnPropertyChangedWithValue(value, "CountdownTitle");
            }
        }
    }

    [DataSourceProperty]
    public int Countdown
    {
        get
        {
            return _countdown;
        }
        set
        {
            if (value != _countdown)
            {
                _countdown = value;
                OnPropertyChangedWithValue(value, "Countdown");
            }
        }
    }

    [DataSourceProperty]
    public string Header
    {
        get
        {
            return _header;
        }
        set
        {
            if (value != _header)
            {
                _header = value;
                OnPropertyChangedWithValue(value, "Header");
            }
        }
    }

    [DataSourceProperty]
    public int BattleResult
    {
        get
        {
            return _battleResult;
        }
        set
        {
            if (value != _battleResult)
            {
                _battleResult = value;
                OnPropertyChangedWithValue(value, "BattleResult");
            }
        }
    }

    [DataSourceProperty]
    public string ResultText
    {
        get
        {
            return _resultText;
        }
        set
        {
            if (value != _resultText)
            {
                _resultText = value;
                OnPropertyChangedWithValue(value, "ResultText");
            }
        }
    }

    [DataSourceProperty]
    public MPEndOfBattleSideVM AllySide
    {
        get
        {
            return _allySide;
        }
        set
        {
            if (value != _allySide)
            {
                _allySide = value;
                OnPropertyChangedWithValue(value, "AllySide");
            }
        }
    }

    [DataSourceProperty]
    public MPEndOfBattleSideVM EnemySide
    {
        get
        {
            return _enemySide;
        }
        set
        {
            if (value != _enemySide)
            {
                _enemySide = value;
                OnPropertyChangedWithValue(value, "EnemySide");
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

    private MissionScoreboardComponent _missionScoreboardComponent;

    private MissionMultiplayerGameModeBaseClient _gameMode;

    private MissionLobbyComponent _lobbyComponent;

    private bool _isSingleTeam;

    private BattleSideEnum _allyBattleSide = default!;

    private BattleSideEnum _enemyBattleSide = default!;

    private bool _isAvailable;

    private string _countdownTitle = default!;

    private int _countdown;

    private string _header = default!;

    private int _battleResult;

    private string _resultText = default!;

    private MPEndOfBattleSideVM _allySide = default!;

    private MPEndOfBattleSideVM _enemySide = default!;

    private BannerImageIdentifierVM? _allyBanner;

    private BannerImageIdentifierVM? _enemyBanner;
}
