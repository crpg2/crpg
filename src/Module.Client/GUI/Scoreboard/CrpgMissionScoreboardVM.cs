using Crpg.Module.Common.Commander;
using TaleWorlds.CampaignSystem.ViewModelCollection.Input;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Generic;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.MountAndBlade.Multiplayer;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Scoreboard;
using TaleWorlds.PlatformService;
using TaleWorlds.PlayerServices;

namespace Crpg.Module.GUI.Scoreboard;

internal class CrpgMissionScoreboardVm : ViewModel
{
    private const float AttributeRefreshDuration = 1f;

    private readonly Dictionary<BattleSideEnum, CrpgScoreboardSideVM> _missionSides;
    private readonly MissionScoreboardComponent _missionScoreboardComponent;
    private readonly MultiplayerPollComponent _missionPollComponent = null!;
    private readonly CrpgCommanderPollComponent? _commanderPollComponent;
    private readonly CrpgCommanderBehaviorClient? _commanderClient;
    private readonly Mission _mission;
    private readonly ChatBox _chatBox;
    private readonly VoiceChatHandler? _voiceChatHandler;
    private readonly MultiplayerPermissionHandler? _permissionHandler;
    private readonly bool _canStartKickPolls;
    private readonly bool _canStartCommanderPolls;
    private readonly TextObject _muteAllText = new("{=AZSbwcG5}Mute All");
    private readonly TextObject _unmuteAllText = new("{=SzRVIPeZ}Unmute All");
    private float _attributeRefreshTimeElapsed;
    private bool _hasMutedAll;
    private bool _isActive;
    private InputKeyItemVM? _showMouseKey;
    private CrpgScoreboardEndOfBattleVM? _endOfBattle;
    private MBBindingList<CrpgScoreboardSideVM> _sides = null!;
    private MBBindingList<StringPairItemWithActionVM> _playerActionList = null!;
    private string _spectators = null!;
    private string _missionName = null!;
    private string _gameModeText = null!;
    private string _mapName = null!;
    private string _serverName = null!;
    private bool _isBotsEnabled;
    private bool _isSingleSide;
    private bool _isInitalizationOver;
    private bool _isUpdateOver;
    private bool _isMouseEnabled;
    private bool _isPlayerActionsActive;
    private string _toggleMuteText = null!;
    private BannerImageIdentifierVM? _allyBanner;
    private BannerImageIdentifierVM? _enemyBanner;

    public CrpgMissionScoreboardVm(bool isSingleTeam, Mission mission)
    {
        _chatBox = Game.Current.GetGameHandler<ChatBox>();
        _chatBox.OnPlayerMuteChanged += OnPlayerMuteChanged;
        _mission = mission;
        MissionLobbyComponent missionBehavior = mission.GetMissionBehavior<MissionLobbyComponent>();
        _missionScoreboardComponent = mission.GetMissionBehavior<MissionScoreboardComponent>();
        _voiceChatHandler = _mission.GetMissionBehavior<VoiceChatHandler>();
        _permissionHandler = GameNetwork.GetNetworkComponent<MultiplayerPermissionHandler>();
        if (_voiceChatHandler != null)
        {
            _voiceChatHandler.OnPeerMuteStatusUpdated += OnPeerMuteStatusUpdated;
        }

        if (_permissionHandler != null)
        {
            _permissionHandler.OnPlayerPlatformMuteChanged += OnPlayerPlatformMuteChanged;
        }

        _canStartKickPolls = MultiplayerOptions.OptionType.AllowPollsToKickPlayers.GetBoolValue();
        if (_canStartKickPolls)
        {
            _missionPollComponent = mission.GetMissionBehavior<MultiplayerPollComponent>();
        }

        _commanderClient = mission.GetMissionBehavior<CrpgCommanderBehaviorClient>();
        _commanderPollComponent = mission.GetMissionBehavior<CrpgCommanderPollComponent>();
        _canStartCommanderPolls = false;
        if (_commanderClient != null && _commanderPollComponent != null)
        {
            _canStartCommanderPolls = true;
        }

        EndOfBattle = new CrpgScoreboardEndOfBattleVM(mission, _missionScoreboardComponent, isSingleTeam);
        PlayerActionList = new MBBindingList<StringPairItemWithActionVM>();
        Sides = new MBBindingList<CrpgScoreboardSideVM>();
        _missionSides = new Dictionary<BattleSideEnum, CrpgScoreboardSideVM>();
        IsSingleSide = isSingleTeam;
        InitSides();
        GameKey gameKey = HotKeyManager.GetCategory("ScoreboardHotKeyCategory").GetGameKey(35);
        ShowMouseKey = InputKeyItemVM.CreateFromGameKey(gameKey, false);
        _missionScoreboardComponent.OnPlayerSideChanged += OnPlayerSideChanged;
        _missionScoreboardComponent.OnPlayerPropertiesChanged += OnPlayerPropertiesChanged;
        _missionScoreboardComponent.OnBotPropertiesChanged += OnBotPropertiesChanged;
        _missionScoreboardComponent.OnRoundPropertiesChanged += OnRoundPropertiesChanged;
        _missionScoreboardComponent.OnScoreboardInitialized += OnScoreboardInitialized;
        _missionScoreboardComponent.OnMVPSelected += OnMVPSelected;
        MissionName = "";
        IsBotsEnabled = missionBehavior.MissionType == MultiplayerGameType.Captain || missionBehavior.MissionType == MultiplayerGameType.Battle;
        RefreshValues();
        var customBanners = Mission.Current.GetMissionBehavior<CrpgCustomTeamBannersAndNamesClient>();
        if (customBanners != null)
        {
            customBanners.BannersChanged += HandleBannerChange;
        }
    }

