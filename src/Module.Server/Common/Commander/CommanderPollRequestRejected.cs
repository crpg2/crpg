using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Commander;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class CommanderPollRequestRejected : GameNetworkMessage
{
    public int Reason { get; set; }

    protected override bool OnRead()
    {
        bool bufferReadValid = true;
        Reason = ReadIntFromPacket(CompressionBasic.DebugIntNonCompressionInfo, ref bufferReadValid);
        return bufferReadValid;
    }

    protected override void OnWrite()
    {
        WriteIntToPacket(Reason, CompressionBasic.DebugIntNonCompressionInfo);
    }

    protected override MultiplayerMessageFilter OnGetLogFilter()
    {
        return MultiplayerMessageFilter.Administration;
    }

    protected override string OnGetLogFormat()
    {
        return "Poll request rejected (" + ((MultiplayerPollRejectReason)Reason) + ")";
    }
}
