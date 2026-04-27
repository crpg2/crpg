using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Network.TeamInventory;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class ServerSendTeamItemsQuantityUpdated : GameNetworkMessage
{
    public VirtualPlayer? Peer { get; set; }
    public Dictionary<string, int> Items { get; set; } = [];

    protected override void OnWrite()
    {
        WriteVirtualPlayerReferenceToPacket(Peer);
        WriteIntToPacket(Items.Count, CompressionBasic.DebugIntNonCompressionInfo);
        foreach (KeyValuePair<string, int> kvp in Items)
        {
            WriteStringToPacket(kvp.Key);
            WriteIntToPacket(kvp.Value, CompressionBasic.DebugIntNonCompressionInfo);
        }
    }

    protected override bool OnRead()
    {
        bool valid = true;
        Peer = ReadVirtualPlayerReferenceToPacket(ref valid);
        int count = ReadIntFromPacket(CompressionBasic.DebugIntNonCompressionInfo, ref valid);
        Items = [];
        for (int i = 0; i < count; i++)
        {
            string id = ReadStringFromPacket(ref valid);
            int quantity = ReadIntFromPacket(CompressionBasic.DebugIntNonCompressionInfo, ref valid);
            Items[id] = quantity;
        }

        return valid;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter() => MultiplayerMessageFilter.GameMode;

    protected override string OnGetLogFormat() => $"Server Send cRPG All Shared Team Items Returned for peer {Peer?.UserName}";
}
