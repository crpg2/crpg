using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.FriendlyFireReport;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class FriendlyFireNotificationMessage : GameNetworkMessage
{
    public string Message { get; private set; }
    public FriendlyFireMessageMode Mode { get; private set; }
    private readonly CompressionInfo.Integer messageModeCompressionInfo = new(
        0,
        (int)Enum.GetValues(typeof(FriendlyFireMessageMode)).Cast<FriendlyFireMessageMode>().Max(),
        true);

    public FriendlyFireNotificationMessage()
    {
        Message = string.Empty; // Default constructor for deserialization
        Mode = FriendlyFireMessageMode.Default; // Default mode
    }

    public FriendlyFireNotificationMessage(string message, FriendlyFireMessageMode mode)
    {
        Message = message;
        Mode = mode;
    }

    protected override bool OnRead()
    {
        bool bufferReadValid = true;
        Message = ReadStringFromPacket(ref bufferReadValid);
        Mode = (FriendlyFireMessageMode)ReadIntFromPacket(messageModeCompressionInfo, ref bufferReadValid);
        return bufferReadValid;
    }

    protected override void OnWrite()
    {
        WriteStringToPacket(Message);
        WriteIntToPacket((int)Mode, messageModeCompressionInfo);
    }

    protected override MultiplayerMessageFilter OnGetLogFilter()
    {
        return MultiplayerMessageFilter.General;
    }

    protected override string OnGetLogFormat()
    {
        return $"({Mode}) {Message}";
    }
}
