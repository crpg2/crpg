using Crpg.Module.Api.Models.Users;
using Crpg.Module.Common;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Diamond;

namespace Crpg.Module.Modes.TeamDeathmatch;

internal class CrpgTeamDeathmatchSpawningBehavior : CrpgSpawningBehaviorBase
{
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
        SpawnBotAgents();

        _timeSinceSpawnEnabled += dt;
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
        if (_timeSinceSpawnEnabled != 0 && _timeSinceSpawnEnabled % respawnPeriod > 1)
        {
            return false;
        }

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
        if (_timeSinceSpawnEnabled != 0 && _timeSinceSpawnEnabled % respawnPeriod > 1)
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
