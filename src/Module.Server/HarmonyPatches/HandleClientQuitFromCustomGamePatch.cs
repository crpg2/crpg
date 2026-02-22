using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.DedicatedCustomServer;
using TaleWorlds.PlayerServices;

namespace Crpg.Module.HarmonyPatches;

[HarmonyPatch]
public class HandleClientQuitFromCustomGamePatch
{
#if CRPG_SERVER
    [HarmonyPrefix]
    [HarmonyPatch(typeof(DedicatedCustomGameServerHandler), "HandleClientQuitFromCustomGame")]
    public static bool Prefix(PlayerId playerId)
    {
        Task.Run(async () =>
        {
            while (Mission.Current != null && Mission.Current.CurrentState != Mission.State.Continuing)
            {
                await Task.Delay(1);
            }

            NetworkCommunicator? networkCommunicator = GameNetwork.NetworkPeers.FirstOrDefault(x => x.VirtualPlayer.Id == playerId);
            if (networkCommunicator != null && !networkCommunicator.IsServerPeer)
            {
                networkCommunicator.QuitFromMission = true;
                GameNetwork.AddNetworkPeerToDisconnectAsServer(networkCommunicator);
                MBDebug.Print("player with id " + playerId.ToString() + " quit from game");
            }
        });

        return false;
    }
#endif
}
