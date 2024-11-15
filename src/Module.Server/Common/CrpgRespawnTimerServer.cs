using Crpg.Module.Common.Network;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common;
internal class CrpgRespawnTimerServer : MissionNetwork
{
    private readonly MissionMultiplayerGameModeBase _missionMultiplayerGameModeBase;
    private readonly CrpgSpawningBehaviorBase _spawnBehavior;
    public CrpgRespawnTimerServer(MissionMultiplayerGameModeBase missionMultiplayerGameModeBase, CrpgSpawningBehaviorBase spawnBehavior)
    {
        _missionMultiplayerGameModeBase = missionMultiplayerGameModeBase;
        _spawnBehavior = spawnBehavior;
    }

    public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
    {
        NetworkCommunicator? networkCommunicator = affectedAgent.MissionPeer?.GetNetworkPeer();
        if (networkCommunicator != null && !_missionMultiplayerGameModeBase.WarmupComponent.IsInWarmup)
        {
            float timeUntilRespawn = _spawnBehavior.TimeUntilRespawn(affectedAgent.Team);
            GameNetwork.BeginModuleEventAsServer(networkCommunicator);
            GameNetwork.WriteMessage(new CrpgUpdateRespawnTimerMessage { TimeToSpawn = timeUntilRespawn });
            GameNetwork.EndModuleEventAsServer();
        }
    }
}