    public override void RefreshValues()
    {
        base.RefreshValues();
        MissionLobbyComponent missionBehavior = _mission.GetMissionBehavior<MissionLobbyComponent>();
        UpdateToggleMuteText();
        GameModeText = GameTexts.FindText("str_multiplayer_game_type", missionBehavior.MissionType.ToString()).ToString().ToLower();
        EndOfBattle?.RefreshValues();
        Sides.ApplyActionOnAllItems(x =>
        {
            x.RefreshValues();
        });
        MapName = GameTexts.FindText("str_multiplayer_scene_name", missionBehavior.Mission.SceneName).ToString();
        ServerName = MultiplayerOptions.OptionType.ServerName.GetStrValue();
        ShowMouseKey?.RefreshValues();
    }

    public void SetMouseState(bool isMouseVisible)
    {
        IsMouseEnabled = isMouseVisible;
    }

    public void Tick(float dt)
    {
        if (IsActive)
        {
            EndOfBattle?.Tick(dt);

            CheckAttributeRefresh(dt);
            foreach (CrpgScoreboardSideVM crpgScoreboardSideVm in Sides)
            {
                crpgScoreboardSideVm.Tick(dt);
            }

            foreach (CrpgScoreboardSideVM crpgScoreboardSideVm2 in Sides)
            {
                foreach (MissionScoreboardPlayerVM missionScoreboardPlayerVm in crpgScoreboardSideVm2.Players)
                {
                    missionScoreboardPlayerVm.RefreshDivision(IsSingleSide);
                }
            }
        }
    }

    public override void OnFinalize()
    {
        base.OnFinalize();
        _missionScoreboardComponent.OnPlayerSideChanged -= OnPlayerSideChanged;
        _missionScoreboardComponent.OnPlayerPropertiesChanged -= OnPlayerPropertiesChanged;
        _missionScoreboardComponent.OnBotPropertiesChanged -= OnBotPropertiesChanged;
        _missionScoreboardComponent.OnRoundPropertiesChanged -= OnRoundPropertiesChanged;
        _missionScoreboardComponent.OnScoreboardInitialized -= OnScoreboardInitialized;
        _missionScoreboardComponent.OnMVPSelected -= OnMVPSelected;
        _chatBox.OnPlayerMuteChanged -= OnPlayerMuteChanged;
        if (_voiceChatHandler != null)
        {
            _voiceChatHandler.OnPeerMuteStatusUpdated -= OnPeerMuteStatusUpdated;
        }

        foreach (CrpgScoreboardSideVM crpgScoreboardSideVm in Sides)
        {
            crpgScoreboardSideVm.OnFinalize();
        }
    }

