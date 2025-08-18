using System.IO.Compression;
using Crpg.Module.Api.Models.Items;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Network;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class ServerSendUserCharacterEquippedItems : GameNetworkMessage
{
    private static readonly CompressionInfo.Integer PacketLengthCompressionInfo = new(0, int.MaxValue, true);

    public IList<CrpgEquippedItemExtended> Items { get; set; } = Array.Empty<CrpgEquippedItemExtended>();

    protected override void OnWrite()
    {
        WriteEquippedItemsToPacket(Items);
    }

    protected override bool OnRead()
    {
        bool bufferReadValid = true;
        Items = ReadEquippedItemsFromPacket(ref bufferReadValid);
        return bufferReadValid;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter()
        => MultiplayerMessageFilter.GameMode;

    protected override string OnGetLogFormat()
        => $"Server sent {Items.Count} equipped items";

    private void WriteEquippedItemsToPacket(IList<CrpgEquippedItemExtended> items)
    {
        using MemoryStream stream = new();

        // Compress payload
        using (GZipStream gZipStream = new(stream, CompressionMode.Compress, leaveOpen: true))
        using (BinaryWriter writer = new(gZipStream))
        {
            writer.Write(items.Count);

            foreach (var item in items)
            {
                writer.Write((int)item.Slot);

                // Write UserItem
                writer.Write(item.UserItem.Id);
                writer.Write(item.UserItem.UserId);
                writer.Write(item.UserItem.Rank);
                writer.Write(item.UserItem.ItemId ?? string.Empty);
                writer.Write(item.UserItem.IsBroken);
                writer.Write(item.UserItem.CreatedAt.ToBinary());
                writer.Write(item.UserItem.IsArmoryItem);
                writer.Write(item.UserItem.IsPersonal);
            }
        }

        byte[] compressedData = stream.ToArray();
        WriteIntToPacket(compressedData.Length, PacketLengthCompressionInfo);
        WriteByteArrayToPacket(compressedData, 0, compressedData.Length);
    }

    private IList<CrpgEquippedItemExtended> ReadEquippedItemsFromPacket(ref bool bufferReadValid)
    {
        byte[]? compressedData = ReadDynamicByteArrayFromPacket(ref bufferReadValid);

        if (!bufferReadValid || compressedData == null || compressedData.Length == 0)
        {
            return Array.Empty<CrpgEquippedItemExtended>();
        }

        using MemoryStream stream = new(compressedData, writable: false);
        using GZipStream gZipStream = new(stream, CompressionMode.Decompress);
        using BinaryReader reader = new(gZipStream);

        int count = reader.ReadInt32();
        List<CrpgEquippedItemExtended> items = new(count);

        for (int i = 0; i < count; i++)
        {
            var slot = (CrpgItemSlot)reader.ReadInt32();

            var userItem = new CrpgUserItemExtended
            {
                Id = reader.ReadInt32(),
                UserId = reader.ReadInt32(),
                Rank = reader.ReadInt32(),
                ItemId = reader.ReadString(),
                IsBroken = reader.ReadBoolean(),
                CreatedAt = DateTime.FromBinary(reader.ReadInt64()),
                IsArmoryItem = reader.ReadBoolean(),
                IsPersonal = reader.ReadBoolean(),
            };

            items.Add(new CrpgEquippedItemExtended
            {
                Slot = slot,
                UserItem = userItem,
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
