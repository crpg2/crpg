using Crpg.Module.Common.KeyBinder;
using HarmonyLib;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade.Options;

namespace Crpg.Module.HarmonyPatches;

// This patch enables Custom Gamekey categories available for customization in options -> keybindinds

[HarmonyPatch]
public static class GameKeyPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(HotKeyManager), nameof(HotKeyManager.RegisterInitialContexts))]
    public static bool Prefix_RegisterInitialContexts(ref IEnumerable<GameKeyContext> contexts)
    {
        List<GameKeyContext> newContexts = contexts.ToList();
        foreach (GameKeyContext context in KeyBinder.KeyContexts.Values)
        {
            if (!newContexts.Contains(context))
            {
                newContexts.Add(context);
            }
        }

        contexts = newContexts;
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(OptionsProvider), nameof(OptionsProvider.GetGameKeyCategoriesList))]
    public static IEnumerable<string> Postfix_GetGameKeyCategoriesList(IEnumerable<string> __result)
    {
        return __result.Concat(KeyBinder.KeysCategories.Select(c => c.CategoryId).Distinct());
    }
}
