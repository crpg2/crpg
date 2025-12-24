using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Network.CharacterLoadout;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class ServerSendUserInventoryItems : GameNetworkMessage
{
    private static readonly CompressionInfo.Integer ChunkLengthCompressionInfo = new(0, 1024, true);

    public bool IsFirstChunk { get; set; }
    public bool IsLastChunk { get; set; }
    public byte[] ChunkData { get; set; } = Array.Empty<byte>();

    protected override void OnWrite()
    {
        WriteBoolToPacket(IsFirstChunk);
        WriteBoolToPacket(IsLastChunk);
        WriteIntToPacket(ChunkData.Length, ChunkLengthCompressionInfo);
        WriteByteArrayToPacket(ChunkData, 0, ChunkData.Length);
        // Debug.Print($"[ServerSendUserInventoryItems] Sent chunk size={ChunkData.Length} first={IsFirstChunk} last={IsLastChunk}", 0, Debug.DebugColor.Cyan);
    }

    protected override bool OnRead()
    {
        bool bufferReadValid = true;
        IsFirstChunk = ReadBoolFromPacket(ref bufferReadValid);
        IsLastChunk = ReadBoolFromPacket(ref bufferReadValid);
        int length = ReadIntFromPacket(ChunkLengthCompressionInfo, ref bufferReadValid);

        if (!bufferReadValid || length < 0 || length > 1024)
        {
            Debug.Print($"[ServerSendUserInventoryItems] Invalid chunk length={length}", 0, Debug.DebugColor.Red);
            return false;
        }

        byte[] data = new byte[length];
        int read = ReadByteArrayFromPacket(data, 0, length, ref bufferReadValid);

        if (!bufferReadValid || read != length)
        {
            Debug.Print($"[ServerSendUserInventoryItems] Chunk read mismatch asked={length} got={read}", 0, Debug.DebugColor.Red);
            return false;
        }

        ChunkData = data;
        return bufferReadValid;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter() => MultiplayerMessageFilter.GameMode;
    protected override string OnGetLogFormat() => "Server send cRPG user inventory items Chunk ";
}
