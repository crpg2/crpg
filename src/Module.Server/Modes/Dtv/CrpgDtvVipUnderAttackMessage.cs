using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Modes.Dtv;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class CrpgDtvVipUnderAttackMessage : GameNetworkMessage
{
    public int AgentAttackerIndex { get; set; }
    public int AgentVictimIndex { get; set; }
    protected override void OnWrite()
    {
        WriteAgentIndexToPacket(AgentAttackerIndex);
        WriteAgentIndexToPacket(AgentVictimIndex);
    }

    protected override bool OnRead()
    {
        bool bufferReadValid = true;
        AgentAttackerIndex = ReadAgentIndexFromPacket(ref bufferReadValid);
        AgentVictimIndex = ReadAgentIndexFromPacket(ref bufferReadValid);
        return bufferReadValid;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter()
    {
        return MultiplayerMessageFilter.GameMode;
    }

    protected override string OnGetLogFormat()
    {
        return "cRPG DTV VIP Under Attack";
    }
}