    public void OnPlayerSideChanged(Team? curTeam, Team? nextTeam, MissionPeer client)
    {
        if (client.IsMine && nextTeam != null && IsSideValid(nextTeam.Side))
        {
            InitSides();
            return;
        }

        if (curTeam != null && IsSideValid(curTeam.Side))
        {
            _missionSides[_missionScoreboardComponent.GetSideSafe(curTeam.Side).Side].RemovePlayer(client);
        }

        if (nextTeam != null && IsSideValid(nextTeam.Side))
        {
            _missionSides[_missionScoreboardComponent.GetSideSafe(nextTeam.Side).Side].AddPlayer(client);
        }
    }

    private void OnPlayerPlatformMuteChanged(PlayerId playerId, bool isPlayerMuted)
    {
        foreach (CrpgScoreboardSideVM crpgScoreboardSideVm in Sides)
        {
            foreach (MissionScoreboardPlayerVM missionScoreboardPlayerVm in crpgScoreboardSideVm.Players)
            {
                if (missionScoreboardPlayerVm.Peer?.Peer?.Id.Equals(playerId) ?? false)
                {
                    missionScoreboardPlayerVm.UpdateIsMuted();
                    return;
                }
            }
        }
    }

    private void OnPlayerMuteChanged(PlayerId playerId, bool isMuted)
    {
        foreach (CrpgScoreboardSideVM crpgScoreboardSideVm in Sides)
        {
            foreach (MissionScoreboardPlayerVM missionScoreboardPlayerVm in crpgScoreboardSideVm.Players)
            {
                if (missionScoreboardPlayerVm.Peer?.Peer?.Id.Equals(playerId) ?? false)
                {
                    missionScoreboardPlayerVm.UpdateIsMuted();
                    return;
                }
            }
        }
    }

    private void ExecutePopulateActionList(MissionScoreboardPlayerVM player)
    {
        PlayerActionList.Clear();
        if (player.Peer != null && !player.IsMine && !player.IsBot)
        {
            PlayerId id = player.Peer.Peer.Id;
            bool flag = _chatBox.IsPlayerMutedFromGame(id);
            bool flag2 = PermaMuteList.IsPlayerMuted(id);
            bool flag3 = _chatBox.IsPlayerMutedFromPlatform(id);
            bool isMutedFromPlatform = player.Peer.IsMutedFromPlatform;
            if (!flag3)
            {
                if (!flag2)
                {
                    if (PlatformServices.Instance.IsPermanentMuteAvailable)
                    {
                        PlayerActionList.Add(new StringPairItemWithActionVM(ExecutePermanentlyMute, new TextObject("{=77jmd4QF}Mute Permanently").ToString(), "PermanentMute", player));
                    }

                    string definition = flag ? GameTexts.FindText("str_mp_scoreboard_context_unmute_text").ToString() : GameTexts.FindText("str_mp_scoreboard_context_mute_text").ToString();
                    PlayerActionList.Add(new StringPairItemWithActionVM(ExecuteMute, definition, flag ? "UnmuteText" : "MuteText", player));
                }
                else
                {
                    PlayerActionList.Add(new StringPairItemWithActionVM(ExecuteLiftPermanentMute, new TextObject("{=CIVPNf2d}Remove Permanent Mute").ToString(), "UnmuteText", player));
                }
            }

            if (player.IsTeammate)
            {
                if (!isMutedFromPlatform && _voiceChatHandler != null && !flag2)
                {
                    string definition2 = player.Peer.IsMuted ? GameTexts.FindText("str_mp_scoreboard_context_unmute_voice").ToString() : GameTexts.FindText("str_mp_scoreboard_context_mute_voice").ToString();
                    PlayerActionList.Add(new StringPairItemWithActionVM(ExecuteMuteVoice, definition2, player.Peer.IsMuted ? "UnmuteVoice" : "MuteVoice", player));
                }

                if (_canStartKickPolls)
                {
                    PlayerActionList.Add(new StringPairItemWithActionVM(ExecuteKick, GameTexts.FindText("str_mp_scoreboard_context_kick").ToString(), "StartKickPoll", player));
                }

                if (_canStartCommanderPolls)
                {
                    bool isCommander = _commanderClient!.IsPeerCommander(player.Peer);
                    string definition3 = isCommander ? GameTexts.FindText("str_mp_scoreboard_context_demote_commander").ToString() : GameTexts.FindText("str_mp_scoreboard_context_promote_commander").ToString();
                    PlayerActionList.Add(new StringPairItemWithActionVM(ExecuteCommander, definition3, isCommander ? "DemoteCommander" : "PromoteCommander", player));
                }
            }

            StringPairItemWithActionVM stringPairItemWithActionVm = new(ExecuteReport, GameTexts.FindText("str_mp_scoreboard_context_report").ToString(), "Report", player);
            if (MultiplayerReportPlayerManager.IsPlayerReportedOverLimit(id))
            {
                stringPairItemWithActionVm.IsEnabled = false;
                stringPairItemWithActionVm.Hint.HintText = new TextObject("{=klkYFik9}You've already reported this player.");
            }

            PlayerActionList.Add(stringPairItemWithActionVm);
            MultiplayerPlayerContextMenuHelper.AddMissionViewProfileOptions(player, PlayerActionList);
        }

        if (PlayerActionList.Count > 0)
        {
            IsPlayerActionsActive = false;
            IsPlayerActionsActive = true;
        }
    }

