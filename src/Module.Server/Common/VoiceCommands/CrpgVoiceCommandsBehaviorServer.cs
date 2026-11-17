using System.Collections.Generic;
using Crpg.Module.Common.VoiceCommands;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.VoiceCommands;

internal sealed class CrpgVoiceCommandsBehaviorServer : MissionNetwork
{
    private readonly object _pendingLock = new();
    private readonly Dictionary<NetworkCommunicator, (Agent Agent, int CommandId)> _pendingRequests = new();
    private readonly List<(NetworkCommunicator Peer, Agent Agent, int CommandId)> _drainBuffer = new();
    private readonly SkinVoiceManager.SkinVoiceType?[] _voiceTypes = new SkinVoiceManager.SkinVoiceType?[CrpgVoiceCommandCatalog.Count];

    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        if (GameNetwork.IsServer)
        {
            registerer.RegisterBaseHandler<VoiceCommandRequested>(HandleVoiceCommandRequested);
        }
    }

    public override void OnMissionTick(float dt)
    {
        base.OnMissionTick(dt);

        lock (_pendingLock)
        {
            foreach (KeyValuePair<NetworkCommunicator, (Agent Agent, int CommandId)> request in _pendingRequests)
            {
                _drainBuffer.Add((request.Key, request.Value.Agent, request.Value.CommandId));
            }

            _pendingRequests.Clear();
        }

        try
        {
            foreach ((NetworkCommunicator peer, Agent agent, int commandId) in _drainBuffer)
            {
                if (!peer.IsConnectionActive || !peer.IsSynchronized || !ReferenceEquals(peer.ControlledAgent, agent))
                {
                    continue;
                }

                if (agent.Mission != Mission || !agent.IsHuman || !agent.IsActive())
                {
                    continue;
                }

                SkinVoiceManager.SkinVoiceType? voiceType = GetVoiceType(commandId);
                if (voiceType.HasValue)
                {
                    agent.MakeVoice(voiceType.Value, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
                }
            }
        }
        finally
        {
            _drainBuffer.Clear();
        }
    }

    public override void OnClearScene()
    {
        ClearPendingRequests();
        _drainBuffer.Clear();
        base.OnClearScene();
    }

    public override void OnRemoveBehavior()
    {
        ClearPendingRequests();
        _drainBuffer.Clear();
        base.OnRemoveBehavior();
    }

    protected override void HandleEarlyPlayerDisconnect(NetworkCommunicator peer)
    {
        lock (_pendingLock)
        {
            _pendingRequests.Remove(peer);
        }

        base.HandleEarlyPlayerDisconnect(peer);
    }

    private bool HandleVoiceCommandRequested(NetworkCommunicator peer, GameNetworkMessage baseMessage)
    {
        if (baseMessage is not VoiceCommandRequested message || !CrpgVoiceCommandCatalog.IsValid(message.CommandId))
        {
            return false;
        }

        Agent? controlledAgent = peer.ControlledAgent;
        if (controlledAgent == null)
        {
            return true;
        }

        lock (_pendingLock)
        {
            _pendingRequests.TryAdd(peer, (controlledAgent, message.CommandId));
        }

        return true;
    }

    private SkinVoiceManager.SkinVoiceType? GetVoiceType(int commandId)
    {
        if (_voiceTypes[commandId] is SkinVoiceManager.SkinVoiceType cachedVoiceType)
        {
            return cachedVoiceType.Index < 0 ? null : cachedVoiceType;
        }

        string voiceName = CrpgVoiceCommandCatalog.GetVoiceName(commandId);
        var voiceType = new SkinVoiceManager.SkinVoiceType(voiceName);
        _voiceTypes[commandId] = voiceType;
        if (voiceType.Index < 0)
        {
            Debug.Print($"Voice command type not found: {voiceName}", color: Debug.DebugColor.Red);
            return null;
        }

        return voiceType;
    }

    private void ClearPendingRequests()
    {
        lock (_pendingLock)
        {
            _pendingRequests.Clear();
        }
    }
}
