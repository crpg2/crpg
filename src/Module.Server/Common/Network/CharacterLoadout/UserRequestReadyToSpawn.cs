using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Network.CharacterLoadout;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
internal sealed class UserRequestReadyToSpawn : GameNetworkMessage
{
    // No payload needed; client just requests readytospawn

    protected override void OnWrite()
    {
    }

    protected override bool OnRead() => true;

    protected override MultiplayerMessageFilter OnGetLogFilter() => MultiplayerMessageFilter.GameMode;

    protected override string OnGetLogFormat() => "characterLoadout user request readytospawn";
}
