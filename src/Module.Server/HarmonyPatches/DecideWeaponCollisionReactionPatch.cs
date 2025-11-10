using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.HarmonyPatches;

[HarmonyPatch]
public class DecideWeaponCollisionReactionPatch
{
    [HarmonyPostfix]

    [HarmonyPatch(typeof(MissionCombatMechanicsHelper), "DecideWeaponCollisionReaction")]
    public static void AddAxeSlicedThrough(
        ref MeleeCollisionReaction colReaction,
        in Blow registeredBlow,
        in AttackCollisionData collisionData,
        Agent attacker,
        Agent defender, in MissionWeapon attackerWeapon,
        bool isFatalHit,
        bool isShruggedOff,
        float momentumRemaining)
    {
        // Check if the weapon used for the attack is an axe
        var weaponClass = attackerWeapon.IsEmpty ? WeaponClass.Undefined : attackerWeapon.CurrentUsageItem.WeaponClass;
        bool isAxe = weaponClass == WeaponClass.OneHandedAxe || weaponClass == WeaponClass.TwoHandedAxe;
        // Check if the attack hit an enemy (not a shield or an obstacle)
        bool hitEnemy = collisionData.IsColliderAgent && !collisionData.AttackBlockedWithShield;
        // Modify the collision reaction if the conditions are met
        if (isAxe && hitEnemy && colReaction != MeleeCollisionReaction.SlicedThrough)
        {
            colReaction = MeleeCollisionReaction.SlicedThrough;
        }
    }
}