    private void ExecuteReport(object playerObj)
    {
        if (playerObj is not MissionScoreboardPlayerVM missionScoreboardPlayerVm)
        {
            return;
        }

        MultiplayerReportPlayerManager.RequestReportPlayer(NetworkMain.GameClient.CurrentMatchId, missionScoreboardPlayerVm.Peer.Peer.Id, missionScoreboardPlayerVm.Peer.DisplayedName, true);
    }

    private void ExecuteCommander(object playerObj)
    {
        if (playerObj is not MissionScoreboardPlayerVM missionScoreboardPlayerVm)
        {
            return;
        }

        if (_mission.GetMissionBehavior<MissionLobbyComponent>().IsInWarmup)
        {
            MBInformationManager.AddQuickInformation(new TextObject("{=wQOq8JIE}You cannot vote for a Commander during warmup!"));
            return;
        }

        bool isCommander = _commanderClient!.IsPeerCommander(missionScoreboardPlayerVm.Peer);
        _commanderPollComponent!.RequestCommanderPoll(missionScoreboardPlayerVm.Peer.GetNetworkPeer(), isCommander);
    }

    private void ExecuteMute(object playerObj)
    {
        if (playerObj is not MissionScoreboardPlayerVM missionScoreboardPlayerVm)
        {
            return;
        }

        bool flag = _chatBox.IsPlayerMutedFromGame(missionScoreboardPlayerVm.Peer.Peer.Id);
        _chatBox.SetPlayerMuted(missionScoreboardPlayerVm.Peer.Peer.Id, !flag);
        GameTexts.SetVariable("PLAYER_NAME", missionScoreboardPlayerVm.Peer.DisplayedName);
        InformationManager.DisplayMessage(new InformationMessage(!flag ? GameTexts.FindText("str_mute_notification").ToString() : GameTexts.FindText("str_unmute_notification").ToString()));
    }

    private void ExecuteMuteVoice(object playerObj)
    {
        MissionScoreboardPlayerVM? missionScoreboardPlayerVm = playerObj as MissionScoreboardPlayerVM;
        missionScoreboardPlayerVm?.Peer.SetMuted(!missionScoreboardPlayerVm.Peer.IsMuted);
        missionScoreboardPlayerVm?.UpdateIsMuted();
    }

    private void ExecutePermanentlyMute(object playerObj)
    {
        if (playerObj is not MissionScoreboardPlayerVM missionScoreboardPlayerVm)
        {
            return;
        }

        PermaMuteList.MutePlayer(missionScoreboardPlayerVm.Peer.Peer.Id, missionScoreboardPlayerVm.Peer.Name);
        missionScoreboardPlayerVm.Peer.SetMuted(true);
        missionScoreboardPlayerVm.UpdateIsMuted();
        GameTexts.SetVariable("PLAYER_NAME", missionScoreboardPlayerVm.Peer.DisplayedName);
        InformationManager.DisplayMessage(new InformationMessage(GameTexts.FindText("str_permanent_mute_notification").ToString()));
    }

