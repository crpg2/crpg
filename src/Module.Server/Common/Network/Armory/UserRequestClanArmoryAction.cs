using Crpg.Module.Api.Models;
using Crpg.Module.Api.Models.Items;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Network.Armory;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
public sealed class UserRequestClanArmoryAction : GameNetworkMessage
{
    public ClanArmoryActionRequest Request { get; set; } = default!;
    protected override void OnWrite()
    {
        WriteIntToPacket((int)Request.ActionType, CompressionBasic.DebugIntNonCompressionInfo);
        WriteIntToPacket(Request.ClanId, CompressionBasic.DebugIntNonCompressionInfo);
        WriteIntToPacket(Request.UserItemId, CompressionBasic.DebugIntNonCompressionInfo);
        WriteIntToPacket(Request.UserId, CompressionBasic.DebugIntNonCompressionInfo);
    }

    protected override bool OnRead()
    {
        bool valid = true;

        Request = new ClanArmoryActionRequest
        {
            ActionType = (ClanArmoryActionType)ReadIntFromPacket(CompressionBasic.DebugIntNonCompressionInfo, ref valid),
            ClanId = ReadIntFromPacket(CompressionBasic.DebugIntNonCompressionInfo, ref valid),
            UserItemId = ReadIntFromPacket(CompressionBasic.DebugIntNonCompressionInfo, ref valid),
            UserId = ReadIntFromPacket(CompressionBasic.DebugIntNonCompressionInfo, ref valid),
        };

        return valid;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter() => MultiplayerMessageFilter.GameMode;

    protected override string OnGetLogFormat() =>
        $"Clan Armory Action: {Request.ActionType}, ClanId={Request.ClanId}, UserItemId={Request.UserItemId}, UserId={Request.UserId}";
}

public enum ClanArmoryActionType : byte
{
    Add = 0,
    Remove = 1,
    Borrow = 2,
    Return = 3,
    Get,
}

public sealed class ClanArmoryActionRequest
{
    public ClanArmoryActionType ActionType { get; set; }
    public int ClanId { get; set; }
    public int UserItemId { get; set; }
    public int UserId { get; set; }
}
