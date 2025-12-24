using System.IO.Compression;
using Crpg.Module.Api.Models.Items;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Network.Armory;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class ServerSendClanArmoryCompleteUpdate : GameNetworkMessage
{
    private static readonly CompressionInfo.Integer PacketLengthCompressionInfo = new(0, int.MaxValue, true);
    public int ClanId { get; set; }
    public IList<CrpgClanArmoryItem> ArmoryItems { get; set; } = Array.Empty<CrpgClanArmoryItem>();

    protected override void OnWrite()
    {
        using MemoryStream stream = new();
        using (GZipStream gZipStream = new(stream, CompressionMode.Compress, leaveOpen: true))
        using (BinaryWriter writer = new(gZipStream))
        {
            writer.Write(ClanId);

            // Armory items
            writer.Write(ArmoryItems.Count);
            foreach (var item in ArmoryItems)
            {
                // UserItem
                writer.Write(item.UserItem != null);
                if (item.UserItem != null)
                {
                    writer.Write(item.UserItem.Id);
                    writer.Write(item.UserItem.UserId);
                    writer.Write(item.UserItem.Rank);
                    writer.Write(item.UserItem.ItemId ?? string.Empty);
                    writer.Write(item.UserItem.IsBroken);
                    writer.Write(item.UserItem.IsArmoryItem);
                    writer.Write(item.UserItem.IsPersonal);
                    writer.Write(item.UserItem.CreatedAt.ToBinary());
                }

                // BorrowedItem
                writer.Write(item.BorrowedItem != null);
                if (item.BorrowedItem != null)
                {
                    writer.Write(item.BorrowedItem.BorrowerUserId);
                    writer.Write(item.BorrowedItem.UserItemId);
                    writer.Write(item.BorrowedItem.UpdatedAt.ToBinary());
                }

                // UpdatedAt
                writer.Write(item.UpdatedAt.ToBinary());
            }
        }

        byte[] compressedData = stream.ToArray();
        WriteIntToPacket(compressedData.Length, PacketLengthCompressionInfo);
        WriteByteArrayToPacket(compressedData, 0, compressedData.Length);
    }

    protected override bool OnRead()
    {
        bool bufferReadValid = true;
        byte[]? compressedData = ReadDynamicByteArrayFromPacket(ref bufferReadValid);

        if (!bufferReadValid || compressedData == null || compressedData.Length == 0)
        {
            return false;
        }

        using MemoryStream stream = new(compressedData, writable: false);
        using GZipStream gZipStream = new(stream, CompressionMode.Decompress);
        using BinaryReader reader = new(gZipStream);

        ClanId = reader.ReadInt32();

        int count = reader.ReadInt32();
        List<CrpgClanArmoryItem> items = new(count);

        for (int i = 0; i < count; i++)
        {
            var armoryItem = new CrpgClanArmoryItem();

            // UserItem
            if (reader.ReadBoolean())
            {
                armoryItem.UserItem = new CrpgUserItemExtended
                {
                    Id = reader.ReadInt32(),
                    UserId = reader.ReadInt32(),
                    Rank = reader.ReadInt32(),
                    ItemId = reader.ReadString(),
                    IsBroken = reader.ReadBoolean(),
                    IsArmoryItem = reader.ReadBoolean(),
                    IsPersonal = reader.ReadBoolean(),
                    CreatedAt = DateTime.FromBinary(reader.ReadInt64()),
                };
            }

            // BorrowedItem
            if (reader.ReadBoolean())
            {
                armoryItem.BorrowedItem = new CrpgClanArmoryBorrowedItem
                {
                    BorrowerUserId = reader.ReadInt32(),
                    UserItemId = reader.ReadInt32(),
                    UpdatedAt = DateTime.FromBinary(reader.ReadInt64()),
                };
            }

            // UpdatedAt
            armoryItem.UpdatedAt = DateTime.FromBinary(reader.ReadInt64());

            items.Add(armoryItem);
        }

        ArmoryItems = items;
        return true;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter() => MultiplayerMessageFilter.GameMode;
    protected override string OnGetLogFormat() =>
        $"ClanArmoryCompleteUpdate Clan= {ClanId} ArmoryItems= {ArmoryItems.Count}";

    private byte[]? ReadDynamicByteArrayFromPacket(ref bool bufferReadValid)
    {
        int length = ReadIntFromPacket(PacketLengthCompressionInfo, ref bufferReadValid);
        if (!bufferReadValid || length < 0)
        {
            return null;
        }

        byte[] data = new byte[length];
        int read = ReadByteArrayFromPacket(data, 0, length, ref bufferReadValid);
        return (!bufferReadValid || read != length) ? null : data;
    }
}
