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
        AttackerAgentIndex = GameNetworkMessage.ReadAgentIndexFromPacket(ref bufferReadValid);
        Damage = GameNetworkMessage.ReadIntFromPacket(CompressionBasic.AgentHitDamageCompressionInfo, ref bufferReadValid);
        ReportWindow = GameNetworkMessage.ReadIntFromPacket(reportWindowCompressionInfo, ref bufferReadValid);
        return bufferReadValid;
    }

    protected override void OnWrite()
    {
        GameNetworkMessage.WriteAgentIndexToPacket(AttackerAgentIndex);
        GameNetworkMessage.WriteIntToPacket(Damage, CompressionBasic.AgentHitDamageCompressionInfo);
        GameNetworkMessage.WriteIntToPacket(ReportWindow, reportWindowCompressionInfo);
    }

    protected override MultiplayerMessageFilter OnGetLogFilter()
    {
        return MultiplayerMessageFilter.General;
    }

    protected override string OnGetLogFormat()
    {
        return $"[FF Message] Hit by agent index {AttackerAgentIndex} for {Damage} damage. window: {ReportWindow}";
    }
}
