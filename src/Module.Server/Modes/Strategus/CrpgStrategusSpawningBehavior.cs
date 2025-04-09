using Crpg.Module.Api.Models.Users;
using Crpg.Module.Common;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Diamond;

namespace Crpg.Module.Modes.Strategus;

internal class CrpgStrategusSpawningBehavior : CrpgSpawningBehaviorBase
{
    public readonly Dictionary<BattleSideEnum, int> Tickets = new();
    private CrpgStrategusServer? _server;
    public CrpgStrategusSpawningBehavior(CrpgConstants constants)
        : base(constants)
    {
        CurrentGameMode = MultiplayerGameType.Siege;
    }

    public override void Initialize(SpawnComponent spawnComponent)
    {
        base.Initialize(spawnComponent);
        _server = Mission.Current.GetMissionBehavior<CrpgStrategusServer>();
    }

    public override void OnTick(float dt)
    {
        SpawnAgents();

        TimeSinceSpawnEnabled += dt;
    }

    public void SetTickets(BattleSideEnum side, int tickets)
    {
        Tickets[side] = tickets;
    }

    protected override void OnPeerSpawned(Agent agent)
    {
        base.OnPeerSpawned(agent);

        if (_server!.IsInWarmup)
        {
            return;
        }

        Tickets[agent.Team.Side] -= 1;
        _server!.OnPeerSpwaned(agent);
    }

    protected override bool IsRoundInProgress()
    {
        return Mission.Current.CurrentState == Mission.State.Continuing;
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

        if (Tickets[missionPeer.Team.Side] <= 0)
        {
            return false;
        }

        int respawnPeriod = missionPeer.Team.Side == BattleSideEnum.Defender
            ? MultiplayerOptions.OptionType.RespawnPeriodTeam2.GetIntValue()
            : MultiplayerOptions.OptionType.RespawnPeriodTeam1.GetIntValue();
        if (TimeSinceSpawnEnabled != 0 && TimeSinceSpawnEnabled % respawnPeriod > 1)
        {
            return false;
        }

        var characterEquipment = CrpgCharacterBuilder.CreateCharacterEquipment(crpgPeer.User.Character.EquippedItems);

        // allow spawning without weapon? what if they run out of weapons but not troops, can always pick up

        //if (!DoesEquipmentContainWeapon(characterEquipment)) // Disallow spawning without weapons.
        //{
        //    KickHelper.Kick(networkPeer, DisconnectType.KickedByHost, "no_weapon");
        //    return false;
        //}

        return true;
    }
}
