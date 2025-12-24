using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Network.TeamInventory;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class ServerSendTeamItemQuantityUpdated : GameNetworkMessage
{
    public VirtualPlayer? Peer { get; set; }
    public string Item { get; set; } = string.Empty;
    public int Quantity { get; set; }

    protected override void OnWrite()
    {
        WriteVirtualPlayerReferenceToPacket(Peer);
        WriteStringToPacket(Item);
        WriteIntToPacket(Quantity, CompressionBasic.DebugIntNonCompressionInfo);
    }

    protected override bool OnRead()
    {
        bool valid = true;
        Peer = ReadVirtualPlayerReferenceToPacket(ref valid);
        Item = ReadStringFromPacket(ref valid);
        Quantity = ReadIntFromPacket(CompressionBasic.DebugIntNonCompressionInfo, ref valid);
        return valid;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter()
    {
        return MultiplayerMessageFilter.GameMode;
    }

    protected override string OnGetLogFormat()
    {
        return $"Server Send cRPG Team Item Quantity Updated {Item} : {Quantity}";
    }
}
