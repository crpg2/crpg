using System.Reflection;
using Crpg.Module.Common;
using Crpg.Module.Helpers;
using Crpg.Module.Modes.Battle;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using MathF = TaleWorlds.Library.MathF;

namespace Crpg.Module.Modes.Warmup;

/// <summary>
/// Custom warmup component so we can load the <see cref="CrpgBattleSpawningBehavior"/> as soon as warmup ends.
/// </summary>
internal class CrpgWarmupComponent : MultiplayerWarmupComponent
{
    private const float WarmupRewardTimer = 30f;

    private static readonly PropertyInfo WarmupStateProperty = typeof(MultiplayerWarmupComponent)
        .GetProperty("WarmupState", BindingFlags.NonPublic | BindingFlags.Instance)!;
    private static readonly FieldInfo TimerComponentField = typeof(MultiplayerWarmupComponent)
        .GetField("_timerComponent", BindingFlags.NonPublic | BindingFlags.Instance)!;
    private static readonly FieldInfo LobbyComponentField = typeof(MultiplayerWarmupComponent)
        .GetField("_lobbyComponent", BindingFlags.NonPublic | BindingFlags.Instance)!;
    private static readonly FieldInfo GameModeField = typeof(MultiplayerWarmupComponent)
        .GetField("_gameMode", BindingFlags.NonPublic | BindingFlags.Instance)!;

    private readonly CrpgConstants _constants;
    private readonly Func<(SpawnFrameBehaviorBase, SpawningBehaviorBase)>? _createSpawnBehaviors;
    private readonly List<MissionPeer> _players;
    private MissionTimer? _rewardTickTimer;

    public event Action<float>? OnWarmupRewardTick;
    public event Action<int>? OnUpdatePlayerCount;

    public CrpgWarmupComponent(CrpgConstants constants,
        Func<(SpawnFrameBehaviorBase, SpawningBehaviorBase)>? createSpawnBehaviors)
    {
        _constants = constants;
        _createSpawnBehaviors = createSpawnBehaviors;
        _players = [];
    }

    private WarmupStates WarmupStateReflection
    {
        get => (WarmupStates)WarmupStateProperty.GetValue(this)!;
        set => WarmupStateProperty.SetValue(this, value);
    }

    private MultiplayerTimerComponent TimerComponentReflection => (MultiplayerTimerComponent)TimerComponentField.GetValue(this)!;
    private MissionLobbyComponent LobbyComponentReflection => (MissionLobbyComponent)LobbyComponentField.GetValue(this)!;
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

    private void HandlePeerTeamChanged(NetworkCommunicator peer, Team? oldTeam, Team? newTeam)
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
        OnUpdatePlayerCount?.Invoke(MathF.Max(MultiplayerOptions.OptionType.MinNumberOfPlayersForMatchStart.GetIntValue() - _players.Count, 0));
    }

    private void RewardUsers()
    {
        _rewardTickTimer ??= new MissionTimer(duration: WarmupRewardTimer);
        // only set multi and reward players if not enough to start game
        if (_rewardTickTimer.Check(reset: true) && MultiplayerOptions.OptionType.MinNumberOfPlayersForMatchStart.GetIntValue() - _players.Count > 0)
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

        // Note that compared to native, CheckForWarmupEnd is called instead of CanMatchStartAfterWarmup (checking
        // whether a player is present in both teams). What's the point of CheckForWarmupEnd if it's ignored here TW?
        if (!GameModeReflection.CheckForWarmupEnd())
        {
            LobbyComponentReflection.SetStateEndingAsServer();
        }
    }
}
