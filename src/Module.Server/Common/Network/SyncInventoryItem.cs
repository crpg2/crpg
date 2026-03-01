using Crpg.Module.Api.Models.Items;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;
using TaleWorlds.ObjectSystem;

namespace Crpg.Module.Common.Network;

/// <summary>Sends a single owned item as part of an inventory sync stream.</summary>
[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class SyncInventoryItem : GameNetworkMessage
{
    private static readonly CompressionInfo.Integer RankCompressionInfo = new(0, 3, true);

    public int SequenceId { get; set; }
    public ItemObject Item { get; set; } = null!;
    public int Rank { get; set; }
    public bool IsBroken { get; set; }

    protected override void OnWrite()
    {
        WriteIntToPacket(SequenceId, CompressionBasic.DebugIntNonCompressionInfo);
        WriteObjectReferenceToPacket(Item, CompressionBasic.GUIDCompressionInfo);
        WriteIntToPacket(Rank, RankCompressionInfo);
        WriteBoolToPacket(IsBroken);
    }

    protected override bool OnRead()
    {
        bool valid = true;
        SequenceId = ReadIntFromPacket(CompressionBasic.DebugIntNonCompressionInfo, ref valid);
        Item = (ItemObject)ReadObjectReferenceFromPacket(MBObjectManager.Instance, CompressionBasic.GUIDCompressionInfo, ref valid);
        Rank = ReadIntFromPacket(RankCompressionInfo, ref valid);
        IsBroken = ReadBoolFromPacket(ref valid);

        return valid;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter() => MultiplayerMessageFilter.GameMode;

    protected override string OnGetLogFormat() => "Sync Inventory Item";
}
