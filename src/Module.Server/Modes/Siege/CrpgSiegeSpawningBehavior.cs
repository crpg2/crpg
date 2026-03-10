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

    public void SetSpawnOverride(float timerDuration)
    {
        _allowSpawnTimerOverride = true;
        _spawnTimerOverrideTimer = new MissionTimer(timerDuration);
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

        var characterEquipment = CrpgCharacterBuilder.CreateCharacterEquipment(crpgPeer.User.Character.EquippedItems);
        if (!DoesEquipmentContainWeapon(characterEquipment)) // Disallow spawning without weapons.
        {
            KickHelper.Kick(networkPeer, DisconnectType.KickedByHost, "no_weapon");
            return false;
        }

        return true;
    }
}