    private void ExecuteLiftPermanentMute(object playerObj)
    {
        if (playerObj is not MissionScoreboardPlayerVM missionScoreboardPlayerVm)
        {
            return;
        }

        PermaMuteList.RemoveMutedPlayer(missionScoreboardPlayerVm.Peer.Peer.Id);
        missionScoreboardPlayerVm.Peer.SetMuted(false);
        missionScoreboardPlayerVm.UpdateIsMuted();
        GameTexts.SetVariable("PLAYER_NAME", missionScoreboardPlayerVm.Peer.DisplayedName);
        InformationManager.DisplayMessage(new InformationMessage(GameTexts.FindText("str_unmute_notification").ToString()));
    }

    private void ExecuteKick(object playerObj)
    {
        if (playerObj is not MissionScoreboardPlayerVM missionScoreboardPlayerVm)
        {
            return;
        }

        if (_canStartCommanderPolls)
        {
            if (_commanderPollComponent!.IsPollOngoing())
            {
                _commanderPollComponent.RejectPoll(MultiplayerPollRejectReason.HasOngoingPoll);
                return;
            }
        }

        _missionPollComponent.RequestKickPlayerPoll(missionScoreboardPlayerVm.Peer.GetNetworkPeer(), false);
    }

    private void CheckAttributeRefresh(float dt)
    {
        _attributeRefreshTimeElapsed += dt;
        if (_attributeRefreshTimeElapsed >= AttributeRefreshDuration)
        {
            UpdateSideAllPlayersAttributes(BattleSideEnum.Attacker);
            UpdateSideAllPlayersAttributes(BattleSideEnum.Defender);
            _attributeRefreshTimeElapsed = 0f;
        }
    }

    private void UpdateSideAllPlayersAttributes(BattleSideEnum battleSide)
    {
        MissionScoreboardComponent.MissionScoreboardSide? missionScoreboardSide = _missionScoreboardComponent.Sides.FirstOrDefault(s => s != null && s.Side == battleSide);
        if (missionScoreboardSide != null)
        {
            foreach (MissionPeer client in missionScoreboardSide.Players)
            {
                OnPlayerPropertiesChanged(battleSide, client);
            }
        }
    }

    private void OnRoundPropertiesChanged()
    {
        foreach (CrpgScoreboardSideVM crpgScoreboardSideVm in _missionSides.Values)
        {
            crpgScoreboardSideVm.UpdateRoundAttributes();
        }
    }

    private void OnPlayerPropertiesChanged(BattleSideEnum side, MissionPeer client)
    {
        if (IsSideValid(side))
        {
            _missionSides[_missionScoreboardComponent.GetSideSafe(side).Side].UpdatePlayerAttributes(client);
        }
    }

    private void OnBotPropertiesChanged(BattleSideEnum side)
    {
        BattleSideEnum side2 = _missionScoreboardComponent.GetSideSafe(side).Side;
        if (IsSideValid(side2))
        {
            _missionSides[side2].UpdateBotAttributes();
        }
    }

    private void OnScoreboardInitialized()
    {
        InitSides();
    }

    private void OnMVPSelected(MissionPeer mvpPeer, int mvpCount)
    {
        foreach (CrpgScoreboardSideVM crpgScoreboardSideVm in Sides)
        {
            foreach (MissionScoreboardPlayerVM missionScoreboardPlayerVm in crpgScoreboardSideVm.Players)
            {
                if (missionScoreboardPlayerVm.Peer == mvpPeer)
                {
                    missionScoreboardPlayerVm.SetMVPBadgeCount(mvpCount);
                    break;
                }
            }
        }
    }

    private bool IsSideValid(BattleSideEnum side)
    {
        if (IsSingleSide)
        {
            return side != BattleSideEnum.None && side != BattleSideEnum.NumSides;
        }

        return side != BattleSideEnum.None && side != BattleSideEnum.NumSides &&
               _missionScoreboardComponent.Sides.Any(s => s != null && s.Side == side);
    }

