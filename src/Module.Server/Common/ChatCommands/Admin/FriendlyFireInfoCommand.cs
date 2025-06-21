using Crpg.Module.Common.FriendlyFireReport;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common.ChatCommands.Admin;

internal class FriendlyFireInfoCommand : AdminCommand
{
    public FriendlyFireInfoCommand(ChatCommandsComponent chatComponent)
    : base(chatComponent)
    {
        Name = "ff";
        Description = $"'{ChatCommandsComponent.CommandPrefix}{Name} PLAYERID or PLAYERNANE' to get friendly fire information for player.";
        Overloads = new CommandOverload[]
        {
            new(new[] { ChatCommandParameterType.PlayerId }, ExecuteFriendlyFireInfoByNetworkPeer),
            new(new[] { ChatCommandParameterType.String }, ExecuteFriendlyFireInfoByName),
        };
    }

    private void ExecuteFriendlyFireInfoByName(NetworkCommunicator fromPeer, object[] arguments)
    {
        string targetName = (string)arguments[0];
        if (!TryGetPlayerByName(fromPeer, targetName, out var targetPeer))
        {
            return;
        }

        arguments = new object[] { targetPeer! };
        ExecuteFriendlyFireInfoByNetworkPeer(fromPeer, arguments);
    }

    private void ExecuteFriendlyFireInfoByNetworkPeer(NetworkCommunicator fromPeer, object[] arguments)
    {
        var targetPeer = (NetworkCommunicator)arguments[0];
        var behavior = Mission.Current.GetMissionBehavior<FriendlyFireReportServerBehavior>();

        if (behavior == null)
        {
            ChatComponent.ServerSendMessageToPlayer(fromPeer, ColorFatal, "FriendlyFireReportServerBehavior not found!");
            return;
        }

        if (!CrpgServerConfiguration.IsFriendlyFireReportEnabled)
        {
            ChatComponent.ServerSendMessageToPlayer(fromPeer, ColorWarning, "[FF] Reporting teamhits is disabled.");
        }

        if (targetPeer == null || !targetPeer.IsConnectionActive)
        {
            ChatComponent.ServerSendMessageToPlayer(fromPeer, ColorWarning, $"Invalid peer: {arguments}");
            return;
        }

        var (activeReported, decayedReported, notReported) = behavior.GetReportedTeamHitBreakdown(targetPeer);

        int active = activeReported;
        int decayed = decayedReported;
        int not = notReported;

        string strOut = $"[FF] Info for {targetPeer.UserName}: Active={active}, Decayed={decayed}, NotReported={not}, Combined={active + decayed + not}";
        ChatComponent.ServerSendMessageToPlayer(fromPeer, ColorSuccess, strOut);
    }
}
