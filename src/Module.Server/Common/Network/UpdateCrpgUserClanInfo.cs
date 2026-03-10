using System.IO.Compression;
using Crpg.Module.Api.Models.Clans;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Network;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class UpdateCrpgUserClanInfo : GameNetworkMessage
{
    public VirtualPlayer? Peer { get; set; }
    public CrpgClan Clan { get; set; } = null!;

    protected override void OnWrite()
    {
        WriteVirtualPlayerReferenceToPacket(Peer);
        WriteClanToPacket(Clan);
    }

    protected override bool OnRead()
    {
        bool bufferReadValid = true;
        Peer = ReadVirtualPlayerReferenceToPacket(ref bufferReadValid);
        Clan = ReadClanFromPacket(ref bufferReadValid);
        return bufferReadValid;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter()
    {
        return MultiplayerMessageFilter.GameMode;
    }

    protected override string OnGetLogFormat()
    {
        return "Update cRPG User Clan Info";
    }

    private void WriteClanToPacket(CrpgClan clan)
    {
        using MemoryStream stream = new();

        // This packet is very large so it's compressed.
        using (GZipStream gZipStream = new(stream, CompressionMode.Compress, leaveOpen: true))
        using (BinaryWriter writer = new(gZipStream))
        {
            writer.Write(clan.Id);
            writer.Write(clan.Tag);
            writer.Write(clan.PrimaryColor);
            writer.Write(clan.SecondaryColor);
            writer.Write(clan.Name);
            writer.Write(clan.BannerKey);
        }

        WriteByteArrayToPacket(stream.ToArray(), 0, (int)stream.Length);
    }

    private CrpgClan ReadClanFromPacket(ref bool bufferReadValid)
    {
        byte[] buffer = new byte[1024];
        int bufferLength = ReadByteArrayFromPacket(buffer, 0, buffer.Length, ref bufferReadValid);

        using MemoryStream stream = new(buffer, 0, bufferLength);
        using GZipStream gZipStream = new(stream, CompressionMode.Decompress);
        using BinaryReader reader = new(gZipStream);

        int id = reader.ReadInt32();
        string tag = reader.ReadString();
        uint primaryColor = reader.ReadUInt32();
        uint secondaryColor = reader.ReadUInt32();
        string name = reader.ReadString();
        string bannerKey = reader.ReadString();
        return new CrpgClan
        {
            Id = id,
            Tag = tag,
            PrimaryColor = primaryColor,
            SecondaryColor = secondaryColor,
            Name = name,
            BannerKey = bannerKey,
        };
    }
}
