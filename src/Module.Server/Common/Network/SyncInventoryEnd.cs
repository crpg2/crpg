using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Network;

/// <summary>Signals the end of an inventory sync stream. The client should finalize the accumulated items.</summary>
[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class SyncInventoryEnd : GameNetworkMessage
{
    public int SequenceId { get; set; }

    protected override void OnWrite()
    {
        WriteIntToPacket(SequenceId, CompressionBasic.DebugIntNonCompressionInfo);
    }

    protected override bool OnRead()
    {
        bool valid = true;
        SequenceId = ReadIntFromPacket(CompressionBasic.DebugIntNonCompressionInfo, ref valid);
        return valid;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter() => MultiplayerMessageFilter.GameMode;

    protected override string OnGetLogFormat() => "Sync Inventory End";
}
