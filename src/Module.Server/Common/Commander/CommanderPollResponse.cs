using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Commander;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
internal sealed class CommanderPollResponse : GameNetworkMessage
{
    public bool Accepted { get; set; }

    protected override bool OnRead()
    {
        bool bufferReadValid = true;
        Accepted = ReadBoolFromPacket(ref bufferReadValid);
        return bufferReadValid;
    }

    protected override void OnWrite()
    {
        WriteBoolToPacket(Accepted);
    }

    protected override MultiplayerMessageFilter OnGetLogFilter()
    {
        return MultiplayerMessageFilter.Administration;
    }

    protected override string OnGetLogFormat()
    {
        return "Receiving poll response: " + (Accepted ? "Accepted." : "Not accepted.");
    }
}
