using System;
using System.Reflection;
using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.HarmonyPatches;
public class FillEntityWithPosePatch
{
    public static bool Prefix(CharacterCode characterCode, GameEntity poseEntity, ref Scene scene)
    {
        scene = BannerlordTableauManager.TableauCharacterScenes[2];

        return true; // Continue with original method
    }
}
