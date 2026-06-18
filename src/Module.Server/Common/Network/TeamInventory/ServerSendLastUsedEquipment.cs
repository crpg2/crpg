using Crpg.Module.Api.Models.Items;
using Crpg.Module.Common;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Network.TeamInventory;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class ServerSendLastUsedEquipment : GameNetworkMessage
{
    public Dictionary<CrpgItemSlot, string> Equipment { get; set; } = [];

    protected override void OnWrite()
    {
        WriteIntToPacket(Equipment.Count, CompressionBasic.DebugIntNonCompressionInfo);
        foreach (var kvp in Equipment)
        {
            WriteIntToPacket((int)kvp.Key, CompressionBasic.DebugIntNonCompressionInfo);
            WriteStringToPacket(kvp.Value);
        }
    }

    protected override bool OnRead()
    {
        bool valid = true;
        int count = ReadIntFromPacket(CompressionBasic.DebugIntNonCompressionInfo, ref valid);
        Equipment = new Dictionary<CrpgItemSlot, string>(count);

        for (int i = 0; i < count && valid; i++)
        {
            var slot = (CrpgItemSlot)ReadIntFromPacket(CompressionBasic.DebugIntNonCompressionInfo, ref valid);
            string itemId = ReadStringFromPacket(ref valid);
            Equipment[slot] = itemId;
        }

        return valid;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter() => MultiplayerMessageFilter.GameMode;
    protected override string OnGetLogFormat() => $"ServerSendLastUsedEquipment TeamItems count={Equipment.Count}";
}