    private void InitSides()
    {
        Sides.Clear();
        _missionSides.Clear();
        if (IsSingleSide)
        {
            MissionScoreboardComponent.MissionScoreboardSide sideSafe = _missionScoreboardComponent.GetSideSafe(BattleSideEnum.Defender);
            CrpgScoreboardSideVM crpgScoreboardSideVm = new(sideSafe, ExecutePopulateActionList, IsSingleSide, false);
            Sides.Add(crpgScoreboardSideVm);
            _missionSides.Add(sideSafe.Side, crpgScoreboardSideVm);
            return;
        }

        NetworkCommunicator myPeer = GameNetwork.MyPeer;
        MissionPeer? missionPeer = myPeer?.GetComponent<MissionPeer>();
        BattleSideEnum firstSideToAdd = BattleSideEnum.Attacker;
        BattleSideEnum secondSideToAdd = BattleSideEnum.Defender;
        if (missionPeer != null)
        {
            Team team = missionPeer.Team;
            if (team != null && team.Side == BattleSideEnum.Defender)
            {
                firstSideToAdd = BattleSideEnum.Defender;
                secondSideToAdd = BattleSideEnum.Attacker;
            }
        }

        MissionScoreboardComponent.MissionScoreboardSide? missionScoreboardSide = _missionScoreboardComponent.Sides.FirstOrDefault(s => s != null && s.Side == firstSideToAdd);
        if (missionScoreboardSide != null)
        {
            CrpgScoreboardSideVM crpgScoreboardSideVm2 = new(missionScoreboardSide, ExecutePopulateActionList, IsSingleSide, false);
            Sides.Add(crpgScoreboardSideVm2);
            _missionSides.Add(missionScoreboardSide.Side, crpgScoreboardSideVm2);
        }

        missionScoreboardSide = _missionScoreboardComponent.Sides.FirstOrDefault(s => s != null && s.Side == secondSideToAdd);
        if (missionScoreboardSide != null)
        {
            CrpgScoreboardSideVM crpgScoreboardSideVm3 = new(missionScoreboardSide, ExecutePopulateActionList, IsSingleSide, true);
            Sides.Add(crpgScoreboardSideVm3);
            _missionSides.Add(missionScoreboardSide.Side, crpgScoreboardSideVm3);
        }
    }

    private BattleSideEnum AllySide
    {
        get
        {
            BattleSideEnum result = BattleSideEnum.None;
            if (GameNetwork.IsMyPeerReady)
            {
                MissionPeer component = GameNetwork.MyPeer.GetComponent<MissionPeer>();
                if (component != null && component.Team != null)
                {
                    result = component.Team.Side;
                }
            }

            return result;
        }
    }

    private BattleSideEnum EnemySide
    {
        get
        {
            BattleSideEnum allySide = AllySide;
            if (allySide == BattleSideEnum.Defender)
            {
                return BattleSideEnum.Attacker;
            }

            if (allySide == BattleSideEnum.Attacker)
            {
                return BattleSideEnum.Defender;
            }

            Debug.FailedAssert("Ally side must be either Attacker or Defender");
            return BattleSideEnum.None;
        }
    }

    public void DecreaseSpectatorCount(MissionPeer spectatedPeer)
    {
    }

    public void IncreaseSpectatorCount(MissionPeer spectatedPeer)
    {
    }

    public void ExecuteToggleMute()
    {
        foreach (CrpgScoreboardSideVM crpgScoreboardSideVm in Sides)
        {
            foreach (MissionScoreboardPlayerVM missionScoreboardPlayerVm in crpgScoreboardSideVm.Players)
            {
                if (!missionScoreboardPlayerVm.IsMine && missionScoreboardPlayerVm.Peer != null)
                {
                    _chatBox.SetPlayerMuted(missionScoreboardPlayerVm.Peer.Peer.Id, !_hasMutedAll);
                    missionScoreboardPlayerVm.Peer.SetMuted(!_hasMutedAll);
                    missionScoreboardPlayerVm.UpdateIsMuted();
                }
            }
        }

        _hasMutedAll = !_hasMutedAll;
        UpdateToggleMuteText();
    }

