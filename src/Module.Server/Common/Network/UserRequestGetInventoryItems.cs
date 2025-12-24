using Crpg.Module.Api.Models.Items;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Network;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
internal sealed class UserRequestGetInventoryItems : GameNetworkMessage
{
    // No payload needed; client just requests their items

    protected override void OnWrite()
    {
    }

    protected override bool OnRead() => true;

    protected override MultiplayerMessageFilter OnGetLogFilter() => MultiplayerMessageFilter.GameMode;

    protected override string OnGetLogFormat() => "Request cRPG User Items";
}
