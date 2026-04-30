using Crpg.Module.Notifications;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common.ChatCommands.Commander;
internal class OrderCommand : CommanderCommand
{
    public OrderCommand(ChatCommandsComponent chatComponent)
         : base(chatComponent)
    {
        Name = "o";
        Description = $"'{ChatCommandsComponent.CommandPrefix}{Name} message' to send an order to your troops.";
        Overloads = new CommandOverload[]
        {
            new(new[] { ChatCommandParameterType.String }, ExecuteAnnouncement),
        };
    }

    private void ExecuteAnnouncement(NetworkCommunicator fromPeer, object[] arguments)
    {
        string message = (string)arguments[0];
        MissionPeer? missionPeer = fromPeer.GetComponent<MissionPeer>();
        // MakeVoice disabled: this native P/Invoke into the engine's sound system caused ExecutionEngineException
        // crashes on Linux servers when called during network packet processing.
        // fromPeer.ControlledAgent.MakeVoice(SkinVoiceManager.VoiceType.Yell, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
        //
        // Call GetComponent<MissionPeer> only for peers that are already synchronized. Iterating all NetworkPeers
        // and calling GetComponent on loading / limbo peers has triggered fatal UnmanagedCallersOnly errors on
        // Linux dedicated servers (see logs: crash after "before GetComponent" for a specific peer).
        BattleSideEnum commanderSide = missionPeer.Team.Side;
        foreach (NetworkCommunicator targetPeer in GameNetwork.NetworkPeers)
        {
            if (targetPeer.IsServerPeer || !targetPeer.IsSynchronized)
            {
                continue;
            }

            MissionPeer? targetMp = targetPeer.GetComponent<MissionPeer>();
            if (targetMp?.Team == null || targetMp.Team.Side != commanderSide)
            {
                continue;
            }

            GameNetwork.BeginModuleEventAsServer(targetPeer);
            GameNetwork.WriteMessage(new CrpgNotification
            {
                Type = CrpgNotificationType.Commander,
                Message = message,
            });
            GameNetwork.EndModuleEventAsServer();
        }
    }
}
