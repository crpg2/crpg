using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Rewards;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class CrpgRewardHappyHour : GameNetworkMessage
{
    public bool Started { get; set; }
    public float ExpMultiplier { get; set; }

    protected override void OnWrite()
    {
        WriteBoolToPacket(Started);
        WriteFloatToPacket(ExpMultiplier, new CompressionInfo.Float(0f, 2f, 8));
    }

    protected override bool OnRead()
    {
        bool bufferReadValid = true;
        Started = ReadBoolFromPacket(ref bufferReadValid);
        ExpMultiplier = ReadFloatFromPacket(new CompressionInfo.Float(0f, 2f, 8), ref bufferReadValid);
        return bufferReadValid;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter()
    {
        return MultiplayerMessageFilter.GameMode;
    }

    protected override string OnGetLogFormat()
    {
        return $"{nameof(CrpgRewardHappyHour)}: Started={Started}, ExpMultiplier={ExpMultiplier}";
    }
}
