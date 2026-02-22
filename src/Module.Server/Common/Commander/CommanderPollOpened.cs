using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Commander;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class CommanderPollOpened : GameNetworkMessage
{
    public NetworkCommunicator InitiatorPeer { get; set; } = null!;
    public NetworkCommunicator PlayerPeer { get; set; } = null!;
    public bool IsDemoteRequested { get; set; }
    protected override bool OnRead()
    {
        bool result = true;
        InitiatorPeer = ReadNetworkPeerReferenceFromPacket(ref result);
        PlayerPeer = ReadNetworkPeerReferenceFromPacket(ref result);
        IsDemoteRequested = ReadBoolFromPacket(ref result);

        return result;
    }

    protected override void OnWrite()
    {
        WriteNetworkPeerReferenceToPacket(InitiatorPeer);
        WriteNetworkPeerReferenceToPacket(PlayerPeer);
        WriteBoolToPacket(IsDemoteRequested);
    }

    protected override MultiplayerMessageFilter OnGetLogFilter()
    {
        return MultiplayerMessageFilter.Administration;
    }

    protected override string OnGetLogFormat()
    {
        string?[] array = new string[4];
        array[0] = InitiatorPeer.UserName;
        array[1] = " wants to start poll to promote";
        array[2] = " player: ";
        array[3] = PlayerPeer.UserName;
        return string.Concat(array);
    }
}
