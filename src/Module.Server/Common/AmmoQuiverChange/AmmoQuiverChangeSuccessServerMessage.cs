using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.AmmoQuiverChange;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class AmmoQuiverChangeSuccessServerMessage : GameNetworkMessage
{
    protected override bool OnRead()
    {
        // No data to read anymore
        return true;
    }

    protected override void OnWrite()
    {
        // No data to write anymore
    }

    protected override MultiplayerMessageFilter OnGetLogFilter()
    {
        return MultiplayerMessageFilter.General;
    }

    protected override string OnGetLogFormat()
    {
        return "AmmoQuiverChangeSuccessServerMessage - Quiver Ammo Changed Successfully";
    }
}
