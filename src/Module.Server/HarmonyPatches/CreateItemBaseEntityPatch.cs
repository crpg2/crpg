using System;
using System.Reflection;
using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.HarmonyPatches;
public class CreateItemBaseEntityPatch
{
    public static bool Prefix(ItemObject item, ref Scene scene, ref Camera camera)
    {
        scene = BannerlordTableauManager.TableauCharacterScenes[2];

        return true; // Continue with original method
    }
}
