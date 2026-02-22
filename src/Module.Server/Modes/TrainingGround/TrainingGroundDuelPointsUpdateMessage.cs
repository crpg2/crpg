using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Modes.TrainingGround;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
public sealed class TrainingGroundDuelPointsUpdateMessage : GameNetworkMessage
{
    public int NumberOfWins { get; set; }
    public int NumberOfLosses { get; set; }
    public int Rating { get; set; }
    public NetworkCommunicator NetworkCommunicator { get; set; } = null!;

    protected override void OnWrite()
    {
        WriteIntToPacket(NumberOfLosses, CompressionMatchmaker.KillDeathAssistCountCompressionInfo);
        WriteIntToPacket(NumberOfWins, CompressionMatchmaker.KillDeathAssistCountCompressionInfo);
        WriteIntToPacket(Rating, CompressionBasic.DebugIntNonCompressionInfo);
        WriteNetworkPeerReferenceToPacket(NetworkCommunicator);
    }

    protected override bool OnRead()
    {
        bool bufferReadValid = true;
        NumberOfLosses = ReadIntFromPacket(CompressionMatchmaker.KillDeathAssistCountCompressionInfo, ref bufferReadValid);
        NumberOfWins = ReadIntFromPacket(CompressionMatchmaker.KillDeathAssistCountCompressionInfo, ref bufferReadValid);
        Rating = ReadIntFromPacket(CompressionBasic.DebugIntNonCompressionInfo, ref bufferReadValid);
        NetworkCommunicator = ReadNetworkPeerReferenceFromPacket(ref bufferReadValid);
        return bufferReadValid;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter()
    {
        return MultiplayerMessageFilter.GameMode;
    }

    protected override string OnGetLogFormat()
    {
        return "PointUpdateMessage";
    }
}
