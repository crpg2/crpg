using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Modes.Dtv;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class CrpgDtvVipSpawn : GameNetworkMessage
{
    public int VipAgentIndex { get; set; }

    protected override void OnWrite()
    {
        WriteAgentIndexToPacket(VipAgentIndex);
    }

    protected override bool OnRead()
    {
        bool bufferReadValid = true;
        VipAgentIndex = ReadAgentIndexFromPacket(ref bufferReadValid);
        return bufferReadValid;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter()
    {
        return MultiplayerMessageFilter.GameMode;
    }

    protected override string OnGetLogFormat()
    {
        return "cRPG DTV VIP Spawn Data";
    }
}
