using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.HarmonyPatches;

[HarmonyPatch]
public class DecideWeaponCollisionReactionPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(MissionCombatMechanicsHelper), "DecideWeaponCollisionReaction")]
    public static void AddSlicedThrough(
        ref MeleeCollisionReaction colReaction,
        in Blow registeredBlow,
        in AttackCollisionData collisionData,
        Agent attacker,
        Agent defender,
        in MissionWeapon attackerWeapon,
        bool isFatalHit,
        bool isShruggedOff,
        float momentumRemaining)
    {
        // Bail early if already slicing through or the hit didn't connect to an agent body.
        if (colReaction == MeleeCollisionReaction.SlicedThrough)
        {
            return;
        }

        bool hitEnemy = collisionData.IsColliderAgent && !collisionData.AttackBlockedWithShield;
        if (!hitEnemy)
        {
            return;
        }

        // Original behaviour: all axes cleave regardless of target.
        var weaponClass = attackerWeapon.IsEmpty ? WeaponClass.Undefined : attackerWeapon.CurrentUsageItem.WeaponClass;
        bool isAxe = weaponClass == WeaponClass.OneHandedAxe || weaponClass == WeaponClass.TwoHandedAxe;

        // New behaviour: any melee weapon cleaves when an infantry attacker hits a mounted defender.
        // defender.MountAgent != null  → the hit target is a rider sitting on a horse.
        // attacker.MountAgent == null  → the attacker is on foot (infantry).
        bool attackerIsOnFoot = attacker?.MountAgent == null;
        bool defenderIsMounted = defender?.MountAgent != null;
        bool infantryHittingCav = attackerIsOnFoot && defenderIsMounted;

        if (isAxe || infantryHittingCav)
        {
            colReaction = MeleeCollisionReaction.SlicedThrough;
        }
    }
}
