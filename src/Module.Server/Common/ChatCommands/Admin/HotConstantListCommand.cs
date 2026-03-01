using Crpg.Module.Common.HotConstants;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common.ChatCommands.Admin;

/// <summary>Command to list <see cref="HotConstant"/>s.</summary>
internal class HotConstantListCommand : AdminCommand
{
    public HotConstantListCommand(ChatCommandsComponent chatComponent)
        : base(chatComponent)
    {
        Name = "constl";
        Description = $"'{ChatCommandsComponent.CommandPrefix}{Name} filter' to list constants.";
        Overloads = new CommandOverload[]
        {
            new([], Execute),
            new([ChatCommandParameterType.String], Execute),
        };
    }

    private void Execute(NetworkCommunicator fromPeer, object[] arguments)
    {
        string? filter = arguments.Length > 0 ? (string)arguments[0] : null;

        foreach (var cst in HotConstant.All)
        {
            if (filter != null && !cst.Description.Contains(filter, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string message = $"{cst.Id} ({cst.Description}): {cst.Value}";
            ChatComponent.ServerSendMessageToPlayer(fromPeer, ColorInfo, message);
        }
    }
}
