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
        fromPeer.ControlledAgent.MakeVoice(SkinVoiceManager.VoiceType.Yell, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
        foreach (NetworkCommunicator targetPeer in GameNetwork.NetworkPeers)
        {
            if (targetPeer.GetComponent<MissionPeer>()?.Team.Side == missionPeer.Team.Side)
            {
                if (!targetPeer.IsServerPeer && targetPeer.IsSynchronized)
                {
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
    }
}
