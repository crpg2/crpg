using Crpg.Module.Api.Models.Items;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Network.TeamInventory;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
internal sealed class UserRequestEquipTeamItem : GameNetworkMessage
{
    public CrpgItemSlot Slot { get; set; }
    public string Item { get; set; } = string.Empty;
    public bool Equip { get; set; }
    private static readonly CompressionInfo.Integer SlotCompression = new(0, 15, maximumValueGiven: true);

    protected override void OnWrite()
    {
        WriteIntToPacket((int)Slot, SlotCompression);
        WriteStringToPacket(Item);
        WriteBoolToPacket(Equip);
    }

    protected override bool OnRead()
    {
        bool bufferValid = true;
        Slot = (CrpgItemSlot)ReadIntFromPacket(SlotCompression, ref bufferValid);
        Item = ReadStringFromPacket(ref bufferValid);
        Equip = ReadBoolFromPacket(ref bufferValid);
        return bufferValid;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter() => MultiplayerMessageFilter.GameMode;

    protected override string OnGetLogFormat() => $"Request cRPG Equip/Unequip Shared Team Item {Item}";
}
