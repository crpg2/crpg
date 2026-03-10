using Crpg.Module.Helpers;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Objects;
using Timer = TaleWorlds.Core.Timer;

namespace Crpg.Module.Modes.Battle.FlagSystems;
internal class CrpgBattleFlagSystem : AbstractFlagSystem
{
    private bool _isDeadPlayerThresholdReached;
    public CrpgBattleFlagSystem(Mission mission, MultiplayerGameNotificationsComponent notificationsComponent, CrpgBattleClient battleClient)
        : base(mission, notificationsComponent, battleClient)
    {
    }

    public override void CheckForManipulationOfFlags()
    {
        if (HasFlagCountChanged())
        {
            return;
        }

        Timer checkFlagRemovalTimer = GetCheckFlagRemovalTimer(Mission.CurrentTime, GetBattleClient().FlagManipulationTime);
        if (!checkFlagRemovalTimer.Check(Mission.CurrentTime))
        {
            return;
        }

        var randomFlag = GetRandomFlag();
        float duration = _isDeadPlayerThresholdReached ? GetBattleClient().FlagUnlockTime / 3 * 2 : GetBattleClient().FlagUnlockTime;
        SetFlagUnlockTimer(duration);
        SpawnFlag(randomFlag);
        SetHasFlagCountChanged(true);

        GameNetwork.BeginBroadcastModuleEvent();
        GameNetwork.WriteMessage(new CrpgBattleSpawnFlagMessage
        {
            FlagChar = randomFlag.FlagChar,
            Time = duration,
        });
        GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);

        GetBattleClient().ChangeNumberOfFlags();
        Debug.Print("Random Flag has been spawned");
    }

    public void CheckForDeadPlayerFlagSpawnThreshold(int attackersSpawned, int defendersSpawned)
    {
        float attackerCount = Mission.AttackerTeam.ActiveAgents.Count;
        float defenderCount = Mission.DefenderTeam.ActiveAgents.Count;
        // TODO: Create a key for it in server configuration
        float overpowerThreshold = 2f;

        if (attackerCount == 1 || defenderCount == 1)
        {
            ResetFlagSpawnTimer();
            _isDeadPlayerThresholdReached = true;
        }

        if (Math.Min(attackerCount / attackersSpawned, defenderCount / defendersSpawned) > 0.33f)
        {
            return;
        }

        if (MathHelper.Within(attackerCount / defenderCount, 1 / overpowerThreshold, overpowerThreshold))
        {
            return;
        }

        _isDeadPlayerThresholdReached = true;
        ResetFlagSpawnTimer();
    }

    public override FlagCapturePoint GetRandomFlag()
    {
        var uncapturedFlags = GetAllFlags().Where(f => GetFlagOwner(f) == null).ToArray();
        return uncapturedFlags.GetRandomElement();
    }

    protected override bool CanAgentCaptureFlag(Agent agent) => !agent.IsActive() || !agent.IsHuman || agent.HasMount;

    protected override void ResetFlag(FlagCapturePoint flag) => flag.RemovePointAsServer();

    private void ResetFlagSpawnTimer()
    {
        GetCheckFlagRemovalTimer(Mission.CurrentTime, GetBattleClient().FlagManipulationTime).Reset(Mission.CurrentTime, 0);
    }

    private void SpawnFlag(FlagCapturePoint flag)
    {
        flag.ResetPointAsServer(TeammateColorsExtensions.NEUTRAL_COLOR, TeammateColorsExtensions.NEUTRAL_COLOR2);
    }
}
