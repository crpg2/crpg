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

    public override void OnAgentBuild(Agent agent, Banner banner)
    {
        NetworkCommunicator? networkCommunicator = agent.MissionPeer?.GetNetworkPeer();
        if (networkCommunicator != null && (_missionMultiplayerGameModeBase.WarmupComponent == null || !_missionMultiplayerGameModeBase.WarmupComponent.IsInWarmup))
        {
            GameNetwork.BeginModuleEventAsServer(networkCommunicator);
            GameNetwork.WriteMessage(new CrpgUpdateRespawnTimerMessage { TimeToSpawn = 0 });
            GameNetwork.EndModuleEventAsServer();
        }
    }

    public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
    {
        if (blow.OverrideKillInfo == Agent.KillInfo.TeamSwitch)
        {
            return;
        }

        NetworkCommunicator? networkCommunicator = affectedAgent.MissionPeer?.GetNetworkPeer();
        if (networkCommunicator != null && (_missionMultiplayerGameModeBase.WarmupComponent == null || !_missionMultiplayerGameModeBase.WarmupComponent.IsInWarmup))
        {
            float timeUntilRespawn = _spawnBehavior.TimeUntilRespawn(affectedAgent.Team);
            GameNetwork.BeginModuleEventAsServer(networkCommunicator);
            GameNetwork.WriteMessage(new CrpgUpdateRespawnTimerMessage { TimeToSpawn = timeUntilRespawn });
            GameNetwork.EndModuleEventAsServer();
        }
    }

    private void HandlePeerTeamChanged(NetworkCommunicator peer, Team previousTeam, Team newTeam)
    {
        if (peer != null && (_missionMultiplayerGameModeBase.WarmupComponent == null || !_missionMultiplayerGameModeBase.WarmupComponent.IsInWarmup))
        {
            float timeUntilRespawn = _spawnBehavior.TimeUntilRespawn(newTeam);
            GameNetwork.BeginModuleEventAsServer(peer);
            GameNetwork.WriteMessage(new CrpgUpdateRespawnTimerMessage { TimeToSpawn = timeUntilRespawn });
            GameNetwork.EndModuleEventAsServer();
        }
    }
}
