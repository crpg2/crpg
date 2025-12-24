using System.IO.Compression;
using Crpg.Module.Api.Models.Items;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Network.Armory;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class ServerSendClanArmoryItemUpdate : GameNetworkMessage
{
    private static readonly CompressionInfo.Integer PacketLengthCompressionInfo = new(0, int.MaxValue, true);

    public int ClanId { get; set; }
    public ClanArmoryActionType ActionType { get; set; }
    public CrpgClanArmoryItem? ArmoryItem { get; set; }

    protected override void OnWrite()
    {
        using MemoryStream stream = new();
        using (GZipStream gZipStream = new(stream, CompressionMode.Compress, leaveOpen: true))
        using (BinaryWriter writer = new(gZipStream))
        {
            writer.Write(ClanId);
            writer.Write((int)ActionType);

            writer.Write(ArmoryItem != null);
            if (ArmoryItem != null)
            {
                // UserItem
                writer.Write(ArmoryItem.UserItem != null);
                if (ArmoryItem.UserItem != null)
                {
                    writer.Write(ArmoryItem.UserItem.Id);
                    writer.Write(ArmoryItem.UserItem.UserId);
                    writer.Write(ArmoryItem.UserItem.Rank);
                    writer.Write(ArmoryItem.UserItem.ItemId ?? string.Empty);
                    writer.Write(ArmoryItem.UserItem.IsBroken);
                    writer.Write(ArmoryItem.UserItem.IsArmoryItem);
                    writer.Write(ArmoryItem.UserItem.IsPersonal);
                    writer.Write(ArmoryItem.UserItem.CreatedAt.ToBinary());
                }

                // BorrowedItem
                writer.Write(ArmoryItem.BorrowedItem != null);
                if (ArmoryItem.BorrowedItem != null)
                {
                    writer.Write(ArmoryItem.BorrowedItem.BorrowerUserId);
                    writer.Write(ArmoryItem.BorrowedItem.UserItemId);
                    writer.Write(ArmoryItem.BorrowedItem.UpdatedAt.ToBinary());
                }

                // UpdatedAt
                writer.Write(ArmoryItem.UpdatedAt.ToBinary());
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
        ActionType = (ClanArmoryActionType)reader.ReadInt32();

        if (reader.ReadBoolean())
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

            ArmoryItem = armoryItem;
        }

        return true;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter() => MultiplayerMessageFilter.GameMode;

    protected override string OnGetLogFormat() =>
        $"ClanArmoryItemUpdate (ClanId={ClanId}, Action={ActionType}, HasItem={ArmoryItem != null})";

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
