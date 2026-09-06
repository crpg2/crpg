using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common.ChatCommands.Admin;

internal class FartCommand : AdminCommand
{
    public FartCommand(ChatCommandsComponent chatComponent)
        : base(chatComponent)
    {
        Name = "fart";
        Description = $"'{ChatCommandsComponent.CommandPrefix}{Name}' to make all agents Whistle.";
        Overloads = new CommandOverload[]
        {
            new(Array.Empty<ChatCommandParameterType>(), ExecuteFart),
        };
    }

    private void ExecuteFart(NetworkCommunicator fromPeer, object[] arguments)
    {
        ChatComponent.QueueAction(() =>
        {
            var voiceType = new SkinVoiceManager.SkinVoiceType("Whistle");
            Debug.Print("Making agents whistle...");
            foreach (Agent agent in Mission.Current.Agents)
            {
                if (agent.IsHuman && agent.IsActive())
                {
                    agent.MakeVoice(voiceType, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
                }
            }
        });
    }
}
