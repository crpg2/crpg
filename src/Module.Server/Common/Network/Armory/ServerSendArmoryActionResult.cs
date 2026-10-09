using Crpg.Module.Api.Models.Items;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Network.Armory;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class ServerSendArmoryActionResult : GameNetworkMessage
{
    private static readonly CompressionInfo.Integer IntCompressionInfo = CompressionBasic.DebugIntNonCompressionInfo;

    public ClanArmoryActionType ActionType { get; set; }
    public bool Success { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public int ClanId { get; set; }
    public int UserItemId { get; set; }
    public int UserId { get; set; }
    public IList<CrpgClanArmoryItem> ArmoryItems { get; set; } = [];

    protected override void OnWrite()
    {
        WriteBoolToPacket(Success);
        WriteStringToPacket(ErrorMessage ?? string.Empty);
        WriteIntToPacket((int)ActionType, IntCompressionInfo);
        WriteIntToPacket(ClanId, IntCompressionInfo);
        WriteIntToPacket(UserItemId, IntCompressionInfo);
        WriteIntToPacket(UserId, IntCompressionInfo);
    }

    protected override bool OnRead()
    {
        bool ok = true;
        Success = ReadBoolFromPacket(ref ok);
        ErrorMessage = ReadStringFromPacket(ref ok);
        ActionType = (ClanArmoryActionType)ReadIntFromPacket(IntCompressionInfo, ref ok);
        ClanId = ReadIntFromPacket(IntCompressionInfo, ref ok);
        UserItemId = ReadIntFromPacket(IntCompressionInfo, ref ok);
        UserId = ReadIntFromPacket(IntCompressionInfo, ref ok);
        return ok;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter() => MultiplayerMessageFilter.GameMode;
    protected override string OnGetLogFormat() =>
        $"Server send cRPG ArmoryActionResult (Action={ActionType}, Success={Success}, Error='{ErrorMessage}')";
}
