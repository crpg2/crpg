using System.Reflection;
using Crpg.Module.Common;
using Crpg.Module.Helpers;
using Crpg.Module.Modes.Battle;
using NetworkMessages.FromServer;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using MathF = TaleWorlds.Library.MathF;

namespace Crpg.Module.Modes.Warmup;

/// <summary>
/// Custom warmup component so we can load the <see cref="CrpgBattleSpawningBehavior"/> as soon as warmup ends.
/// </summary>
internal class CrpgWarmupComponent : MultiplayerWarmupComponent
{
    private const float WarmupRewardTimer = 30f;

    private static readonly FieldInfo WarmupStateField = typeof(MultiplayerWarmupComponent)
        .GetField("_warmupState", BindingFlags.NonPublic | BindingFlags.Instance)!;
    private static readonly FieldInfo TimerComponentField = typeof(MultiplayerWarmupComponent)
        .GetField("_timerComponent", BindingFlags.NonPublic | BindingFlags.Instance)!;
    private static readonly FieldInfo GameModeField = typeof(MultiplayerWarmupComponent)
        .GetField("_gameMode", BindingFlags.NonPublic | BindingFlags.Instance)!;

    private readonly CrpgConstants _constants;
    private readonly MultiplayerGameNotificationsComponent _notificationsComponent;
    private readonly Func<(SpawnFrameBehaviorBase, SpawningBehaviorBase)>? _createSpawnBehaviors;
    private MissionTimer? _rewardTickTimer;
    private MissionTime _currentStateStartTime;
    private List<MissionPeer> _players = new();
    public event Action<float> OnWarmupRewardTick = null!;
    public event Action<int> OnUpdatePlayerCount = null!;

    public CrpgWarmupComponent(CrpgConstants constants,
        MultiplayerGameNotificationsComponent notificationsComponent,
        Func<(SpawnFrameBehaviorBase, SpawningBehaviorBase)>? createSpawnBehaviors)
    {
        _constants = constants;
        _notificationsComponent = notificationsComponent;
        _createSpawnBehaviors = createSpawnBehaviors;
    }

    private WarmupStates WarmupStateReflection
    {
        get => (WarmupStates)WarmupStateField.GetValue(this)!;
        set
        {
            WarmupStateField.SetValue(this, value);
            if (GameNetwork.IsServer)
            {
                _currentStateStartTime = MissionTime.Now;
                GameNetwork.BeginBroadcastModuleEvent();
                GameNetwork.WriteMessage(new WarmupStateChange(WarmupStateReflection, _currentStateStartTime.NumberOfTicks));
                GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
            }
        }
    }

    private MultiplayerTimerComponent TimerComponentReflection => (MultiplayerTimerComponent)TimerComponentField.GetValue(this)!;
    private MissionMultiplayerGameModeBase GameModeReflection => (MissionMultiplayerGameModeBase)GameModeField.GetValue(this)!;

    public override void OnBehaviorInitialize()
    {
        base.OnBehaviorInitialize();
        MissionPeer.OnTeamChanged += HandlePeerTeamChanged;
    }

    public override void OnRemoveBehavior()
    {
        base.OnRemoveBehavior();
        MissionPeer.OnTeamChanged -= HandlePeerTeamChanged;
    }

