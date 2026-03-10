using System.Reflection;
using HarmonyLib;

namespace Crpg.Module.HarmonyPatches;

/// <summary>
/// Because some bugs are fixed too slow by TaleWorlds, they are patched here using Harmony. It is the only acceptable
/// use of this library in this project. The patches should be removed as soon as TaleWorlds fixed the bugs.
/// </summary>
internal static class BannerlordPatches
{
    public static void Apply()
    {
        Harmony harmony = new(typeof(BannerlordPatches).FullName);
        harmony.PatchAll(Assembly.GetExecutingAssembly());
    }
}
