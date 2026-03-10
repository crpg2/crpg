using Crpg.Module.Common;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Diamond;

namespace Crpg.Module.Modes.TeamDeathmatch;

internal class CrpgTeamDeathmatchSpawningBehavior : CrpgSpawningBehaviorBase
{
    private bool _haveBotsSpawned;
    public CrpgTeamDeathmatchSpawningBehavior(CrpgConstants constants)
        : base(constants)
    {
        CurrentGameMode = MultiplayerGameType.TeamDeathmatch;
    }

    public override void OnTick(float dt)
    {
        if (!IsSpawningEnabled)
        {
            return;
        }

        SpawnAgents();
        SpawnBotAgents(!_haveBotsSpawned);

        TimeSinceSpawnEnabled += dt;
    }

    protected override bool IsRoundInProgress()
    {
        return Mission.Current.CurrentState == Mission.State.Continuing;
    }

    protected override bool IsBotTeamAllowedToSpawn(Team team)
    {
        int respawnPeriod = team.Side == BattleSideEnum.Defender
            ? MultiplayerOptions.OptionType.RespawnPeriodTeam2.GetIntValue()
            : MultiplayerOptions.OptionType.RespawnPeriodTeam1.GetIntValue();
        if (TimeSinceSpawnEnabled != 0 && TimeSinceSpawnEnabled % respawnPeriod > 1)
        {
            return false;
        }

        _haveBotsSpawned = true;
        return true;
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

        int respawnPeriod = missionPeer.Team.Side == BattleSideEnum.Defender
            ? MultiplayerOptions.OptionType.RespawnPeriodTeam2.GetIntValue()
            : MultiplayerOptions.OptionType.RespawnPeriodTeam1.GetIntValue();
        if (TimeSinceSpawnEnabled != 0 && TimeSinceSpawnEnabled % respawnPeriod > 1)
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
