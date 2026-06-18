using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Network.TeamInventory;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
internal sealed class UserRequestReEquipTeamItems : GameNetworkMessage
{
    protected override void OnWrite()
    {
    }

    protected override bool OnRead() => true;
    protected override MultiplayerMessageFilter OnGetLogFilter() => MultiplayerMessageFilter.GameMode;
    protected override string OnGetLogFormat() => "User Request Re-Equip Team Items";
}
