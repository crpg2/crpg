using System.IO.Compression;
using Crpg.Module.Api.Models.Characters;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Network;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class ServerSendUserCharacterBasic : GameNetworkMessage
{
    private static readonly CompressionInfo.Integer PacketLengthCompressionInfo = new(0, int.MaxValue, true);

    public CrpgCharacter Character { get; set; } = default!;

    protected override void OnWrite()
    {
        WriteCharacterToPacket(Character);
    }

    protected override bool OnRead()
    {
        bool bufferReadValid = true;
        Character = ReadCharacterFromPacket(ref bufferReadValid);
        return bufferReadValid;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter()
        => MultiplayerMessageFilter.GameMode;

    protected override string OnGetLogFormat()
        => $"Server sent character: {Character?.Name}  Gen:{Character?.Generation} / Level:{Character?.Level}";

    private void WriteCharacterToPacket(CrpgCharacter character)
    {
        using MemoryStream stream = new();

        // Compress the payload
        using (GZipStream gZipStream = new(stream, CompressionMode.Compress, leaveOpen: true))
        using (BinaryWriter writer = new(gZipStream))
        {
            writer.Write(character.Generation);
            writer.Write(character.Level);
            writer.Write(character.Experience);

            // Write attributes
            writer.Write((short)character.Characteristics.Attributes.Points);
            writer.Write((short)character.Characteristics.Attributes.Strength);
            writer.Write((short)character.Characteristics.Attributes.Agility);

            // Write skills
            writer.Write((short)character.Characteristics.Skills.Points);
            writer.Write((short)character.Characteristics.Skills.IronFlesh);
            writer.Write((short)character.Characteristics.Skills.PowerStrike);
            writer.Write((short)character.Characteristics.Skills.PowerDraw);
            writer.Write((short)character.Characteristics.Skills.PowerThrow);
            writer.Write((short)character.Characteristics.Skills.Athletics);
            writer.Write((short)character.Characteristics.Skills.Riding);
            writer.Write((short)character.Characteristics.Skills.WeaponMaster);
            writer.Write((short)character.Characteristics.Skills.MountedArchery);
            writer.Write((short)character.Characteristics.Skills.Shield);
        }

        byte[] compressedData = stream.ToArray();
        WriteIntToPacket(compressedData.Length, PacketLengthCompressionInfo);
        WriteByteArrayToPacket(compressedData, 0, compressedData.Length);
    }

    private CrpgCharacter ReadCharacterFromPacket(ref bool bufferReadValid)
    {
        byte[]? compressedData = ReadDynamicByteArrayFromPacket(ref bufferReadValid);

        if (!bufferReadValid || compressedData == null || compressedData.Length == 0)
        {
            return new CrpgCharacter();
        }

        using MemoryStream stream = new(compressedData, writable: false);
        using GZipStream gZipStream = new(stream, CompressionMode.Decompress);
        using BinaryReader reader = new(gZipStream);

        int generation = reader.ReadInt32();
        int level = reader.ReadInt32();
        int experience = reader.ReadInt32();

        var attributes = new CrpgCharacterAttributes
        {
            Points = reader.ReadInt16(),
            Strength = reader.ReadInt16(),
            Agility = reader.ReadInt16(),
        };

        var skills = new CrpgCharacterSkills
        {
            Points = reader.ReadInt16(),
            IronFlesh = reader.ReadInt16(),
            PowerStrike = reader.ReadInt16(),
            PowerDraw = reader.ReadInt16(),
            PowerThrow = reader.ReadInt16(),
            Athletics = reader.ReadInt16(),
            Riding = reader.ReadInt16(),
            WeaponMaster = reader.ReadInt16(),
            MountedArchery = reader.ReadInt16(),
            Shield = reader.ReadInt16(),
        };

        return new CrpgCharacter
        {
            Generation = generation,
            Level = level,
            Experience = experience,
            Characteristics = new()
            {
                Attributes = attributes,
                Skills = skills,
            },
        };
    }

    private byte[]? ReadDynamicByteArrayFromPacket(ref bool bufferReadValid)
    {
        int length = ReadIntFromPacket(PacketLengthCompressionInfo, ref bufferReadValid);

        if (!bufferReadValid || length < 0)
        {
            return null;
        }

        byte[] data = new byte[length];
        int read = ReadByteArrayFromPacket(data, 0, length, ref bufferReadValid);

        if (!bufferReadValid || read != length)
        {
            return null;
        }

        return data;
    }
}
