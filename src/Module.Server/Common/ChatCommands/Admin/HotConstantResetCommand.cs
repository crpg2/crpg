using Crpg.Module.Common.HotConstants;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common.ChatCommands.Admin;

/// <summary>Command to reset one or all <see cref="HotConstant"/>s to their default values.</summary>
internal class HotConstantResetCommand : AdminCommand
{
    public HotConstantResetCommand(ChatCommandsComponent chatComponent)
        : base(chatComponent)
    {
        Name = "constr";
        Description = $"'{ChatCommandsComponent.CommandPrefix}{Name} [id]' to reset one or all constants to their default values.";
        Overloads = new CommandOverload[]
        {
            new([ChatCommandParameterType.Int32], ExecuteSingle),
            new([], ExecuteAll),
        };
    }

    private void ExecuteSingle(NetworkCommunicator fromPeer, object[] arguments)
    {
        int hotConstantId = (int)arguments[0];

        if (!HotConstant.TryGet(hotConstantId, out var constant))
        {
            ChatComponent.ServerSendMessageToPlayer(fromPeer, ColorWarning,
                $"No constant was found with id '{hotConstantId}'.");
            return;
        }

        float oldValue = constant!.Value;
        constant.Reset();

        GameNetwork.BeginBroadcastModuleEvent();
        GameNetwork.WriteMessage(new UpdateHotConstant
        {
            Id = hotConstantId,
            OldValue = oldValue,
            NewValue = constant.Value,
        });
        GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
    }

    private void ExecuteAll(NetworkCommunicator fromPeer, object[] arguments)
    {
        foreach (var constant in HotConstant.All)
        {
            float oldValue = constant.Value;
            constant.Reset();

            GameNetwork.BeginBroadcastModuleEvent();
            GameNetwork.WriteMessage(new UpdateHotConstant
            {
                Id = constant.Id,
                OldValue = oldValue,
                NewValue = constant.Value,
            });
            GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
        }
    }
}
