using System.IO.Compression;
using Crpg.Module.Api.Models.Items;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Network;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class ServerSendUserInventoryItems : GameNetworkMessage
{
    private static readonly CompressionInfo.Integer PacketLengthCompressionInfo = new(0, int.MaxValue, true);

    // public VirtualPlayer? Peer { get; set; }
    public IList<CrpgUserItemExtended> Items { get; set; } = Array.Empty<CrpgUserItemExtended>();

    protected override void OnWrite()
    {
        // WriteVirtualPlayerReferenceToPacket(Peer);
        WriteUserItemsToPacket(Items);
    }

    protected override bool OnRead()
    {
        bool bufferReadValid = true;
        // Peer = ReadVirtualPlayerReferenceToPacket(ref bufferReadValid);
        Items = ReadUserItemsFromPacket(ref bufferReadValid);
        return bufferReadValid;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter()
    {
        return MultiplayerMessageFilter.GameMode;
    }

    protected override string OnGetLogFormat()
    {
        return "Update cRPG User Inventory Items";
    }

    private void WriteUserItemsToPacket(IList<CrpgUserItemExtended> items)
    {
        using MemoryStream stream = new();

        // Compress payload
        using (GZipStream gZipStream = new(stream, CompressionMode.Compress, leaveOpen: true))
        using (BinaryWriter writer = new(gZipStream))
        {
            writer.Write(items.Count);

            foreach (var item in items)
            {
                writer.Write(item.Id);
                writer.Write(item.UserId);
                writer.Write(item.Rank);
                writer.Write(item.ItemId ?? string.Empty);
                writer.Write(item.IsBroken);
                writer.Write(item.CreatedAt.ToBinary());
                writer.Write(item.IsArmoryItem);
                writer.Write(item.IsPersonal);
            }
        }

        byte[] compressedData = stream.ToArray();

        // Write length prefix with compression info
        WriteIntToPacket(compressedData.Length, PacketLengthCompressionInfo);
        WriteByteArrayToPacket(compressedData, 0, compressedData.Length);
    }

    private IList<CrpgUserItemExtended> ReadUserItemsFromPacket(ref bool bufferReadValid)
    {
        byte[]? compressedData = ReadDynamicByteArrayFromPacket(ref bufferReadValid);

        if (!bufferReadValid || compressedData == null || compressedData.Length == 0)
        {
            return Array.Empty<CrpgUserItemExtended>();
        }

        using MemoryStream stream = new(compressedData, writable: false);
        using GZipStream gZipStream = new(stream, CompressionMode.Decompress);
        using BinaryReader reader = new(gZipStream);

        int count = reader.ReadInt32();
        List<CrpgUserItemExtended> items = new(count);

        for (int i = 0; i < count; i++)
        {
            items.Add(new CrpgUserItemExtended
            {
                Id = reader.ReadInt32(),
                UserId = reader.ReadInt32(),
                Rank = reader.ReadInt32(),
                ItemId = reader.ReadString(),
                IsBroken = reader.ReadBoolean(),
                CreatedAt = DateTime.FromBinary(reader.ReadInt64()),
                IsArmoryItem = reader.ReadBoolean(),
                IsPersonal = reader.ReadBoolean(),
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
