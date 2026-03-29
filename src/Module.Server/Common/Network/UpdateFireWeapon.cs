using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Network;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class UpdateFireWeapon : GameNetworkMessage
{
    public int AgentIndex { get; set; }
    public bool Enabled { get; set; }
    public string ParticleSystemId { get; set; } = string.Empty;
    public Vec3 LightColor { get; set; }
    public float LightRadius { get; set; }
    public float LightIntensity { get; set; }

    private static readonly CompressionInfo.Float ColorCompression = new(0f, 255f, 10);
    private static readonly CompressionInfo.Float LightRadiusCompression = new(0f, 100f, 10);
    private static readonly CompressionInfo.Float LightIntensityCompression = new(0f, 100f, 10);

    protected override void OnWrite()
    {
        WriteAgentIndexToPacket(AgentIndex);
        WriteBoolToPacket(Enabled);
        WriteStringToPacket(ParticleSystemId);
        WriteVec3ToPacket(LightColor, ColorCompression);
        WriteFloatToPacket(LightRadius, LightRadiusCompression);
        WriteFloatToPacket(LightIntensity, LightIntensityCompression);
    }

    protected override bool OnRead()
    {
        bool bufferReadValid = true;
        AgentIndex = ReadAgentIndexFromPacket(ref bufferReadValid);
        Enabled = ReadBoolFromPacket(ref bufferReadValid);
        ParticleSystemId = ReadStringFromPacket(ref bufferReadValid);
        LightColor = ReadVec3FromPacket(ColorCompression, ref bufferReadValid);
        LightRadius = ReadFloatFromPacket(LightRadiusCompression, ref bufferReadValid);
        LightIntensity = ReadFloatFromPacket(LightIntensityCompression, ref bufferReadValid);
        return bufferReadValid;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter()
    {
        return MultiplayerMessageFilter.GameMode;
    }

    protected override string OnGetLogFormat()
    {
        return $"UpdateFireWeapon: Agent: {AgentIndex} Enabled: {Enabled} Id: {ParticleSystemId} Color: {LightColor}  Radius: {LightRadius} Intensity: {LightIntensity}";
    }
}
