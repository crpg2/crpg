using System.Runtime.CompilerServices;
using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.HarmonyPatches;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

[HarmonyPatch]
public static class MissionInternalHelper
{
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(Mission), "GetAttackCollisionResults")]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static CombatLogData GetAttackCollisionResults(
        this Mission _,
        Agent attackerAgent,
        Agent victimAgent,
        WeakGameEntity hitObject,
        float momentumRemaining,
        in MissionWeapon attackerWeapon,
        bool crushedThrough,
        bool cancelDamage,
        bool crushedThroughWithoutAgentCollision,
        ref AttackCollisionData attackCollisionData,
        out WeaponComponentData shieldOnBack,
        out CombatLogData combatLog)
    {
        throw new NotImplementedException("Reverse patch not applied");
    }

    [HarmonyReversePatch]
    [HarmonyPatch(typeof(Mission), "RegisterBlow")]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void RegisterBlow(
        this Mission _,
        Agent attacker,
        Agent victim,
        WeakGameEntity realHitEntity,
        Blow b,
        ref AttackCollisionData collisionData,
        in MissionWeapon attackerWeapon,
        ref CombatLogData combatLogData)
    {
        throw new NotImplementedException("Reverse patch not applied");
    }
}

#pragma warning restore SA1313 // Parameter names should begin with lower-case letter

/* original methods in TaleWorlds.MountAndBlade.Mission.cs

private void RegisterBlow(
    Agent attacker,
    Agent victim,
    GameEntity realHitEntity,
    Blow b,
    ref AttackCollisionData collisionData,
    in MissionWeapon attackerWeapon,
    ref CombatLogData combatLogData)
        {
            b.VictimBodyPart = collisionData.VictimHitBodyPart;
            if (!collisionData.AttackBlockedWithShield)
            {
                if (collisionData.IsColliderAgent)
                {
                    if (b.SelfInflictedDamage > 0 && attacker != null && attacker.IsActive() && attacker.IsFriendOf(victim))
                    {
                        Blow outBlow;
                        AttackCollisionData collisionData1;
                        attacker.CreateBlowFromBlowAsReflection(in b, in collisionData, out outBlow, out collisionData1);
                        if (victim.IsMount && attacker.MountAgent != null)
                            attacker.MountAgent.RegisterBlow(outBlow, in collisionData1);
                        else
                            attacker.RegisterBlow(outBlow, in collisionData1);
                    }
                    if (b.InflictedDamage > 0)
                    {
                        combatLogData.IsFatalDamage = victim != null && (double) victim.Health - (double) b.InflictedDamage < 1.0;
                        combatLogData.InflictedDamage = b.InflictedDamage - combatLogData.ModifiedDamage;
                        this.PrintAttackCollisionResults(attacker, victim, realHitEntity, ref collisionData, ref combatLogData);
                    }
                    victim.RegisterBlow(b, in collisionData);
                }
                else if (collisionData.EntityExists)
                {
                    MissionWeapon weapon = b.IsMissile ? this._missiles[b.WeaponRecord.AffectorWeaponSlotOrMissileIndex].Weapon : (attacker == null || !b.WeaponRecord.HasWeapon() ? MissionWeapon.Invalid : attacker.Equipment[b.WeaponRecord.AffectorWeaponSlotOrMissileIndex]);
                    this.OnEntityHit(realHitEntity, attacker, b.InflictedDamage, (DamageTypes) collisionData.DamageType, b.GlobalPosition, b.SwingDirection, in weapon);
                    if (attacker != null && b.SelfInflictedDamage > 0)
                    {
                        Blow outBlow;
                        AttackCollisionData collisionData2;
                        attacker.CreateBlowFromBlowAsReflection(in b, in collisionData, out outBlow, out collisionData2);
                        attacker.RegisterBlow(outBlow, in collisionData2);
                    }
                }
            }
            foreach (MissionBehavior missionBehavior in this.MissionBehaviors)
                missionBehavior.OnRegisterBlow(attacker, victim, realHitEntity, b, ref collisionData, in attackerWeapon);
        }

        private CombatLogData GetAttackCollisionResults(
                Agent attackerAgent,
                Agent victimAgent,
                GameEntity hitObject,
                float momentumRemaining,
                in MissionWeapon attackerWeapon,
            bool crushedThrough,
            bool cancelDamage,
            bool crushedThroughWithoutAgentCollision,
            ref AttackCollisionData attackCollisionData,
            out WeaponComponentData shieldOnBack,
            out CombatLogData combatLog)
        {
            AttackInformation attackInformation = new AttackInformation(attackerAgent, victimAgent, hitObject, in attackCollisionData, in attackerWeapon);
            shieldOnBack = attackInformation.ShieldOnBack;
            MissionCombatMechanicsHelper.GetAttackCollisionResults(in attackInformation, crushedThrough, momentumRemaining, in attackerWeapon, cancelDamage, ref attackCollisionData, out combatLog, out int _);
            float inflictedDamage = (float) attackCollisionData.InflictedDamage;
            if ((double) inflictedDamage > 0.0)
            {
                float damage = MissionGameModels.Current.AgentApplyDamageModel.CalculateDamage(in attackInformation, in attackCollisionData, in attackerWeapon, inflictedDamage);
                combatLog.ModifiedDamage = MathF.Round(damage - inflictedDamage);
                attackCollisionData.InflictedDamage = MathF.Round(damage);
            }
            else
            {
                combatLog.ModifiedDamage = 0;
                attackCollisionData.InflictedDamage = 0;
            }
            if (!attackCollisionData.IsFallDamage && attackInformation.IsFriendlyFire)
            {
                if (!attackInformation.IsAttackerAIControlled && GameNetwork.IsSessionActive)
                {
                    int num1 = attackCollisionData.IsMissile ? MultiplayerOptions.OptionType.FriendlyFireDamageRangedSelfPercent.GetIntValue() : MultiplayerOptions.OptionType.FriendlyFireDamageMeleeSelfPercent.GetIntValue();
                    attackCollisionData.SelfInflictedDamage = MathF.Round((float) attackCollisionData.InflictedDamage * ((float) num1 * 0.01f));
                    int num2 = attackCollisionData.IsMissile ? MultiplayerOptions.OptionType.FriendlyFireDamageRangedFriendPercent.GetIntValue() : MultiplayerOptions.OptionType.FriendlyFireDamageMeleeFriendPercent.GetIntValue();
                    attackCollisionData.InflictedDamage = MathF.Round((float) attackCollisionData.InflictedDamage * ((float) num2 * 0.01f));
                    combatLog.InflictedDamage = attackCollisionData.InflictedDamage;
                }
                combatLog.IsFriendlyFire = true;
            }
            if (attackCollisionData.AttackBlockedWithShield && attackCollisionData.InflictedDamage > 0 && (int) attackInformation.VictimShield.HitPoints - attackCollisionData.InflictedDamage <= 0)
                attackCollisionData.IsShieldBroken = true;
            if (!crushedThroughWithoutAgentCollision)
            {
                combatLog.BodyPartHit = attackCollisionData.VictimHitBodyPart;
                combatLog.IsVictimEntity = (NativeObject) hitObject != (NativeObject) null;
            }
            return combatLog;
        }
*/
