using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;
using TaleWorlds.MountAndBlade.Objects;

namespace Crpg.Module.Modes.Battle;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class CrpgBattleSpawnFlagMessage : GameNetworkMessage
{
    private static readonly CompressionInfo.Integer FlagCapturePointCharCompressionInfo = new(65, 5);
    public int FlagChar { get; set; } = 0!;
    public float Time { get; set; } = 0!;

    protected override void OnWrite()
    {
        WriteIntToPacket(FlagChar, FlagCapturePointCharCompressionInfo);
        WriteFloatToPacket(Time, CompressionInfo.Float.FullPrecision);
    }

    protected override bool OnRead()
    {
        bool bufferReadValid = true;
        FlagChar = ReadIntFromPacket(FlagCapturePointCharCompressionInfo, ref bufferReadValid);
        Time = ReadFloatFromPacket(CompressionInfo.Float.FullPrecision, ref bufferReadValid);
        return bufferReadValid;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter()
    {
        return MultiplayerMessageFilter.GameMode;
    }

    protected override string OnGetLogFormat()
    {
        return "Random Flag Spawned";
    }
}
