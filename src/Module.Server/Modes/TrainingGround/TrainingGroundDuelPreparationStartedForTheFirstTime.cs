using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Modes.TrainingGround;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class TrainingGroundDuelPreparationStartedForTheFirstTime : GameNetworkMessage
{
    public NetworkCommunicator RequesterPeer { get; set; } = null!;

    public NetworkCommunicator RequesteePeer { get; set; } = null!;

    protected override bool OnRead()
    {
        bool bufferReadValid = true;
        RequesterPeer = ReadNetworkPeerReferenceFromPacket(ref bufferReadValid);
        RequesteePeer = ReadNetworkPeerReferenceFromPacket(ref bufferReadValid);
        return bufferReadValid;
    }

    protected override void OnWrite()
    {
        WriteNetworkPeerReferenceToPacket(RequesterPeer);
        WriteNetworkPeerReferenceToPacket(RequesteePeer);
    }

    protected override MultiplayerMessageFilter OnGetLogFilter()
    {
        return MultiplayerMessageFilter.GameMode;
    }

    protected override string OnGetLogFormat()
    {
        return "Duel started between agent with name: " + RequesteePeer.UserName + " and index: " + RequesteePeer.Index + " and agent with name: " + RequesterPeer.UserName + " and index: " + RequesterPeer.Index;
    }
}
