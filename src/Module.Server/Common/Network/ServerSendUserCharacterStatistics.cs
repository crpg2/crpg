using System.IO.Compression;
using Crpg.Module.Api.Models.Characters;
using Messages.FromClient.ToLobbyServer;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Network;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class ServerSendUserCharacterStatistics : GameNetworkMessage
{
    public CrpgCharacterStatistics CharacterStatistics { get; set; } = default!;

    protected override void OnWrite()
    {
        WriteIntToPacket(CharacterStatistics.Kills, new CompressionInfo.Integer(0, int.MaxValue, true));
        WriteIntToPacket(CharacterStatistics.Deaths, new CompressionInfo.Integer(0, int.MaxValue, true));
        WriteIntToPacket(CharacterStatistics.Assists, new CompressionInfo.Integer(0, int.MaxValue, true));
        WriteLongToPacket(CharacterStatistics.PlayTime.Ticks, new CompressionInfo.LongInteger(0, long.MaxValue, true));

        WriteFloatToPacket(CharacterStatistics.Rating.Value, new CompressionInfo.Float(-10000f, 10000f, 16));
        WriteFloatToPacket(CharacterStatistics.Rating.Deviation, new CompressionInfo.Float(0f, 10000f, 16));
        WriteFloatToPacket(CharacterStatistics.Rating.Volatility, new CompressionInfo.Float(0f, 100f, 16));
        WriteFloatToPacket(CharacterStatistics.Rating.CompetitiveValue, new CompressionInfo.Float(-10000f, 10000f, 16));
    }

    protected override bool OnRead()
    {
        bool bufferReadValid = true;

        int kills = ReadIntFromPacket(new CompressionInfo.Integer(0, int.MaxValue, true), ref bufferReadValid);
        int deaths = ReadIntFromPacket(new CompressionInfo.Integer(0, int.MaxValue, true), ref bufferReadValid);
        int assists = ReadIntFromPacket(new CompressionInfo.Integer(0, int.MaxValue, true), ref bufferReadValid);
        long ticks = ReadLongFromPacket(new CompressionInfo.LongInteger(0, long.MaxValue, true), ref bufferReadValid);

        float val = ReadFloatFromPacket(new CompressionInfo.Float(-10000f, 10000f, 16), ref bufferReadValid);
        float deviation = ReadFloatFromPacket(new CompressionInfo.Float(0f, 10000f, 16), ref bufferReadValid);
        float volatility = ReadFloatFromPacket(new CompressionInfo.Float(0f, 100f, 16), ref bufferReadValid);
        float competitiveValue = ReadFloatFromPacket(new CompressionInfo.Float(-10000f, 10000f, 16), ref bufferReadValid);

        CharacterStatistics = new CrpgCharacterStatistics
        {
            Kills = kills,
            Deaths = deaths,
            Assists = assists,
            PlayTime = TimeSpan.FromTicks(ticks),
            Rating = new CrpgCharacterRating
            {
                Value = val,
                Deviation = deviation,
                Volatility = volatility,
                CompetitiveValue = competitiveValue,
            },
        };

        return bufferReadValid;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter()
        => MultiplayerMessageFilter.GameMode;

    protected override string OnGetLogFormat()
        => $"Server sent character statistics";
}
