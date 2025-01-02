using System;
using System.Reflection;
using HarmonyLib;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.HarmonyPatches;
public class RegisterEntityWithoutTexturePatch
{
    public static bool Prefix(ref int width, ref int height, ref Scene scene, Camera camera, GameEntity entity, int allocationGroupIndex, string renderId = "", string debugName = "")
    {
        scene = BannerlordTableauManager.TableauCharacterScenes[2];
        // Modify width and height here
        width = 512; // New width value
        height = 240; // New height value
        
        return true; // Continue with original method
    }
}
