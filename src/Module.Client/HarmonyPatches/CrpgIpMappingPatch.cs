using HarmonyLib;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Client.HarmonyPatches;

/// <summary>
/// Harmony patches for IP address mapping in Bannerlord network connection methods.
/// </summary>
[HarmonyPatch]
public static class CrpgIpMappingPatch
{
    /// <summary>
    /// Target method: GameNetwork.Connect
    /// Alternative connection method that might be used.
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameNetwork), "InitializeClientSide")]
    public static bool Prefix_GameNetworkConnect(ref string serverAddress, ref int port)
    {
        return ApplyIpMapping(ref serverAddress, ref port);
    }

    /// <summary>
    /// Common method to apply IP mapping logic.
    /// </summary>
    private static bool ApplyIpMapping(ref string address, ref int port)
    {
        if (CrpgIpMapping.TryGetMapping(address, port, out string newAddress, out int newPort))
        {
            address = newAddress;
            port = newPort;
        }

        return true; // Continue with original method
    }


}
