using Crpg.Module.Api.Models.Items;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Network;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
public sealed class UserRequestEquipCharacterItem : GameNetworkMessage
{
    public CrpgItemSlot Slot { get; set; }
    public int UserItemId { get; set; } // -1 to unequip

    private readonly CompressionInfo.Integer _slotCompression = new(0, 15, maximumValueGiven: true); // CrpgItemSlot enum 0-15

    protected override void OnWrite()
    {
        WriteIntToPacket((int)Slot, _slotCompression);
        WriteIntToPacket(UserItemId, CompressionBasic.DebugIntNonCompressionInfo);
    }

    protected override bool OnRead()
    {
        bool bufferValid = true;
        Slot = (CrpgItemSlot)ReadIntFromPacket(_slotCompression, ref bufferValid);
        UserItemId = ReadIntFromPacket(CompressionBasic.DebugIntNonCompressionInfo, ref bufferValid);

        return bufferValid;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter() => MultiplayerMessageFilter.GameMode;

    protected override string OnGetLogFormat() => $"Request Equip item: Slot {Slot}, UserItemId {UserItemId}";
}
