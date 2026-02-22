using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Commander;
[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
public sealed class CommanderPollClosed : GameNetworkMessage
{
    public NetworkCommunicator PlayerPeer { get; set; } = null!;
    public bool Accepted { get; set; }

    protected override bool OnRead()
    {
        bool result = true;
        PlayerPeer = ReadNetworkPeerReferenceFromPacket(ref result);
        Accepted = ReadBoolFromPacket(ref result);
        return result;
    }

    protected override void OnWrite()
    {
        WriteNetworkPeerReferenceToPacket(PlayerPeer);
        WriteBoolToPacket(Accepted);
    }

    protected override MultiplayerMessageFilter OnGetLogFilter()
    {
        return MultiplayerMessageFilter.Administration;
    }

    protected override string OnGetLogFormat()
    {
        string?[] array = new string[5];
        array[0] = "Poll is closed. ";
        array[1] = PlayerPeer.UserName;
        array[2] = " is ";
        array[3] = Accepted ? string.Empty : "not ";
        array[4] = "promoted to commander.";
        return string.Concat(array);
    }
}
