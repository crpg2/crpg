using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.VoiceCommands;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
internal sealed class VoiceCommandRequested : GameNetworkMessage
{
    private static readonly CompressionInfo.Integer CommandIdCompressionInfo = new(0, CrpgVoiceCommandCatalog.Count - 1, true);

    public int CommandId { get; set; }

    protected override void OnWrite()
    {
        WriteIntToPacket(CommandId, CommandIdCompressionInfo);
    }

    protected override bool OnRead()
    {
        bool bufferReadValid = true;
        CommandId = ReadIntFromPacket(CommandIdCompressionInfo, ref bufferReadValid);
        return bufferReadValid && CrpgVoiceCommandCatalog.IsValid(CommandId);
    }

    protected override MultiplayerMessageFilter OnGetLogFilter()
    {
        return MultiplayerMessageFilter.General;
    }

    protected override string OnGetLogFormat()
    {
        return $"Voice command requested: {CommandId}";
    }
}
