using Crpg.Module.Api.Models.Items;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Network;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
public sealed class UserRequestSwapCharacterItemMessage : GameNetworkMessage
{
    public CrpgItemSlot SourceSlot { get; set; }
    public int SourceUserItemId { get; set; } // -1 if empty

    public CrpgItemSlot TargetSlot { get; set; }
    public int TargetUserItemId { get; set; } // -1 if empty

    private readonly CompressionInfo.Integer _slotCompression = new(0, 15, maximumValueGiven: true);
    private readonly CompressionInfo.Integer _userItemCompression = CompressionBasic.DebugIntNonCompressionInfo;

    protected override void OnWrite()
    {
        WriteIntToPacket((int)SourceSlot, _slotCompression);
        WriteIntToPacket(SourceUserItemId, _userItemCompression);
        WriteIntToPacket((int)TargetSlot, _slotCompression);
        WriteIntToPacket(TargetUserItemId, _userItemCompression);
    }

    protected override bool OnRead()
    {
        bool bufferValid = true;

        SourceSlot = (CrpgItemSlot)ReadIntFromPacket(_slotCompression, ref bufferValid);
        SourceUserItemId = ReadIntFromPacket(_userItemCompression, ref bufferValid);

        TargetSlot = (CrpgItemSlot)ReadIntFromPacket(_slotCompression, ref bufferValid);
        TargetUserItemId = ReadIntFromPacket(_userItemCompression, ref bufferValid);

        return bufferValid;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter() => MultiplayerMessageFilter.GameMode;

    protected override string OnGetLogFormat() =>
        $"Request Swap item: SourceSlot {SourceSlot}, SourceUserItemId {SourceUserItemId} -> TargetSlot {TargetSlot}, TargetUserItemId {TargetUserItemId}";
}
