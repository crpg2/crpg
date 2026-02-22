using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Commander;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
internal sealed class CommanderPollRequested : GameNetworkMessage
{
    public NetworkCommunicator PlayerPeer { get; set; } = null!;
    public bool IsDemoteRequested { get; set; }

    protected override bool OnRead()
    {
        bool result = true;
        PlayerPeer = ReadNetworkPeerReferenceFromPacket(ref result, true);
        IsDemoteRequested = ReadBoolFromPacket(ref result);
        return result;
    }

    protected override void OnWrite()
    {
        WriteNetworkPeerReferenceToPacket(PlayerPeer);
        WriteBoolToPacket(IsDemoteRequested);
    }

    protected override MultiplayerMessageFilter OnGetLogFilter()
    {
        return MultiplayerMessageFilter.Administration;
    }

    protected override string OnGetLogFormat()
    {
        string str = "Requested to start poll to";
        string str1 = IsDemoteRequested ? " demote" : " promote";
        string str2 = " player: ";
        NetworkCommunicator playerPeer = PlayerPeer;
        return str + str1 + str2 + playerPeer?.UserName;
    }
}
