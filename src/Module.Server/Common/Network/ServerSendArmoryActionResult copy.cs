using Crpg.Module.Api.Models.Items;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Network;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class ServerSendArmoryActionResultCopy : GameNetworkMessage
{
    public ClanArmoryActionType ActionType { get; set; }
    public bool Success { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public int ClanId { get; set; }
    public int UserItemId { get; set; }
    public int UserId { get; set; }
    public List<CrpgClanArmoryItem> ArmoryItems { get; set; } = new();

    protected override void OnWrite()
    {
        WriteIntToPacket((int)ActionType, CompressionBasic.DebugIntNonCompressionInfo);
        WriteBoolToPacket(Success);
        WriteStringToPacket(ErrorMessage);
        WriteIntToPacket(ClanId, CompressionBasic.DebugIntNonCompressionInfo);
        WriteIntToPacket(UserItemId, CompressionBasic.DebugIntNonCompressionInfo);
        WriteIntToPacket(UserId, CompressionBasic.DebugIntNonCompressionInfo);

        // Write the list count
        WriteIntToPacket(ArmoryItems.Count, CompressionBasic.DebugIntNonCompressionInfo);

        foreach (var item in ArmoryItems)
        {
            // Write UserItem (check for null)
            WriteBoolToPacket(item.UserItem != null);
            if (item.UserItem != null)
            {
                WriteIntToPacket(item.UserItem.Id, CompressionBasic.DebugIntNonCompressionInfo);
                WriteIntToPacket(item.UserItem.UserId, CompressionBasic.DebugIntNonCompressionInfo);
                WriteIntToPacket(item.UserItem.Rank, CompressionBasic.DebugIntNonCompressionInfo);
                WriteStringToPacket(item.UserItem.ItemId);
                WriteBoolToPacket(item.UserItem.IsBroken);
                WriteBoolToPacket(item.UserItem.IsArmoryItem);
                WriteBoolToPacket(item.UserItem.IsPersonal);
                WriteStringToPacket(item.UserItem.CreatedAt.ToString("O")); // ISO 8601
            }

            // Write BorrowedItem (if exists)
            WriteBoolToPacket(item.BorrowedItem != null);
            if (item.BorrowedItem != null)
            {
                WriteIntToPacket(item.BorrowedItem.BorrowerUserId, CompressionBasic.DebugIntNonCompressionInfo);
                WriteIntToPacket(item.BorrowedItem.UserItemId, CompressionBasic.DebugIntNonCompressionInfo);
                WriteStringToPacket(item.BorrowedItem.UpdatedAt.ToString("O"));
            }

            WriteStringToPacket(item.UpdatedAt.ToString("O"));
        }
    }

    protected override bool OnRead()
    {
        bool ok = true;

        ActionType = (ClanArmoryActionType)ReadIntFromPacket(CompressionBasic.DebugIntNonCompressionInfo, ref ok);
        Success = ReadBoolFromPacket(ref ok);
        ErrorMessage = ReadStringFromPacket(ref ok);
        ClanId = ReadIntFromPacket(CompressionBasic.DebugIntNonCompressionInfo, ref ok);
        UserItemId = ReadIntFromPacket(CompressionBasic.DebugIntNonCompressionInfo, ref ok);
        UserId = ReadIntFromPacket(CompressionBasic.DebugIntNonCompressionInfo, ref ok);

        ArmoryItems.Clear();
        int count = ReadIntFromPacket(CompressionBasic.DebugIntNonCompressionInfo, ref ok);

        for (int i = 0; i < count; i++)
        {
            var armoryItem = new CrpgClanArmoryItem();

            // Read UserItem
            if (ReadBoolFromPacket(ref ok))
            {
                armoryItem.UserItem = new CrpgUserItemExtended
                {
                    Id = ReadIntFromPacket(CompressionBasic.DebugIntNonCompressionInfo, ref ok),
                    UserId = ReadIntFromPacket(CompressionBasic.DebugIntNonCompressionInfo, ref ok),
                    Rank = ReadIntFromPacket(CompressionBasic.DebugIntNonCompressionInfo, ref ok),
                    ItemId = ReadStringFromPacket(ref ok),
                    IsBroken = ReadBoolFromPacket(ref ok),
                    IsArmoryItem = ReadBoolFromPacket(ref ok),
                    IsPersonal = ReadBoolFromPacket(ref ok),
                    CreatedAt = DateTime.Parse(ReadStringFromPacket(ref ok)),
                };
            }

            // Read BorrowedItem
            if (ReadBoolFromPacket(ref ok))
            {
                armoryItem.BorrowedItem = new CrpgClanArmoryBorrowedItem
                {
                    BorrowerUserId = ReadIntFromPacket(CompressionBasic.DebugIntNonCompressionInfo, ref ok),
                    UserItemId = ReadIntFromPacket(CompressionBasic.DebugIntNonCompressionInfo, ref ok),
                    UpdatedAt = DateTime.Parse(ReadStringFromPacket(ref ok)),
                };
            }

            armoryItem.UpdatedAt = DateTime.Parse(ReadStringFromPacket(ref ok));

            ArmoryItems.Add(armoryItem);
        }

        return ok;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter() => MultiplayerMessageFilter.GameMode;

    protected override string OnGetLogFormat() =>
        $"ArmoryActionResult (Action={ActionType}, ClanId={ClanId}, UserItemId={UserItemId}, UserId={UserId}, ArmoryItems={ArmoryItems.Count} Success={Success}, Error='{ErrorMessage}')";
}
