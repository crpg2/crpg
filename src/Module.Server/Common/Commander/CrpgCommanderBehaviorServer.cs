using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common.Commander;
internal class CrpgCommanderBehaviorServer : MissionNetwork
{
    public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;

    public Dictionary<BattleSideEnum, float> LastCommanderMessage { get; private set; } = new();
    private readonly Dictionary<BattleSideEnum, NetworkCommunicator?> _commanders = new();

    public CrpgCommanderBehaviorServer()
    {
        _commanders[BattleSideEnum.Attacker] = null;
        _commanders[BattleSideEnum.Defender] = null;
        _commanders[BattleSideEnum.None] = null;

        LastCommanderMessage.Add(BattleSideEnum.Attacker, 0);
        LastCommanderMessage.Add(BattleSideEnum.Defender, 0);
        LastCommanderMessage.Add(BattleSideEnum.None, 0);
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

    public void CreateCommand(NetworkCommunicator commander)
    {
        BattleSideEnum commanderSide = commander.GetComponent<MissionPeer>().Team.Side;
        _commanders[commanderSide] = commander;
        OnCommanderUpdated(commanderSide);
    }

    public void RemoveCommand(NetworkCommunicator commander)
    {
        foreach (KeyValuePair<BattleSideEnum, NetworkCommunicator?> keyValuePair in _commanders)
        {
            if (keyValuePair.Value == commander)
            {
                _commanders[keyValuePair.Key] = null;
                OnCommanderUpdated(keyValuePair.Key);
            }
        }
    }

    public void SetCommanderMessageSendTime(BattleSideEnum side,  float time)
    {
        LastCommanderMessage[side] = time;
    }

    public void OnCommanderUpdated(BattleSideEnum side)
{
        GameNetwork.BeginBroadcastModuleEvent();
        GameNetwork.WriteMessage(new UpdateCommander { Side = side, Commander = _commanders[side] });
        GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
    }

    public bool IsPlayerACommander(NetworkCommunicator networkCommunicator)
    {
        return _commanders.ContainsValue(networkCommunicator);
    }

    public override void OnPlayerDisconnectedFromServer(NetworkCommunicator networkPeer)
    {
        if (IsPlayerACommander(networkPeer))
        {
            RemoveCommand(networkPeer);
        }
    }

    public void HandlePeerTeamChanged(NetworkCommunicator peer, Team previousTeam, Team newTeam)
    {
        if (peer != null)
        {
            if (IsPlayerACommander(peer))
            {
                RemoveCommand(peer);
            }
        }
    }

    public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
    {
        NetworkCommunicator? networkPeer = affectedAgent.MissionPeer?.GetNetworkPeer();
        if (networkPeer != null)
        {
            if (IsPlayerACommander(networkPeer))
            {
                if (agentState == AgentState.Deleted | agentState == AgentState.Routed)
                {
                }
                else
                {
                    GameNetwork.BeginBroadcastModuleEvent();
                    GameNetwork.WriteMessage(new CommanderKilled { AgentCommanderIndex = affectedAgent.Index, AgentKillerIndex = affectorAgent.Index });
                    GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
                }
            }
        }
    }

    protected override void HandleNewClientAfterLoadingFinished(NetworkCommunicator networkPeer)
    {
        foreach (KeyValuePair<BattleSideEnum, NetworkCommunicator?> keyValuePair in _commanders)
        {
            GameNetwork.BeginModuleEventAsServer(networkPeer);
            GameNetwork.WriteMessage(new UpdateCommander { Side = keyValuePair.Key, Commander = keyValuePair.Value });
            GameNetwork.EndModuleEventAsServer();
        }
    }
}
