using Crpg.Module.Common;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common.ChatCommands.User;

internal class FireWeaponAllCommand : ChatCommand
{
    public FireWeaponAllCommand(ChatCommandsComponent chatComponent)
    : base(chatComponent)
    {
        Name = "fwall";
        Description = $"'{ChatCommandsComponent.CommandPrefix}{Name}' to toggle FireWeapon on all agents.";
        Overloads = new CommandOverload[]
        {
            new(Array.Empty<ChatCommandParameterType>(), ExecuteFireWeaponAll),
        };
    }

    private void ExecuteFireWeaponAll(NetworkCommunicator fromPeer, object[] arguments)
    {
        var fwBehavior = Mission.Current.GetMissionBehavior<FireWeaponsBehaviorServer>();
        if (fwBehavior == null)
        {
            ChatComponent.ServerSendMessageToPlayer(fromPeer, ColorFatal, "FireWeaponsBehaviorServer not found!");
            return;
        }

        fwBehavior.ToggleFireWeaponAll();
        ChatComponent.ServerSendMessageToPlayer(fromPeer, ColorSuccess, $"[FireWeaponAll]: {(fwBehavior.IsFireWeaponAllEnabled() ? "Enabled" : "Disabled")}");
    }
}
