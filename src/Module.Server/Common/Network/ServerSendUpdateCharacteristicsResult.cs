using System.IO.Compression;
using Crpg.Module.Api.Models.Characters;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Network;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class ServerSendUpdateCharacteristicsResult : GameNetworkMessage
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public CrpgCharacterCharacteristics Characteristics { get; set; } = default!;
    private static readonly CompressionInfo.Integer PacketLengthCompressionInfo = new(0, int.MaxValue, true);

    protected override void OnWrite()
    {
        WriteBoolToPacket(Success);
        WriteStringToPacket(ErrorMessage);
        WriteCharacteristicsToPacket(Characteristics);
    }

    protected override bool OnRead()
    {
        bool ok = true;
        Success = ReadBoolFromPacket(ref ok);
        ErrorMessage = ReadStringFromPacket(ref ok);
        Characteristics = ReadCharacteristicsFromPacket(ref ok);
        return ok;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter() => MultiplayerMessageFilter.GameMode;
    protected override string OnGetLogFormat() =>
        $"Server sent api update characteristics result:";

    private void WriteCharacteristicsToPacket(CrpgCharacterCharacteristics characteristics)
    {
        using MemoryStream stream = new();

        // Compress the payload
        using (GZipStream gZipStream = new(stream, CompressionMode.Compress, leaveOpen: true))
        using (BinaryWriter writer = new(gZipStream))
        {
            // Write attributes
            writer.Write((short)characteristics.Attributes.Points);
            writer.Write((short)characteristics.Attributes.Strength);
            writer.Write((short)characteristics.Attributes.Agility);

            // Write skills
            writer.Write((short)characteristics.Skills.Points);
            writer.Write((short)characteristics.Skills.IronFlesh);
            writer.Write((short)characteristics.Skills.PowerStrike);
            writer.Write((short)characteristics.Skills.PowerDraw);
            writer.Write((short)characteristics.Skills.PowerThrow);
            writer.Write((short)characteristics.Skills.Athletics);
            writer.Write((short)characteristics.Skills.Riding);
            writer.Write((short)characteristics.Skills.WeaponMaster);
            writer.Write((short)characteristics.Skills.MountedArchery);
            writer.Write((short)characteristics.Skills.Shield);

            // Write Proficiencies
            writer.Write((short)characteristics.WeaponProficiencies.Points);
            writer.Write((short)characteristics.WeaponProficiencies.OneHanded);
            writer.Write((short)characteristics.WeaponProficiencies.TwoHanded);
            writer.Write((short)characteristics.WeaponProficiencies.Polearm);
            writer.Write((short)characteristics.WeaponProficiencies.Bow);
            writer.Write((short)characteristics.WeaponProficiencies.Crossbow);
            writer.Write((short)characteristics.WeaponProficiencies.Throwing);
        }

        byte[] compressedData = stream.ToArray();
        WriteIntToPacket(compressedData.Length, PacketLengthCompressionInfo);
        WriteByteArrayToPacket(compressedData, 0, compressedData.Length);
    }

    private CrpgCharacterCharacteristics ReadCharacteristicsFromPacket(ref bool bufferReadValid)
    {
        byte[]? compressedData = ReadDynamicByteArrayFromPacket(ref bufferReadValid);

        if (!bufferReadValid || compressedData == null || compressedData.Length == 0)
        {
            return new CrpgCharacterCharacteristics();
        }

        using MemoryStream stream = new(compressedData, writable: false);
        using GZipStream gZipStream = new(stream, CompressionMode.Decompress);
        using BinaryReader reader = new(gZipStream);

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

        var proficiencies = new CrpgCharacterWeaponProficiencies
        {
            Points = reader.ReadInt16(),
            OneHanded = reader.ReadInt16(),
            TwoHanded = reader.ReadInt16(),
            Polearm = reader.ReadInt16(),
            Bow = reader.ReadInt16(),
            Crossbow = reader.ReadInt16(),
            Throwing = reader.ReadInt16(),
        };

        return new CrpgCharacterCharacteristics
        {
            Attributes = attributes,
            Skills = skills,
            WeaponProficiencies = proficiencies,
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
