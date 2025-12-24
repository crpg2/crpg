using Crpg.Module.Common;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Diamond;

namespace Crpg.Module.Modes.Siege;

internal class CrpgSiegeSpawningBehavior : CrpgSpawningBehaviorBase
{
    private const int AttackerSpawnDelay = 20;
    private bool _allowSpawnTimerOverride;
    private MissionTimer? _spawnTimerOverrideTimer;
#if CRPG_SERVER
    private CrpgTeamInventoryServer? _teamInventory;
    private CrpgCharacterLoadoutBehaviorServer? _userLoadout;
#endif

    public CrpgSiegeSpawningBehavior(CrpgConstants constants)
        : base(constants)
    {
        CurrentGameMode = MultiplayerGameType.Siege;
    }

    public override void OnTick(float dt)
    {
        if (!IsSpawningEnabled)
        {
            return;
        }

        SpawnAgents();
        SpawnBotAgents();
        TimeSinceSpawnEnabled += dt;
        if (_spawnTimerOverrideTimer != null && _spawnTimerOverrideTimer.Check())
        {
            _allowSpawnTimerOverride = false;
        }
    }

    public override void Initialize(SpawnComponent spawnComponent)
    {
        base.Initialize(spawnComponent);
#if CRPG_SERVER
        _teamInventory = Mission.Current.GetMissionBehavior<CrpgTeamInventoryServer>();
        _userLoadout = Mission.Current.GetMissionBehavior<CrpgCharacterLoadoutBehaviorServer>();
#endif
    }

    public void SetSpawnOverride(float timerDuration)
    {
        _allowSpawnTimerOverride = true;
        _spawnTimerOverrideTimer = new MissionTimer(timerDuration);
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

    protected override bool IsRoundInProgress()
    {
        return Mission.CurrentState == Mission.State.Continuing;
    }

    protected override bool IsPlayerAllowedToSpawn(NetworkCommunicator networkPeer)
    {
        var crpgPeer = networkPeer.GetComponent<CrpgPeer>();
        var missionPeer = networkPeer.GetComponent<MissionPeer>();
        if (crpgPeer?.User == null
            || missionPeer == null)
        {
            return false;
        }

        BattleSideEnum side = missionPeer.Team.Side;
        if (side == BattleSideEnum.Attacker)
        {
            if (TimeSinceSpawnEnabled < AttackerSpawnDelay)
            {
                return false;
            }
        }

        int respawnPeriod = side == BattleSideEnum.Defender
            ? MultiplayerOptions.OptionType.RespawnPeriodTeam2.GetIntValue()
            : MultiplayerOptions.OptionType.RespawnPeriodTeam1.GetIntValue();
        if (TimeSinceSpawnEnabled != 0 && !_allowSpawnTimerOverride && TimeSinceSpawnEnabled % respawnPeriod > 1)
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
            KickHelper.Kick(networkPeer, DisconnectType.KickedByHost, "no_weapon");
            return false;
        }

        return true;
    }
}
