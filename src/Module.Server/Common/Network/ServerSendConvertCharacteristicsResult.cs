using System.IO.Compression;
using Crpg.Module.Api.Models.Characters;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Network;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class ServerSendConvertCharacteristicsResult : GameNetworkMessage
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public int AttributesPoints { get; set; }
    public int SkillPoints { get; set; }

    protected override void OnWrite()
    {
        WriteBoolToPacket(Success);
        WriteStringToPacket(ErrorMessage);
        WriteIntToPacket(AttributesPoints, CompressionBasic.DebugIntNonCompressionInfo);
        WriteIntToPacket(SkillPoints, CompressionBasic.DebugIntNonCompressionInfo);
    }

    protected override bool OnRead()
    {
        bool ok = true;
        Success = ReadBoolFromPacket(ref ok);
        ErrorMessage = ReadStringFromPacket(ref ok);
        AttributesPoints = ReadIntFromPacket(CompressionBasic.DebugIntNonCompressionInfo, ref ok);
        SkillPoints = ReadIntFromPacket(CompressionBasic.DebugIntNonCompressionInfo, ref ok);

        return ok;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter() => MultiplayerMessageFilter.GameMode;
    protected override string OnGetLogFormat() =>
        $"Server sent api update characteristics result:";
}
