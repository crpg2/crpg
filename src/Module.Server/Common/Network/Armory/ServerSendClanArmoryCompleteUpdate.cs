using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Network.Armory;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class ServerSendClanArmoryCompleteUpdate : GameNetworkMessage
{
    private static readonly CompressionInfo.Integer ChunkLengthCompressionInfo = new(0, 1024, true);

    public bool IsFirstChunk { get; set; }
    public bool IsLastChunk { get; set; }
    public byte[] ChunkData { get; set; } = [];

    protected override void OnWrite()
    {
        WriteBoolToPacket(IsFirstChunk);
        WriteBoolToPacket(IsLastChunk);
        WriteIntToPacket(ChunkData.Length, ChunkLengthCompressionInfo);
        WriteByteArrayToPacket(ChunkData, 0, ChunkData.Length);
    }

    protected override bool OnRead()
    {
        bool bufferReadValid = true;
        IsFirstChunk = ReadBoolFromPacket(ref bufferReadValid);
        IsLastChunk = ReadBoolFromPacket(ref bufferReadValid);
        int length = ReadIntFromPacket(ChunkLengthCompressionInfo, ref bufferReadValid);

        if (!bufferReadValid || length < 0 || length > 1024)
        {
            return false;
        }

        byte[] data = new byte[length];
        int read = ReadByteArrayFromPacket(data, 0, length, ref bufferReadValid);

        if (!bufferReadValid || read != length)
        {
            Debug.Print($"[ServerSendClanArmoryCompleteUpdate] Chunk read mismatch asked={length} got={read}", 0, Debug.DebugColor.Red);
            return false;
        }

        ChunkData = data;
        return bufferReadValid;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter() => MultiplayerMessageFilter.GameMode;
    protected override string OnGetLogFormat() => $"Server send cRPG clan armory complete update chunk first={IsFirstChunk} last={IsLastChunk}";
}
