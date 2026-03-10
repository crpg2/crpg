using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Commander;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class UpdateCommander : GameNetworkMessage
{
    public NetworkCommunicator? Commander { get; set; }
    public BattleSideEnum Side { get; set; }

    protected override bool OnRead()
    {
        bool bufferReadValid = true;
        Side = (BattleSideEnum)ReadIntFromPacket(CompressionBasic.DebugIntNonCompressionInfo, ref bufferReadValid);
        Commander = ReadNetworkPeerReferenceFromPacket(ref bufferReadValid);
        return bufferReadValid;
    }

    protected override void OnWrite()
    {
        WriteIntToPacket((int)Side, CompressionBasic.DebugIntNonCompressionInfo);
        WriteNetworkPeerReferenceToPacket(Commander);
    }

    protected override MultiplayerMessageFilter OnGetLogFilter()
    {
        return MultiplayerMessageFilter.Administration;
    }

    protected override string OnGetLogFormat()
    {
        return "Updating commander: " + Commander?.UserName + " on team: " + Side;
    }
}