    private void HandleBannerChange(string attackerBanner, string defenderBanner, string attackerName, string defenderName)
    {
        AllyBanner = new(GameNetwork.MyPeer.GetComponent<MissionPeer>()?.Team?.Side == BattleSideEnum.Attacker ? new Banner(attackerBanner) : new Banner(defenderBanner), true);
        EnemyBanner = new(GameNetwork.MyPeer.GetComponent<MissionPeer>()?.Team?.Side == BattleSideEnum.Defender ? new Banner(attackerBanner) : new Banner(defenderBanner), true);
    }

    private void UpdateToggleMuteText()
    {
        if (_hasMutedAll)
        {
            ToggleMuteText = _unmuteAllText.ToString();
            return;
        }

        ToggleMuteText = _muteAllText.ToString();
    }

    private void OnPeerMuteStatusUpdated(MissionPeer peer)
    {
        foreach (CrpgScoreboardSideVM crpgScoreboardSideVm in Sides)
        {
            foreach (MissionScoreboardPlayerVM missionScoreboardPlayerVm in crpgScoreboardSideVm.Players)
            {
                if (missionScoreboardPlayerVm.Peer == peer)
                {
                    missionScoreboardPlayerVm.UpdateIsMuted();
                    break;
                }
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

    [DataSourceProperty]
    public CrpgScoreboardEndOfBattleVM? EndOfBattle
    {
        get
        {
            return _endOfBattle;
        }
        set
        {
            if (value != _endOfBattle)
            {
                _endOfBattle = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public MBBindingList<StringPairItemWithActionVM> PlayerActionList
    {
        get
        {
            return _playerActionList;
        }
        set
        {
            if (value != _playerActionList)
            {
                _playerActionList = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public MBBindingList<CrpgScoreboardSideVM> Sides
    {
        get
        {
            return _sides;
        }
        set
        {
            if (value != _sides)
            {
                _sides = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public bool IsUpdateOver
    {
        get
        {
            return _isUpdateOver;
        }
        set
        {
            _isUpdateOver = value;
            OnPropertyChangedWithValue(value);
        }
    }

    [DataSourceProperty]
    public bool IsInitalizationOver
    {
        get
        {
            return _isInitalizationOver;
        }
        set
        {
            if (value != _isInitalizationOver)
            {
                _isInitalizationOver = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public bool IsMouseEnabled
    {
        get
        {
            return _isMouseEnabled;
        }
        set
        {
            if (value != _isMouseEnabled)
            {
                _isMouseEnabled = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public bool IsActive
    {
        get
        {
            return _isActive;
        }
        set
        {
            if (value != _isActive)
            {
                _isActive = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public bool IsPlayerActionsActive
    {
        get
        {
            return _isPlayerActionsActive;
        }
        set
        {
            if (value != _isPlayerActionsActive)
            {
                _isPlayerActionsActive = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public string Spectators
    {
        get
        {
            return _spectators;
        }
        set
        {
            if (value != _spectators)
            {
                _spectators = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public InputKeyItemVM? ShowMouseKey
    {
        get
        {
            return _showMouseKey;
        }
        set
        {
            if (value != _showMouseKey)
            {
                _showMouseKey = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public string MissionName
    {
        get
        {
            return _missionName;
        }
        set
        {
            if (value != _missionName)
            {
                _missionName = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public string GameModeText
    {
        get
        {
            return _gameModeText;
        }
        set
        {
            if (value != _gameModeText)
            {
                _gameModeText = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public string MapName
    {
        get
        {
            return _mapName;
        }
        set
        {
            if (value != _mapName)
            {
                _mapName = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public string ServerName
    {
        get
        {
            return _serverName;
        }
        set
        {
            if (value != _serverName)
            {
                _serverName = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public bool IsBotsEnabled
    {
        get
        {
            return _isBotsEnabled;
        }
        set
        {
            if (value != _isBotsEnabled)
            {
                _isBotsEnabled = value;
                OnPropertyChangedWithValue(value);
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
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public string ToggleMuteText
    {
        get
        {
            return _toggleMuteText;
        }
        set
        {
            if (value != _toggleMuteText)
            {
                _toggleMuteText = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }
}
