using System.IO.Compression;
using Crpg.Module.Api.Models.Items;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Network.TeamInventory;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class ServerSendTeamItemsList : GameNetworkMessage
{
    private static readonly CompressionInfo.Integer PacketLengthCompressionInfo = new(0, int.MaxValue, true);

    public IList<CrpgTeamInventoryItem> Items { get; set; } = Array.Empty<CrpgTeamInventoryItem>();

    protected override void OnWrite()
    {
        WriteTeamItemsToPacket(Items);
    }

    protected override bool OnRead()
    {
        bool bufferReadValid = true;
        Items = ReadTeamItemsFromPacket(ref bufferReadValid);
        return bufferReadValid;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter()
    {
        return MultiplayerMessageFilter.GameMode;
    }

    protected override string OnGetLogFormat()
    {
        return "Update cRPG Shared Team Items";
    }

    private void WriteTeamItemsToPacket(IList<CrpgTeamInventoryItem> items)
    {
        using MemoryStream stream = new();

        // Compress payload
        using (GZipStream gZipStream = new(stream, CompressionMode.Compress, leaveOpen: true))
        using (BinaryWriter writer = new(gZipStream))
        {
            writer.Write(items.Count);

            foreach (var item in items)
            {
                writer.Write(item.Id ?? string.Empty);
                writer.Write(item.Quantity);
                writer.Write(item.Restricted);
            }
        }

        byte[] compressedData = stream.ToArray();

        // Write length prefix with compression info
        WriteIntToPacket(compressedData.Length, PacketLengthCompressionInfo);
        WriteByteArrayToPacket(compressedData, 0, compressedData.Length);
    }

    private IList<CrpgTeamInventoryItem> ReadTeamItemsFromPacket(ref bool bufferReadValid)
    {
        byte[]? compressedData = ReadDynamicByteArrayFromPacket(ref bufferReadValid);

        if (!bufferReadValid || compressedData == null || compressedData.Length == 0)
        {
            return Array.Empty<CrpgTeamInventoryItem>();
        }

        using MemoryStream stream = new(compressedData, writable: false);
        using GZipStream gZipStream = new(stream, CompressionMode.Decompress);
        using BinaryReader reader = new(gZipStream);

        int count = reader.ReadInt32();
        List<CrpgTeamInventoryItem> items = new(count);

        for (int i = 0; i < count; i++)
        {
            items.Add(new CrpgTeamInventoryItem
            {
                Id = reader.ReadString(),
                Quantity = reader.ReadInt32(),
                Restricted = reader.ReadBoolean(),
            });
        }

        return items;
    }

    private byte[]? ReadDynamicByteArrayFromPacket(ref bool bufferReadValid)
    {
        int length = ReadIntFromPacket(PacketLengthCompressionInfo, ref bufferReadValid);

        if (!bufferReadValid || length < 0)
        {
            return null;
        }

        byte[] data = new byte[length];
        int read = ReadByteArrayFromPacket(data, 0, length, ref bufferReadValid);

        if (!bufferReadValid || read != length)
        {
            return null;
        }

        return data;
    }
}
