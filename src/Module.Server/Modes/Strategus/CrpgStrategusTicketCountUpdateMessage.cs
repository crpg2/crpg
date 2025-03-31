using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Modes.Strategus;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class CrpgStrategusTicketCountUpdateMessage : GameNetworkMessage
{
    public int AttackerTickets { get; set; }
    public int DefenderTickets { get; set; }

    protected override void OnWrite()
    {
        WriteIntToPacket(AttackerTickets, CompressionBasic.DebugIntNonCompressionInfo);
        WriteIntToPacket(DefenderTickets, CompressionBasic.DebugIntNonCompressionInfo);
    }

    protected override bool OnRead()
    {
        bool bufferReadValid = true;
        AttackerTickets = ReadIntFromPacket(CompressionBasic.DebugIntNonCompressionInfo, ref bufferReadValid);
        DefenderTickets = ReadIntFromPacket(CompressionBasic.DebugIntNonCompressionInfo, ref bufferReadValid);
        return bufferReadValid;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter()
    {
        return MultiplayerMessageFilter.GameMode;
    }

    protected override string OnGetLogFormat()
    {
        return "cRPG Strategus Send Ticket Counts";
    }
}
