using System;
using System.Collections.Generic;
using Crpg.Module.Common.Commander;
using Crpg.Module.GUI.HudExtension;
using Crpg.Module.GUI.Scoreboard;
using Crpg.Module.Modes.Conquest;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Scoreboard;
using TaleWorlds.ObjectSystem;
namespace Crpg.Module.Gui;

public class CrpgScoreboardSideVM : ViewModel
{
    private const string _avatarHeaderId = "avatar";

    private readonly MissionScoreboardComponent.MissionScoreboardSide _missionScoreboardSide;

    private readonly Dictionary<MissionPeer, MissionScoreboardPlayerVM> _playersMap;

    private readonly int _avatarHeaderIndex;

    private MissionScoreboardPlayerVM _bot;

    private Action<MissionScoreboardPlayerVM> _executeActivate;

    private List<string> _irregularHeaderIDs = new()
    {
        "name",
        "avatar",
        "score",
        "kill",
        "assist",
    };

    private MBBindingList<MissionScoreboardPlayerVM> _players = default!;

    private MBBindingList<CrpgMissionScoreboardHeaderItemVM> _entryProperties = default!;

    private MissionScoreboardPlayerSortControllerVM _playerSortController = default!;

    private readonly CrpgCommanderBehaviorClient? _commanderClient;

    private bool _isSingleSide;

    private bool _isSecondSide;

    private bool _useSecondary;

    private bool _showAttackerOrDefenderIcons;
    private bool _showCommanders;

    private bool _isAttacker;

    private int _roundsWon;

    private string _allyTeamName = default!;

    private string _enemyTeamName = default!;

    private string _cultureId = default!;

    private string _teamColor = default!;

    private string _playersText = default!;
    private string _commanderText = default!;

