using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Network;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
internal sealed class CrpgRequestPlayAnimation : GameNetworkMessage
{
    public string ActionId { get; set; } = string.Empty;

    protected override void OnWrite()
    {
        WriteStringToPacket(ActionId);
    }

    protected override bool OnRead()
    {
        bool valid = true;
        ActionId = ReadStringFromPacket(ref valid);
        return valid;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter() => MultiplayerMessageFilter.GameMode;
    protected override string OnGetLogFormat() => $"Request play animation: {ActionId}";
}
