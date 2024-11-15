using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Network;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class CrpgUpdateRespawnTimerMessage : GameNetworkMessage
{
    public float TimeToSpawn { get; set; }

    protected override void OnWrite()
    {
        WriteFloatToPacket(TimeToSpawn, CompressionInfo.Float.FullPrecision);
    }

    protected override bool OnRead()
    {
        bool bufferReadValid = true;
        TimeToSpawn = ReadFloatFromPacket(CompressionInfo.Float.FullPrecision, ref bufferReadValid);
        return bufferReadValid;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter()
    {
        return MultiplayerMessageFilter.GameMode;
    }

    protected override string OnGetLogFormat()
    {
        return "Update time to respawn";
    }
}
