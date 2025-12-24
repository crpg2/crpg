using Crpg.Module.Common;
using Crpg.Module.Notifications;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.PlayerServices;

namespace Crpg.Module.Modes.Battle;

internal class CrpgBattleSpawningBehavior : CrpgSpawningBehaviorBase
{
    private const float TotalSpawnDuration = 30f;
    private readonly MultiplayerRoundController _roundController;
    private readonly HashSet<PlayerId> _notifiedPlayersAboutSpawnRestriction;
    private MissionTimer? _spawnTimer;
    private MissionTimer? _cavalrySpawnDelayTimer;
    private bool _botsSpawned;
#if CRPG_SERVER
    private CrpgTeamInventoryServer? _teamInventory;
    private CrpgCharacterLoadoutBehaviorServer? _userLoadout;
#endif
    public CrpgBattleSpawningBehavior(CrpgConstants constants, MultiplayerRoundController roundController, MultiplayerGameType currentGameType)
        : base(constants)
    {
        _roundController = roundController;
        _notifiedPlayersAboutSpawnRestriction = new HashSet<PlayerId>();
        CurrentGameMode = currentGameType;
    }

    public override void Initialize(SpawnComponent spawnComponent)
    {
        base.Initialize(spawnComponent);
        _roundController.OnPreparationEnded += RequestStartSpawnSession;
        _roundController.OnRoundEnding += RequestStopSpawnSession;
#if CRPG_SERVER
        _teamInventory = Mission.Current.GetMissionBehavior<CrpgTeamInventoryServer>();
        _userLoadout = Mission.Current.GetMissionBehavior<CrpgCharacterLoadoutBehaviorServer>();
#endif
    }

    public override void Clear()
    {
        base.Clear();
        _roundController.OnPreparationEnded -= RequestStartSpawnSession;
        _roundController.OnRoundEnding -= RequestStopSpawnSession;
    }

    public override void OnTick(float dt)
    {
        if (!IsSpawningEnabled || !IsRoundInProgress())
        {
            return;
        }

        if (_spawnTimer!.Check())
        {
            return;
        }

        if (!_botsSpawned)
        {
            SpawnBotAgents();
            _botsSpawned = true;
        }

        SpawnAgents();
    }

    public override void RequestStartSpawnSession()
    {
        base.RequestStartSpawnSession();
        _botsSpawned = false;
        _spawnTimer = new MissionTimer(TotalSpawnDuration); // Limit spawning for 30 seconds.
        _cavalrySpawnDelayTimer = new MissionTimer(GetCavalrySpawnDelay()); // Cav will spawn X seconds later.
        _notifiedPlayersAboutSpawnRestriction.Clear();
    }

    public bool SpawnDelayEnded()
    {
        return _cavalrySpawnDelayTimer != null && _cavalrySpawnDelayTimer!.Check();
    }

    protected override bool IsRoundInProgress()
    {
        return _roundController.IsRoundInProgress;
    }

    protected override Equipment GetCharacterEquipment(NetworkCommunicator networkPeer, CrpgPeer crpgPeer)
    {
#if CRPG_SERVER
        if (_teamInventory?.IsEnabled == true)
        {
            return _teamInventory.GetPendingEquipment(networkPeer);
        }

        if (_userLoadout?.IsEnabled == true)
        {
            return _userLoadout.GetPeerEquipment(networkPeer);
        }
#endif
        return base.GetCharacterEquipment(networkPeer, crpgPeer);
    }

    protected override bool IsPlayerAllowedToSpawn(NetworkCommunicator networkPeer)
    {
        var crpgPeer = networkPeer.GetComponent<CrpgPeer>();
        var missionPeer = networkPeer.GetComponent<MissionPeer>();
        if (crpgPeer?.User == null
            || crpgPeer.LastSpawnInfo != null
            || missionPeer == null)
        {
            return false;
        }

#if CRPG_SERVER
        if (_teamInventory?.IsEnabled == true)
        {
            var equipment = _teamInventory.GetPendingEquipment(networkPeer);
            return _teamInventory.ReadyToSpawn.Contains(networkPeer) && DoesEquipmentContainWeapon(equipment);
        }

        if (_userLoadout?.IsEnabled == true)
        {
            if (_userLoadout.ReadyToSpawn.Contains(networkPeer))
            {
                if (!DoesEquipmentContainWeapon(_userLoadout.GetPeerEquipment(networkPeer)))
                {
                    _userLoadout.UnsetReadyToSpawnShowMenu(networkPeer, "You must have a melee weapon equipped to spawn.");
                    return false;
                }

                return true;
            }

            return false; // not ready to spawn
        }
#endif

        var characterEquipment = CrpgCharacterBuilder.CreateCharacterEquipment(crpgPeer.User.Character.EquippedItems);
        if (!DoesEquipmentContainWeapon(characterEquipment)) // Disallow spawning without weapons.
        {
            if (_notifiedPlayersAboutSpawnRestriction.Add(networkPeer.VirtualPlayer.Id))
            {
                GameNetwork.BeginModuleEventAsServer(networkPeer);
                GameNetwork.WriteMessage(new CrpgNotificationId
                {
                    Type = CrpgNotificationType.Announcement,
                    TextId = "str_kick_reason",
                    TextVariation = "no_weapon",
                    SoundEvent = string.Empty,
                });
                GameNetwork.EndModuleEventAsServer();
            }

            return false;
        }

        bool hasMount = characterEquipment[EquipmentIndex.Horse].Item != null;
        // Disallow spawning cavalry before the cav spawn delay ended.
        if (hasMount && _cavalrySpawnDelayTimer != null && !_cavalrySpawnDelayTimer.Check())
        {
            if (_notifiedPlayersAboutSpawnRestriction.Add(networkPeer.VirtualPlayer.Id))
            {
                GameNetwork.BeginModuleEventAsServer(networkPeer);
                GameNetwork.WriteMessage(new CrpgNotificationId
                {
                    Type = CrpgNotificationType.Notification,
                    TextId = "str_notification",
                    TextVariation = "cavalry_spawn_delay",
                    SoundEvent = string.Empty,
                    Variables = { ["SECONDS"] = ((int)_cavalrySpawnDelayTimer.GetTimerDuration()).ToString() },
                });
                GameNetwork.EndModuleEventAsServer();
            }

            return false;
        }

        return true;
    }

    protected override void OnPeerSpawned(Agent agent)
    {
        base.OnPeerSpawned(agent);
        agent.MissionPeer.SpawnCountThisRound += 1;
    }

    /// <summary>
    /// Cav spawn delay values
    /// 10 => 7sec
    /// 30 => 9sec
    /// 60 => 13sec
    /// 90 => 17sec
    /// 120 => 22sec
    /// 150 => 26sec
    /// 165+ => 28sec.
    /// </summary>
    private int GetCavalrySpawnDelay()
    {
        int currentPlayers = Math.Max(GetCurrentPlayerCount(), 1);
        return Math.Min(28, 5 + currentPlayers / 7);
    }

    private int GetCurrentPlayerCount()
    {
        int counter = 0;
        foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
        {
            var missionPeer = networkPeer.GetComponent<MissionPeer>();
            if (!networkPeer.IsSynchronized
                || missionPeer == null
                || missionPeer.Team == null
                || missionPeer.Team.Side == BattleSideEnum.None)
            {
                continue;
            }

            counter++;
        }

        return counter;
    }
}
