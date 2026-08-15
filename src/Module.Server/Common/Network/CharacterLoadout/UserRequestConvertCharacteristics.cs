using Crpg.Module.Api.Models;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Network.CharacterLoadout;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
public sealed class UserRequestConvertCharacteristics : GameNetworkMessage
{
    public CrpgGameCharacteristicConversionRequest ConversionRequest { get; set; }

    // CrpgGameCharacteristicConversionRequest enum 0-1
    private readonly CompressionInfo.Integer _conversionCompression = new(0, 1, maximumValueGiven: true);

    protected override void OnWrite()
    {
        WriteIntToPacket((int)ConversionRequest, _conversionCompression);
    }

    protected override bool OnRead()
    {
        bool bufferValid = true;
        ConversionRequest = (CrpgGameCharacteristicConversionRequest)ReadIntFromPacket(_conversionCompression, ref bufferValid);
        return bufferValid;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter() => MultiplayerMessageFilter.GameMode;

    protected override string OnGetLogFormat() => $"Request Convert Characteristic: {ConversionRequest}";
}
