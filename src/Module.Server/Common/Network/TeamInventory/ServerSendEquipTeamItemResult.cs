using Crpg.Module.Api.Models.Items;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Network.TeamInventory;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class ServerSendEquipTeamItemResult : GameNetworkMessage
{
    public VirtualPlayer? Peer { get; set; }
    public CrpgItemSlot Slot { get; set; }
    public string Item { get; set; } = string.Empty;
    public bool Equipped { get; set; }
    public int Quantity { get; set; }
    private static readonly CompressionInfo.Integer SlotCompression = new(0, 15, maximumValueGiven: true);

    protected override void OnWrite()
    {
        WriteVirtualPlayerReferenceToPacket(Peer);
        WriteStringToPacket(Item);
        WriteIntToPacket((int)Slot, SlotCompression);
        WriteBoolToPacket(Equipped);
        WriteIntToPacket(Quantity, CompressionBasic.DebugIntNonCompressionInfo);
    }

    protected override bool OnRead()
    {
        bool valid = true;
        Peer = ReadVirtualPlayerReferenceToPacket(ref valid);
        Item = ReadStringFromPacket(ref valid);
        Slot = (CrpgItemSlot)ReadIntFromPacket(SlotCompression, ref valid);
        Equipped = ReadBoolFromPacket(ref valid);
        Quantity = ReadIntFromPacket(CompressionBasic.DebugIntNonCompressionInfo, ref valid);
        return valid;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter()
    {
        return MultiplayerMessageFilter.GameMode;
    }

    protected override string OnGetLogFormat()
    {
        return "Server Send cRPG Shared Team Item Used";
    }
}
