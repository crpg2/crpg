using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Network;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class CrpgServerSendAnimationsEnabled : GameNetworkMessage
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
        return MultiplayerMessageFilter.None;
    }

    protected override string OnGetLogFormat()
    {
        return $"Server Send cRPG Animations {(IsEnabled ? "Enabled" : "Disabled")}";
    }
}
