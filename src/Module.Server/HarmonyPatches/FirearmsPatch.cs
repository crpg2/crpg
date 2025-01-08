using HarmonyLib;
using TaleWorlds.Core;

namespace Crpg.Module.HarmonyPatches;

[HarmonyPatch]
public class FirearmsPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(WeaponComponentData), "GetRelevantSkillFromWeaponClass")]
    public static bool AddFirearmRelevantSkill(ref SkillObject __result, WeaponClass weaponClass)
    {
        if (weaponClass == WeaponClass.Cartridge || weaponClass == WeaponClass.Musket || weaponClass == WeaponClass.Pistol)
        {
            __result = DefaultSkills.Crossbow;
            return false;
        }

        return true;
    }
}
