using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Network;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class ServerSendEquipItemResult : GameNetworkMessage
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public int SlotIndex { get; set; }
    public int UserItemId { get; set; } = -1; // -1 means unequip

    private static readonly CompressionInfo.Integer SlotCompressionInfo = new(0, 15);

    protected override void OnWrite()
    {
        WriteBoolToPacket(Success);
        WriteStringToPacket(ErrorMessage);
        WriteIntToPacket(SlotIndex, SlotCompressionInfo);
        WriteIntToPacket(UserItemId, CompressionBasic.DebugIntNonCompressionInfo);
    }

    protected override bool OnRead()
    {
        bool ok = true;
        Success = ReadBoolFromPacket(ref ok);
        ErrorMessage = ReadStringFromPacket(ref ok);
        SlotIndex = ReadIntFromPacket(SlotCompressionInfo, ref ok);
        UserItemId = ReadIntFromPacket(CompressionBasic.DebugIntNonCompressionInfo, ref ok);
        return ok;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter() => MultiplayerMessageFilter.GameMode;
    protected override string OnGetLogFormat() =>
        $"EquipItemResult (Slot={SlotIndex}, UserItemId={UserItemId}, Success={Success}, Error={ErrorMessage})";
}
