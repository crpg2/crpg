using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Network.CharacterLoadout;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
internal sealed class UserRequestGetEquippedItems : GameNetworkMessage
{
    // No payload needed; client just requests their items

    protected override void OnWrite()
    {
    }

    protected override bool OnRead() => true;

    protected override MultiplayerMessageFilter OnGetLogFilter() => MultiplayerMessageFilter.GameMode;

    protected override string OnGetLogFormat() => "Request cRPG User Character Equipped Items";
}
