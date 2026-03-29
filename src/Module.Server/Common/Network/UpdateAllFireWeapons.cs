using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Network;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class UpdateAllFireWeapons : GameNetworkMessage
{
    public struct FireWeaponData
    {
        public int AgentIndex;
        public bool Enabled;
        public string ParticleSystemId;
        public Vec3 LightColor;
        public float LightRadius;
        public float LightIntensity;
    }

    public List<FireWeaponData> Entries { get; set; } = new();

    private static readonly CompressionInfo.Float ColorCompression = new(0f, 255f, 10);
    private static readonly CompressionInfo.Float LightRadiusCompression = new(0f, 100f, 10);
    private static readonly CompressionInfo.Float LightIntensityCompression = new(0f, 100f, 10);
    private static readonly CompressionInfo.Integer CountCompression = new(0, 10);

    protected override void OnWrite()
    {
        WriteIntToPacket(Entries.Count, CountCompression);
        foreach (var entry in Entries)
        {
            WriteAgentIndexToPacket(entry.AgentIndex);
            WriteBoolToPacket(entry.Enabled);
            WriteStringToPacket(entry.ParticleSystemId);
            WriteVec3ToPacket(entry.LightColor, ColorCompression);
            WriteFloatToPacket(entry.LightRadius, LightRadiusCompression);
            WriteFloatToPacket(entry.LightIntensity, LightIntensityCompression);
        }
    }

    protected override bool OnRead()
    {
        bool bufferReadValid = true;
        int count = ReadIntFromPacket(CountCompression, ref bufferReadValid);
        Entries = new List<FireWeaponData>(count);
        for (int i = 0; i < count; i++)
        {
            Entries.Add(new FireWeaponData
            {
                AgentIndex = ReadAgentIndexFromPacket(ref bufferReadValid),
                Enabled = ReadBoolFromPacket(ref bufferReadValid),
                ParticleSystemId = ReadStringFromPacket(ref bufferReadValid),
                LightColor = ReadVec3FromPacket(ColorCompression, ref bufferReadValid),
                LightRadius = ReadFloatFromPacket(LightRadiusCompression, ref bufferReadValid),
                LightIntensity = ReadFloatFromPacket(LightIntensityCompression, ref bufferReadValid),
            });
        }

        return bufferReadValid;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter() => MultiplayerMessageFilter.GameMode;
    protected override string OnGetLogFormat() => $"UpdateAllFireWeapons: {Entries.Count} entries";
}
