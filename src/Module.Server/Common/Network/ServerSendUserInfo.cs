using System.IO.Compression;
using Crpg.Module.Api.Models;
using Crpg.Module.Api.Models.Characters;
using Crpg.Module.Api.Models.Clans;
using Crpg.Module.Api.Models.Users;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Network;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class ServerSendUserInfo : GameNetworkMessage
{
    public VirtualPlayer? Peer { get; set; }
    public CrpgUser User { get; set; } = default!;

    public ServerSendUserInfo()
    {
    }

    protected override void OnWrite()
    {
        WriteVirtualPlayerReferenceToPacket(Peer);
        WriteUserToPacket(User);
    }

    protected override bool OnRead()
    {
        bool valid = true;
        Peer = ReadVirtualPlayerReferenceToPacket(ref valid);
        User = ReadUserFromPacket(ref valid);
        return valid;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter() => MultiplayerMessageFilter.GameMode;
    protected override string OnGetLogFormat() => $"ServerSendUserInfo: {User.Name} ({User.Id})";

    private void WriteUserToPacket(CrpgUser user)
    {
        using MemoryStream stream = new();
        using (GZipStream gzip = new(stream, CompressionMode.Compress, leaveOpen: true))
        using (BinaryWriter writer = new(gzip))
        {
            // Platform info
            writer.Write((byte)user.Platform);
            writer.Write(user.PlatformUserId);
            writer.Write((byte)user.Region);

            // Basic user info
            writer.Write(user.Id);
            writer.Write(user.Name);
            writer.Write(user.Gold);

            // Character info
            writer.Write(user.Character?.Name ?? string.Empty);
            writer.Write(user.Character?.Generation ?? 0);
            writer.Write(user.Character?.Level ?? 0);
            writer.Write(user.Character?.Experience ?? 0f);

            // Clan info
            writer.Write(user.ClanMembership?.ClanId ?? -1);
            writer.Write(user.ClanMembership != null ? (byte)user.ClanMembership.Role : (byte)255);
        }

        WriteByteArrayToPacket(stream.ToArray(), 0, (int)stream.Length);
    }

    private CrpgUser ReadUserFromPacket(ref bool valid)
    {
        byte[] buffer = new byte[1024];
        int length = ReadByteArrayFromPacket(buffer, 0, buffer.Length, ref valid);

        using MemoryStream stream = new(buffer, 0, length);
        using GZipStream gzip = new(stream, CompressionMode.Decompress);
        using BinaryReader reader = new(gzip);

        var user = new CrpgUser
        {
            // Platform info
            Platform = (Platform)reader.ReadByte(),
            PlatformUserId = reader.ReadString(),
            Region = (CrpgRegion)reader.ReadByte(),

            // Basic info
            Id = reader.ReadInt32(),
            Name = reader.ReadString(),
            Gold = reader.ReadInt32(),

            // Character info
            Character = new CrpgCharacter
            {
                Name = reader.ReadString(),
                Generation = reader.ReadInt32(),
                Level = reader.ReadInt32(),
                Experience = reader.ReadInt32(),
            },
        };

        // Clan info
        int clanId = reader.ReadInt32();
        byte clanRole = reader.ReadByte();
        user.ClanMembership = clanId != -1 ? new CrpgClanMember { ClanId = clanId, Role = (CrpgClanMemberRole)clanRole } : null;

        return user;
    }
}
