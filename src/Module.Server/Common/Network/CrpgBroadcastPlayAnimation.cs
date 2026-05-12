using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Network;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class CrpgBroadcastPlayAnimation : GameNetworkMessage
{
    public int AgentIndex { get; set; }
    public string ActionId { get; set; } = string.Empty;

    protected override void OnWrite()
    {
        WriteAgentIndexToPacket(AgentIndex);
        WriteStringToPacket(ActionId);
    }

    protected override bool OnRead()
    {
        bool valid = true;
        AgentIndex = ReadAgentIndexFromPacket(ref valid);
        ActionId = ReadStringFromPacket(ref valid);
        return valid;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter() => MultiplayerMessageFilter.GameMode;
    protected override string OnGetLogFormat() => $"Broadcast play animation: {ActionId} on agent {AgentIndex}";
}
