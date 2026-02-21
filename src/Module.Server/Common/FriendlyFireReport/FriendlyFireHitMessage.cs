using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.FriendlyFireReport;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class FriendlyFireHitMessage : GameNetworkMessage
{
    public int AttackerAgentIndex { get; private set; }
    public int Damage { get; private set; }
    public int ReportWindow { get; private set; }
    private readonly CompressionInfo.Integer reportWindowCompressionInfo = new(0, 200, true);

    public FriendlyFireHitMessage()
    {
        // Default constructor for deserialization
    }

    public FriendlyFireHitMessage(int attackerAgentIndex, int damage, int reportWindow)
    {
        AttackerAgentIndex = attackerAgentIndex;
        Damage = damage;
        ReportWindow = reportWindow;
    }

    protected override bool OnRead()
    {
        bool bufferReadValid = true;
        AttackerAgentIndex = ReadAgentIndexFromPacket(ref bufferReadValid);
        Damage = ReadIntFromPacket(CompressionBasic.AgentHitDamageCompressionInfo, ref bufferReadValid);
        ReportWindow = ReadIntFromPacket(reportWindowCompressionInfo, ref bufferReadValid);
        return bufferReadValid;
    }

    protected override void OnWrite()
    {
        WriteAgentIndexToPacket(AttackerAgentIndex);
        WriteIntToPacket(Damage, CompressionBasic.AgentHitDamageCompressionInfo);
        WriteIntToPacket(ReportWindow, reportWindowCompressionInfo);
    }

    protected override MultiplayerMessageFilter OnGetLogFilter()
    {
        return MultiplayerMessageFilter.General;
    }

    protected override string OnGetLogFormat()
    {
        return $"Hit by agent index {AttackerAgentIndex} for {Damage} damage. window: {ReportWindow}";
    }
}
