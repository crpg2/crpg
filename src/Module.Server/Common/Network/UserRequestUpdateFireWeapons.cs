using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Network;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
internal sealed class UserRequestUpdateFireWeapons : GameNetworkMessage
{
    // No payload needed; client just requests updated FireWeapon Data

    protected override void OnWrite()
    {
    }

    protected override bool OnRead() => true;

    protected override MultiplayerMessageFilter OnGetLogFilter() => MultiplayerMessageFilter.GameMode;

    protected override string OnGetLogFormat() => "Request updated FireWeapon Data";
}
