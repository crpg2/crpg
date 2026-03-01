using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;
using TaleWorlds.ObjectSystem;

namespace Crpg.Module.Common.Network;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
internal sealed class RequestEquipItem : GameNetworkMessage
{
    /// <summary>Item to equip. Null means unequip the slot.</summary>
    public ItemObject? Item { get; set; }

    /// <summary>Target equipment slot.</summary>
    public EquipmentIndex Slot { get; set; }

    protected override void OnWrite()
    {
        WriteObjectReferenceToPacket(Item, CompressionBasic.GUIDCompressionInfo);
        WriteIntToPacket((int)Slot, CompressionBasic.DebugIntNonCompressionInfo);
    }

    protected override bool OnRead()
    {
        bool bufferReadValid = true;
        Item = (ItemObject?)ReadObjectReferenceFromPacket(MBObjectManager.Instance, CompressionBasic.GUIDCompressionInfo, ref bufferReadValid);
        Slot = (EquipmentIndex)ReadIntFromPacket(CompressionBasic.DebugIntNonCompressionInfo, ref bufferReadValid);

        return bufferReadValid;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter() => MultiplayerMessageFilter.GameMode;

    protected override string OnGetLogFormat() => "Request Equip Item";
}
