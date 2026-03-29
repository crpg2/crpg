using Crpg.Module.Common;
using TaleWorlds.MountAndBlade;
namespace Crpg.Module.Common.ChatCommands.User;

internal class FireWeaponCommand : ChatCommand
{
    public FireWeaponCommand(ChatCommandsComponent chatComponent)
    : base(chatComponent)
    {
        Name = "fw";
        Description = $"'{ChatCommandsComponent.CommandPrefix}{Name} PLAYERID or PLAYERNANE' to toggle FireWeapon.";
        Overloads = new CommandOverload[]
        {
            // No target — applies to self                                                                                   // /fs firesword
            new(Array.Empty<ChatCommandParameterType>(), ExecuteFireWeapon),
            // With player ID
            new(new[] { ChatCommandParameterType.PlayerId }, ExecuteFireWeapon),
            // With player Name
            new(new[] { ChatCommandParameterType.String }, ExecuteFireWeapon),
        };
    }

    private void ExecuteFireWeapon(NetworkCommunicator fromPeer, object[] arguments)
    {
        NetworkCommunicator targetPeer;

        if (arguments.Length == 0)
        {
            targetPeer = fromPeer;
        }
        else if (arguments[0] is NetworkCommunicator peer)
        {
            targetPeer = peer;
        }
        else if (arguments[0] is string name && TryGetPlayerByName(fromPeer, name, out var namedPeer))
        {
            targetPeer = namedPeer!;
        }
        else
        {
            return;
        }

        var fwBehavior = Mission.Current.GetMissionBehavior<FireWeaponsBehaviorServer>();
        if (fwBehavior == null)
        {
            ChatComponent.ServerSendMessageToPlayer(fromPeer, ColorFatal, "FireWeaponsBehaviorServer not found!");
            return;
        }

        if (!targetPeer.IsConnectionActive)
        {
            ChatComponent.ServerSendMessageToPlayer(fromPeer, ColorWarning, $"Invalid peer: {targetPeer.UserName}");
            return;
        }

        Agent? agent = targetPeer.ControlledAgent;
        if (agent == null)
        {
            ChatComponent.ServerSendMessageToPlayer(fromPeer, ColorWarning, $"{targetPeer.UserName} has no active agent.");
            return;
        }

        fwBehavior.ToggleFireWeapon(agent);
        bool enabled = fwBehavior.IsFireWeaponEnabled(targetPeer);
        ChatComponent.ServerSendMessageToPlayer(fromPeer, ColorSuccess, $"[FireWeapon] {targetPeer.UserName}: {(enabled ? "Enabled" : "Disabled")}");
    }
}