    private BannerImageIdentifierVM? _allyBanner;
    private BannerImageIdentifierVM? _enemyBanner;
    public CrpgScoreboardSideVM(MissionScoreboardComponent.MissionScoreboardSide missionScoreboardSide, Action<MissionScoreboardPlayerVM> executeActivate, bool isSingleSide, bool isSecondSide)
    {
        _executeActivate = executeActivate;
        _missionScoreboardSide = missionScoreboardSide;
        _playersMap = new Dictionary<MissionPeer, MissionScoreboardPlayerVM>();
        Players = new MBBindingList<MissionScoreboardPlayerVM>();
        PlayerSortController = new MissionScoreboardPlayerSortControllerVM(ref _players);
        _avatarHeaderIndex = missionScoreboardSide.GetHeaderIds().IndexOf("avatar");
        int score = missionScoreboardSide.GetScore(null);
        string[] valuesOf = missionScoreboardSide.GetValuesOf(null);
        string[] headerIds = missionScoreboardSide.GetHeaderIds();
        _bot = new MissionScoreboardPlayerVM(valuesOf, headerIds, score, _executeActivate);

        foreach (MissionPeer peer in missionScoreboardSide.Players)
        {
            AddPlayer(peer);
        }

        UpdateBotAttributes();
        UpdateRoundAttributes();
        string text = (_missionScoreboardSide.Side == BattleSideEnum.Attacker) ? MultiplayerOptions.OptionType.CultureTeam1.GetStrValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions) : MultiplayerOptions.OptionType.CultureTeam2.GetStrValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions);
        BasicCultureObject @object = MBObjectManager.Instance.GetObject<BasicCultureObject>(text);
        UseSecondary = _missionScoreboardSide.Side == BattleSideEnum.Defender;
        IsSingleSide = isSingleSide;
        IsSecondSide = isSecondSide;
        CultureId = text;
        TeamColor = "0x" + @object.Color2.ToString("X");
        ShowAttackerOrDefenderIcons = Mission.Current.HasMissionBehavior<CrpgConquestClient>();
        _commanderClient = Mission.Current.GetMissionBehavior<CrpgCommanderBehaviorClient>();
        if (_commanderClient != null)
        {
            ShowCommanders = true;
            _commanderClient.OnCommanderUpdated += OnCommanderUpdated;
        }
        IsAttacker = missionScoreboardSide.Side == BattleSideEnum.Attacker;
        RefreshValues();
        NetworkCommunicator.OnPeerAveragePingUpdated += OnPeerPingUpdated;
        ManagedOptions.OnManagedOptionChanged = (ManagedOptions.OnManagedOptionChangedDelegate)Delegate.Combine(ManagedOptions.OnManagedOptionChanged, new ManagedOptions.OnManagedOptionChangedDelegate(OnManagedOptionChanged));
        var customBanner = Mission.Current.GetMissionBehavior<CrpgCustomTeamBannersAndNamesClient>();
        if (customBanner != null)
        {
            AllyBanner = new(GameNetwork.MyPeer.GetComponent<MissionPeer>()?.Team?.Side == BattleSideEnum.Attacker ? new Banner(customBanner.AttackerBannerCode) : new Banner(customBanner.DefenderBannerCode), true);
            EnemyBanner = new(GameNetwork.MyPeer.GetComponent<MissionPeer>()?.Team?.Side == BattleSideEnum.Attacker ? new Banner(customBanner.DefenderBannerCode) : new Banner(customBanner.AttackerBannerCode), true);
            AllyTeamName = GameNetwork.MyPeer.GetComponent<MissionPeer>()?.Team?.Side == BattleSideEnum.Attacker ? customBanner.AttackerName : customBanner.DefenderName;
            EnemyTeamName = GameNetwork.MyPeer.GetComponent<MissionPeer>()?.Team?.Side == BattleSideEnum.Attacker ? customBanner.DefenderName : customBanner.AttackerName;
            customBanner.BannersChanged += HandleBannerChange;
        }
        else if (Mission.Current.Teams.Count > 0)
        {
            AllyBanner = new(GameNetwork.MyPeer.GetComponent<MissionPeer>()?.Team?.Side == BattleSideEnum.Attacker ? Mission.Current.Teams.Attacker?.Banner : Mission.Current.Teams.Defender?.Banner, true);
            EnemyBanner = new(GameNetwork.MyPeer.GetComponent<MissionPeer>()?.Team?.Side == BattleSideEnum.Attacker ? Mission.Current.Teams.Defender?.Banner : Mission.Current.Teams.Attacker?.Banner, true);
        }
    }

    private void HandleBannerChange(string attackerBanner, string defenderBanner, string attackerName, string defenderName)
    {
        AllyBanner = new(GameNetwork.MyPeer.GetComponent<MissionPeer>()?.Team?.Side == BattleSideEnum.Attacker ? new Banner(attackerBanner) : new Banner(defenderBanner), true);
        EnemyBanner = new(GameNetwork.MyPeer.GetComponent<MissionPeer>()?.Team?.Side == BattleSideEnum.Attacker ? new Banner(defenderBanner) : new Banner(attackerBanner), true);
        AllyTeamName = GameNetwork.MyPeer.GetComponent<MissionPeer>()?.Team?.Side == BattleSideEnum.Attacker ? attackerName : defenderName;
        EnemyTeamName = GameNetwork.MyPeer.GetComponent<MissionPeer>()?.Team?.Side == BattleSideEnum.Attacker ? defenderName : attackerName;
    }

    public override void RefreshValues()
    {
        base.RefreshValues();

        BasicCultureObject @object = MBObjectManager.Instance.GetObject<BasicCultureObject>((_missionScoreboardSide.Side == BattleSideEnum.Attacker) ? MultiplayerOptions.OptionType.CultureTeam1.GetStrValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions) : MultiplayerOptions.OptionType.CultureTeam2.GetStrValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions));

        EntryProperties = new MBBindingList<CrpgMissionScoreboardHeaderItemVM>();
        string[] headerIds = _missionScoreboardSide.GetHeaderIds();
        string[] headerNames = _missionScoreboardSide.GetHeaderNames();
        for (int i = 0; i < headerIds.Length; i++)
        {
            EntryProperties.Add(new CrpgMissionScoreboardHeaderItemVM(this, headerIds[i], headerNames[i], headerIds[i] == "avatar", _irregularHeaderIDs.Contains(headerIds[i])));
        }

        UpdatePlayersText();
        UpdateCommanderText();

        MissionScoreboardPlayerSortControllerVM playerSortController = PlayerSortController;
        if (playerSortController == null)
        {
            return;
        }

        playerSortController.RefreshValues();
    }

    public void Tick(float dt)
    {
        foreach (MissionScoreboardPlayerVM missionScoreboardPlayerVM in Players)
        {
            missionScoreboardPlayerVM.Tick(dt);
        }
    }

    public override void OnFinalize()
    {
        base.OnFinalize();
        NetworkCommunicator.OnPeerAveragePingUpdated -= OnPeerPingUpdated;
        ManagedOptions.OnManagedOptionChanged = (ManagedOptions.OnManagedOptionChangedDelegate)Delegate.Remove(ManagedOptions.OnManagedOptionChanged, new ManagedOptions.OnManagedOptionChangedDelegate(OnManagedOptionChanged));
    }

    public void UpdateRoundAttributes()
    {
        RoundsWon = _missionScoreboardSide.SideScore;
        SortPlayers();
    }

    public void UpdateBotAttributes()
    {
        int num = (_missionScoreboardSide.Side == BattleSideEnum.Attacker) ? MultiplayerOptions.OptionType.NumberOfBotsTeam1.GetIntValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions) : MultiplayerOptions.OptionType.NumberOfBotsTeam2.GetIntValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions);
        if (num > 0)
        {
            int score = _missionScoreboardSide.GetScore(null);
            string[] valuesOf = _missionScoreboardSide.GetValuesOf(null);
            _bot.UpdateAttributes(valuesOf, score);
            if (!Players.Contains(_bot))
            {
                Players.Add(_bot);
            }
        }
        else if (num == 0 && Players.Contains(_bot))
        {
            Players.Remove(_bot);
        }

        SortPlayers();
    }

    public void UpdatePlayerAttributes(MissionPeer player)
    {
        if (_playersMap.ContainsKey(player))
        {
            int score = _missionScoreboardSide.GetScore(player);
            string[] valuesOf = _missionScoreboardSide.GetValuesOf(player);
            _playersMap[player].UpdateAttributes(valuesOf, score);
        }

        SortPlayers();
    }

    public void RemovePlayer(MissionPeer peer)
    {
        Players.Remove(_playersMap[peer]);
        _playersMap.Remove(peer);
        SortPlayers();
        UpdatePlayersText();
    }

    public void AddPlayer(MissionPeer peer)
    {
        if (!_playersMap.ContainsKey(peer))
        {
            int score = _missionScoreboardSide.GetScore(peer);
            string[] valuesOf = _missionScoreboardSide.GetValuesOf(peer);
            string[] headerIds = _missionScoreboardSide.GetHeaderIds();
            MissionScoreboardPlayerVM missionScoreboardPlayerVM = new(peer, valuesOf, headerIds, score, _executeActivate);
            _playersMap.Add(peer, missionScoreboardPlayerVM);
            Players.Add(missionScoreboardPlayerVM);
        }

        SortPlayers();
        UpdatePlayersText();
    }

    private void UpdatePlayersText()
    {
        TextObject textObject = new("{=R28ac5ij}{NUMBER} Players", null);
        textObject.SetTextVariable("NUMBER", Players.Count);
        PlayersText = textObject.ToString();
    }

    private void SortPlayers()
    {
        PlayerSortController.SortByCurrentState();
    }

    private void OnPeerPingUpdated(NetworkCommunicator peer)
    {
        MissionPeer component = peer.GetComponent<MissionPeer>();
        if (component != null)
        {
            UpdatePlayerAttributes(component);
        }
    }

    private void OnManagedOptionChanged(ManagedOptions.ManagedOptionsType changedManagedOptionsType)
    {
        if (changedManagedOptionsType == ManagedOptions.ManagedOptionsType.EnableGenericAvatars)
        {
            foreach (MissionScoreboardPlayerVM missionScoreboardPlayerVM in Players)
            {
                missionScoreboardPlayerVM.RefreshAvatar();
            }
        }
    }

    private void OnCommanderUpdated(BattleSideEnum side)
    {
        if (side == _missionScoreboardSide.Side)
        {
            UpdateCommanderText();
        }
    }

    private void UpdateCommanderText()
    {
        TextObject textObject = new("{=hZgASnWR}Commander: {COMMANDER}", null);
        textObject.SetTextVariable("COMMANDER", _commanderClient?.GetCommanderBySide(_missionScoreboardSide.Side)?.UserName ?? new TextObject("{=EKqqPsBk}Unassigned").ToString());
        CommanderText = textObject.ToString();
    }

    [DataSourceProperty]
    public MBBindingList<MissionScoreboardPlayerVM> Players
    {
        get
        {
            return _players;
        }
        set
        {
            if (_players != value)
            {
                _players = value;
                OnPropertyChangedWithValue(value, "Players");
            }
        }
    }

    [DataSourceProperty]
    public MBBindingList<CrpgMissionScoreboardHeaderItemVM> EntryProperties
    {
        get
        {
            return _entryProperties;
        }
        set
        {
            if (value != _entryProperties)
            {
                _entryProperties = value;
                OnPropertyChangedWithValue(value, "EntryProperties");
            }
        }
    }

    [DataSourceProperty]
    public MissionScoreboardPlayerSortControllerVM PlayerSortController
    {
        get
        {
            return _playerSortController;
        }
        set
        {
            if (value != _playerSortController)
            {
                _playerSortController = value;
                OnPropertyChangedWithValue(value, "PlayerSortController");
            }
        }
    }

    [DataSourceProperty]
    public bool IsSingleSide
    {
        get
        {
            return _isSingleSide;
        }
        set
        {
            if (value != _isSingleSide)
            {
                _isSingleSide = value;
                OnPropertyChangedWithValue(value, "IsSingleSide");
            }
        }
    }

    [DataSourceProperty]
    public bool IsSecondSide
    {
        get
        {
            return _isSecondSide;
        }
        set
        {
            if (value != _isSecondSide)
            {
                _isSecondSide = value;
                OnPropertyChangedWithValue(value, "IsSecondSide");
            }
        }
    }

    [DataSourceProperty]
    public bool UseSecondary
    {
        get
        {
            return _useSecondary;
        }
        set
        {
            if (value != _useSecondary)
            {
                _useSecondary = value;
                OnPropertyChangedWithValue(value, "UseSecondary");
            }
        }
    }

    [DataSourceProperty]
    public bool ShowAttackerOrDefenderIcons
    {
        get
        {
            return _showAttackerOrDefenderIcons;
        }
        set
        {
            if (value != _showAttackerOrDefenderIcons)
            {
                _showAttackerOrDefenderIcons = value;
                OnPropertyChangedWithValue(value, "ShowAttackerOrDefenderIcons");
            }
        }
    }

    [DataSourceProperty]
    public bool ShowCommanders
    {
        get
        {
            return _showCommanders;
        }
        set
        {
            if (value != _showCommanders)
            {
                _showCommanders = value;
                OnPropertyChangedWithValue(value, "ShowCommanders");
            }
        }
    }

    [DataSourceProperty]
    public bool IsAttacker
    {
        get
        {
            return _isAttacker;
        }
        set
        {
            if (value != _isAttacker)
            {
                _isAttacker = value;
                OnPropertyChangedWithValue(value, "IsAttacker");
            }
        }
    }

    [DataSourceProperty]
    public string AllyTeamName
    {
        get
        {
            return _allyTeamName;
        }
        set
        {
            if (value != _allyTeamName)
            {
                _allyTeamName = value;
                OnPropertyChangedWithValue(value, "Name");
            }
        }
    }

    [DataSourceProperty]
    public string EnemyTeamName
    {
        get
        {
            return _enemyTeamName;
        }
        set
        {
            if (value != _enemyTeamName)
            {
                _enemyTeamName = value;
                OnPropertyChangedWithValue(value, "Name");
            }
        }
    }

    [DataSourceProperty]
    public string PlayersText
    {
        get
        {
            return _playersText;
        }
        set
        {
            if (value != _playersText)
            {
                _playersText = value;
                OnPropertyChangedWithValue(value, "PlayersText");
            }
        }
    }

    [DataSourceProperty]
    public string CommanderText
    {
        get
        {
            return _commanderText;
        }
        set
        {
            if (value != _commanderText)
            {
                _commanderText = value;
                OnPropertyChangedWithValue(value, "CommanderText");
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
                OnPropertyChangedWithValue(value, "CultureId");
            }
        }
    }

    [DataSourceProperty]
    public int RoundsWon
    {
        get
        {
            return _roundsWon;
        }
        set
        {
            if (_roundsWon != value)
            {
                _roundsWon = value;
                OnPropertyChangedWithValue(value, "RoundsWon");
            }
        }
    }

    [DataSourceProperty]
    public string TeamColor
    {
        get
        {
            return _teamColor;
        }
        set
        {
            if (value != _teamColor)
            {
                _teamColor = value;
                OnPropertyChangedWithValue(value, "TeamColor");
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
