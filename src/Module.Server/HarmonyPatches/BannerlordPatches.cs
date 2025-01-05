using System.Reflection;
using HarmonyLib;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Diamond;
#if CRPG_CLIENT
using TaleWorlds.MountAndBlade.View.Tableaus;
#endif

namespace Crpg.Module.HarmonyPatches;

/// <summary>
/// Because some bugs are fixed too slow by TaleWorlds, they are patched here using Harmony. It is the only acceptable
/// use of this library in this project. The patches should be removed as soon as TaleWorlds fixed the bugs.
/// </summary>
internal static class BannerlordPatches
{
    public static void Apply()
    {
        Harmony harmony = new("BannerlordServerPatches");
        harmony.PatchAll();
        /*
        AddPrefix(harmony, typeof(MissionLobbyComponent), "SendPeerInformationsToPeer",
            BindingFlags.NonPublic | BindingFlags.Instance, typeof(SendPeerInformationsToPeerPatch),
            nameof(SendPeerInformationsToPeerPatch.Prefix));
        AddPrefix(harmony, typeof(MissionNetworkComponent), "SendSpawnedMissionObjectsToPeer",
            BindingFlags.NonPublic | BindingFlags.Instance, typeof(MissionNetworkComponentPatch),
            nameof(MissionNetworkComponentPatch.Prefix));*/
        AddPrefix(harmony, typeof(ThumbnailCreatorView), "RegisterEntityWithoutTexture",
            BindingFlags.Public | BindingFlags.Instance, typeof(RegisterEntityWithoutTexturePatch),
            nameof(RegisterEntityWithoutTexturePatch.Prefix));
#if CRPG_CLIENT
        AddPrefix(harmony, typeof(TableauCacheManager), "CreateItemBaseEntity",
        BindingFlags.NonPublic| BindingFlags.Instance, typeof(CreateItemBaseEntityPatch),
        nameof(CreateItemBaseEntityPatch.Prefix));
        AddPrefix(harmony, typeof(TableauCacheManager), "FillEntityWithPose",
        BindingFlags.NonPublic | BindingFlags.Instance, typeof(FillEntityWithPosePatch),
        nameof(FillEntityWithPosePatch.Prefix));
#endif
    }

    private static void AddPrefix(Harmony harmony, Type classToPatch, string functionToPatchName, BindingFlags flags, Type patchClass, string functionPatchName)
    {
        var functionToPatch = classToPatch.GetMethod(functionToPatchName, flags);
        var newHarmonyPatch = patchClass.GetMethod(functionPatchName);
        harmony.Patch(functionToPatch, prefix: new HarmonyMethod(newHarmonyPatch));
    }
}
