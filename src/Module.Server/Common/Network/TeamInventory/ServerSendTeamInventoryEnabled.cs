using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Network.TeamInventory;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class ServerSendTeamInventoryEnabled : GameNetworkMessage
{
    public bool IsEnabled { get; set; }

    protected override void OnWrite()
    {
        WriteBoolToPacket(IsEnabled);
    }

    protected override bool OnRead()
    {
        bool valid = true;
        IsEnabled = ReadBoolFromPacket(ref valid);
        return valid;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter()
    {
        return MultiplayerMessageFilter.GameMode;
    }

    protected override string OnGetLogFormat()
    {
        return $"Server Send cRPG Team Inventory {(IsEnabled ? "Enabled" : "Disabled")}";
    }
}