    public override void OnPreDisplayMissionTick(float dt)
    {
        if (!GameNetwork.IsServer)
        {
            return;
        }

        var warmupState = WarmupStateReflection;
        switch (warmupState)
        {
            case WarmupStates.WaitingForPlayers:
                BeginWarmup();
                break;
            case WarmupStates.InProgress:
                if (CheckForWarmupProgressEnd())
                {
                    EndWarmupProgress();
                }

                break;
            case WarmupStates.Ending:
                if (TimerComponentReflection.CheckIfTimerPassed())
                {
                    EndWarmup();
                }

                break;
            case WarmupStates.Ended:
                if (TimerComponentReflection.CheckIfTimerPassed())
                {
                    Mission.RemoveMissionBehavior(this);
                }

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public override void OnMissionTick(float dt)
    {
        if (!GameNetwork.IsServer)
        {
            return;
        }

        RewardUsers();
    }

    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        base.AddRemoveMessageHandlers(registerer);
        registerer.Register<WarmupStateChange>(HandleServerEventWarmupStateChange);
    }

    protected override void HandleNewClientAfterSynchronized(NetworkCommunicator networkPeer)
    {
        if (IsInWarmup && !networkPeer.IsServerPeer)
        {
            GameNetwork.BeginModuleEventAsServer(networkPeer);
            GameNetwork.WriteMessage(new WarmupStateChange(WarmupStateReflection, _currentStateStartTime.NumberOfTicks));
            GameNetwork.EndModuleEventAsServer();
        }
    }

    private void HandleServerEventWarmupStateChange(WarmupStateChange message)
    {
        WarmupStateReflection = message.WarmupState;
        switch (WarmupStateReflection)
        {
            case WarmupStates.InProgress:
                TimerComponentReflection.StartTimerAsClient(message.StateStartTimeInSeconds, TotalWarmupDuration);
                break;
            case WarmupStates.Ending:
                TimerComponentReflection.StartTimerAsClient(message.StateStartTimeInSeconds, 30f);
                ReflectionHelper.RaiseEvent(this, nameof(OnWarmupEnding), Array.Empty<object>());
                _notificationsComponent.WarmupEnding();
                break;
            case WarmupStates.Ended:
                TimerComponentReflection.StartTimerAsClient(message.StateStartTimeInSeconds, 3f);
                ReflectionHelper.RaiseEvent(this, nameof(OnWarmupEnded), Array.Empty<object>());
                PlayBattleStartingSound();
                break;
        }
    }

    private void HandlePeerTeamChanged(NetworkCommunicator peer, Team oldTeam, Team newTeam)
    {
        if (GameNetwork.VirtualPlayers[peer.VirtualPlayer.Index] != peer.VirtualPlayer)
        {
            return;
        }

        MissionPeer component = peer.GetComponent<MissionPeer>();
        if (newTeam == null || newTeam == Mission.SpectatorTeam)
        {
            _players.Remove(component);
        }

        if ((oldTeam == null || oldTeam.Side == BattleSideEnum.None) && newTeam != Mission.SpectatorTeam)
        {
            _players.Add(component);
        }

        UpdateRemainingPlayers();
    }

    private void UpdateRemainingPlayers()
    {
        // calculate number of remaining players needed to start game
        OnUpdatePlayerCount?.Invoke(MathF.Max(MultiplayerOptions.OptionType.MinNumberOfPlayersForMatchStart.GetIntValue() - _players.Count(), 0));
    }

    private void RewardUsers()
    {
        _rewardTickTimer ??= new MissionTimer(duration: WarmupRewardTimer);
        // only set multi and reward players if not enough to start game
        if (_rewardTickTimer.Check(reset: true) && MultiplayerOptions.OptionType.MinNumberOfPlayersForMatchStart.GetIntValue() - _players.Count() > 0)
        {
            OnWarmupRewardTick?.Invoke(_rewardTickTimer.GetTimerDuration());
        }
    }

    private void BeginWarmup()
    {
        WarmupStateReflection = WarmupStates.InProgress;
        Mission.Current.ResetMission();
        GameModeReflection.MultiplayerTeamSelectComponent.BalanceTeams();
        TimerComponentReflection.StartTimerAsServer(TotalWarmupDuration);
        GameModeReflection.SpawnComponent.SpawningBehavior.Clear();
        SpawnComponent spawnComponent = Mission.GetMissionBehavior<SpawnComponent>();
        spawnComponent.SetNewSpawnFrameBehavior(new CrpgWarmupSpawnFrameBehavior());
        spawnComponent.SetNewSpawningBehavior(new CrpgWarmupSpawningBehavior(_constants));
    }

    private new void EndWarmupProgress()
    {
        WarmupStateReflection = WarmupStates.Ending;
        TimerComponentReflection.StartTimerAsServer(30f);
        ReflectionHelper.RaiseEvent(this, nameof(OnWarmupEnding), Array.Empty<object>());
    }

    private void EndWarmup()
    {
        if (!Mission.GetMissionBehavior<MissionMultiplayerGameModeBase>().CheckForWarmupEnd())
        {
            Mission.GetMissionBehavior<MissionLobbyComponent>().SetStateEndingAsServer();
        }

        WarmupStateReflection = WarmupStates.Ended;
        TimerComponentReflection.StartTimerAsServer(3f);
        ReflectionHelper.RaiseEvent(this, nameof(OnWarmupEnded), Array.Empty<object>());

        Mission.Current.ResetMission();
        GameModeReflection.MultiplayerTeamSelectComponent.BalanceTeams();

        GameModeReflection.SpawnComponent.SpawningBehavior.Clear();
        SpawnComponent spawnComponent = Mission.GetMissionBehavior<SpawnComponent>();
        (SpawnFrameBehaviorBase spawnFrame, SpawningBehaviorBase spawning) = _createSpawnBehaviors!();
        spawnComponent.SetNewSpawnFrameBehavior(spawnFrame);
        spawnComponent.SetNewSpawningBehavior(spawning);
    }

    private void PlayBattleStartingSound()
    {
        MatrixFrame cameraFrame = Mission.Current.GetCameraFrame();
        Vec3 vec = cameraFrame.origin + cameraFrame.rotation.u;
        NetworkCommunicator myPeer = GameNetwork.MyPeer;
        MissionPeer missionPeer = myPeer.GetComponent<MissionPeer>();
        if (missionPeer?.Team != null)
        {
            string text = missionPeer.Team.Side == BattleSideEnum.Attacker ? MultiplayerOptions.OptionType.CultureTeam1.GetStrValue() : MultiplayerOptions.OptionType.CultureTeam2.GetStrValue();
            MBSoundEvent.PlaySound(SoundEvent.GetEventIdFromString("event:/alerts/rally/" + text.ToLower()), vec);
            return;
        }

        MBSoundEvent.PlaySound(SoundEvent.GetEventIdFromString("event:/alerts/rally/generic"), vec);
    }
}
