using Crpg.Module.Common;
using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.HarmonyPatches;

/*
// Harmony patch for the ChargeDamageCallback method in the Mission class
// Needs MissionInternalHelper.cs to reverse patch the GetAttackCollisionResults method and RegisterBlow method

  // Console commands/ServerConfigurations to control charge damage behavior

    crpg_charge_damage_disable_all False
    crpg_charge_damage_allow_enemies True
    crpg_charge_damage_allow_friendly True
    crpg_charge_damage_mirror_friendly_to_mount False
    crpg_charge_damage_mirror_friendly_to_agent False
    crpg_charge_damage_mirror_mount_multiplier 5
    crpg_charge_damage_mirror_agent_multiplier 1
    crpg_charge_damage_mirror_mount_damage_max 50
    crpg_charge_damage_mirror_mount_damage_min 0
    crpg_charge_damage_mirror_mount_damage_max_percentage 25
    crpg_charge_damage_min_velocity_for_friendly_damage 0.0

    crpg_charge_damage_settings // list all charge damage settings

    CrpgServerConfiguration.DisableAllChargeDamage = false // Disable all charge damage ***overrides other flags***
    CrpgServerConfiguration.AllowChargeEnemies = true // Allow charge damage to enemies
    CrpgServerConfiguration.AllowFriendlyChargeDamage = true // Allow charge damage to friends
    CrpgServerConfiguration.MirrorFriendlyChargeDamageMount = false // Mirror charge damage from friendlies to mount
    CrpgServerConfiguration.MirrorFriendlyChargeDamageAgent = false // Mirror charge damage from friendlies to rider
    CrpgServerConfiguration.MirrorMountDamageMultiplier = 5 // Multiplier for charge damage to mount
    CrpgServerConfiguration.MirrorAgentDamageMultiplier = 1 // Multiplier for charge damage to rider
    CrpgServerConfiguration.MirrorMountDamageMaximum = 100 // Maximum damage to the mount <int 0-1000>
    CrpgServerConfiguration.MirrorMountDamageMinimum = 0 // Minimum damage to the mount <int 0-1000>
    CrpgServerConfiguration.MirrorMountDamageMaximumPercentage = 25 // Maximum percentage of horse max health that can be damaged in blow <int 0-100>
    CrpgServerConfiguration.MinimumChargeVelocityForFriendlyDamage = 0.0 // Minimum speed for charge damage to affect teammates <float>

*/
#pragma warning disable IDE0018 // Inline variable declaration
[HarmonyPatch(typeof(Mission))]
public static class ChargeDamageCallbackPatch
{
  [HarmonyPatch("ChargeDamageCallback")]
  [HarmonyPrefix]
  private static bool Prefix(
      Mission __instance,
      ref AttackCollisionData collisionData,
      Blow blow,
      Agent attacker,
      Agent victim)
  {
    if (victim.CurrentMortalityState == Agent.MortalityState.Invulnerable)
    {
      return false;
    }

    // If the charge damage is disabled or for Circumstance, skip the rest of the method and original method
    if (attacker.RiderAgent != null &&
        (CrpgServerConfiguration.DisableAllChargeDamage || // no charge damage
        (!CrpgServerConfiguration.AllowChargeEnemies && attacker.IsEnemyOf(victim)) || // no charge damage to enemies
        (!CrpgServerConfiguration.AllowFriendlyChargeDamage && !attacker.IsEnemyOf(victim)))) // no charge damage to friends
    {
      return false;
    }

    WeaponComponentData shieldOnBack;
    CombatLogData combatLog;
#pragma warning restore IDE0018 // Inline variable declaration

    // Call the reverse patched method to get collision results
    __instance.GetAttackCollisionResults(
         attacker,
         victim,
         WeakGameEntity.Invalid, // GameEntity is not used in this context, so we can pass null
         1f,
         in MissionWeapon.Invalid,
         false,
         false,
         false,
         ref collisionData,
         out shieldOnBack,
         out combatLog);

    if (collisionData.CollidedWithShieldOnBack && shieldOnBack != null && victim != null && victim.IsMainAgent)
    {
      InformationManager.DisplayMessage(new InformationMessage(GameTexts.FindText("ui_hit_shield_on_back").ToString(), Color.ConvertStringToColor("#FFFFFFFF")));
    }

    if ((double)collisionData.InflictedDamage <= 0.0)
    {
      return false;
    }

    blow.BaseMagnitude = collisionData.BaseMagnitude;
    blow.MovementSpeedDamageModifier = collisionData.MovementSpeedDamageModifier;
    blow.InflictedDamage = collisionData.InflictedDamage;
    blow.SelfInflictedDamage = collisionData.SelfInflictedDamage;
    blow.AbsorbedByArmor = (float)collisionData.AbsorbedByArmor;
    blow.DamageCalculated = true;

    if (MissionGameModels.Current.AgentApplyDamageModel.DecideAgentKnockedBackByBlow(attacker, victim, in collisionData, null, in blow))
    {
      blow.BlowFlag |= BlowFlags.KnockBack;
    }
    else
    {
      blow.BlowFlag &= ~BlowFlags.KnockBack;
    }

    if (MissionGameModels.Current.AgentApplyDamageModel.DecideAgentKnockedDownByBlow(attacker, victim, in collisionData, null, in blow))
    {
      blow.BlowFlag |= BlowFlags.KnockDown;
    }

    // Handle friendly charge damage
    if (!attacker.IsEnemyOf(victim))
    {
      if (victim == null)
      {
        return false;
      }

      // Set Minimum speed for charge damage to affect teammates?
      if (collisionData.ChargeVelocity < CrpgServerConfiguration.MinimumChargeVelocityForFriendlyDamage)
      {
        return false;
      }

      if (CrpgServerConfiguration.MirrorFriendlyChargeDamageAgent)
      {
        if (attacker.RiderAgent != null && attacker.RiderAgent.IsActive())
        {
          Blow duplicateBlow = blow;

          duplicateBlow.InflictedDamage = 0; // No damage to the victim in duplicate blow, only here to mirror the damage to the attacker agent
          duplicateBlow.SelfInflictedDamage = collisionData.InflictedDamage * CrpgServerConfiguration.MirrorAgentDamageMultiplier; // Adjust the multiplier as needed

          // Apply mirrored/damage to attacker (rider) but not to the victim up here
          __instance.RegisterBlow(attacker.RiderAgent, victim!, WeakGameEntity.Invalid, duplicateBlow, ref collisionData, in MissionWeapon.Invalid, ref combatLog);
        }
      }

      if (CrpgServerConfiguration.MirrorFriendlyChargeDamageMount)
      {
        Blow duplicateBlow = blow;
        int mountDamage = (int)(collisionData.InflictedDamage * CrpgServerConfiguration.MirrorMountDamageMultiplier);
        float horseMaxHealth = attacker.HealthLimit;

        // Minimum damage to the mount can be set here
        if (mountDamage < CrpgServerConfiguration.MirrorMountDamageMinimum)
        {
          mountDamage = CrpgServerConfiguration.MirrorMountDamageMinimum;
        }

        // Maximum damage allowed to the mount can be set here
        if (mountDamage > CrpgServerConfiguration.MirrorMountDamageMaximum)
        {
          mountDamage = CrpgServerConfiguration.MirrorMountDamageMaximum;
        }

        // Maximum percentage of horse max health that can be damaged
        int maxPercent = CrpgServerConfiguration.MirrorMountDamageMaximumPercentage;
        float maxFraction = maxPercent / 100f;
        if (mountDamage > horseMaxHealth * maxFraction)
        {
          mountDamage = (int)(horseMaxHealth * maxFraction);
        }

        duplicateBlow.InflictedDamage = 0; // No damage to the victim in duplicate blow, only here to mirror the damage to the mount
        duplicateBlow.SelfInflictedDamage = mountDamage;

        // Apply mirrored damage to mount but not to the victim up here
        __instance.RegisterBlow(attacker, victim!, WeakGameEntity.Invalid, duplicateBlow, ref collisionData, in MissionWeapon.Invalid, ref combatLog);
      }

      // blow.InflictedDamage = 0f; // No damage to the victim in original blow to cancel victim being affected but i think we should still hurt them
    }

    __instance.RegisterBlow(attacker, victim!, WeakGameEntity.Invalid, blow, ref collisionData, in MissionWeapon.Invalid, ref combatLog);

    // Return false to skip the original method entirely
    return false;
  }
}

/* original methods

   internal void ChargeDamageCallback(
      ref AttackCollisionData collisionData,
      Blow blow,
      Agent attacker,
      Agent victim)
    {
      if (victim.CurrentMortalityState == Agent.MortalityState.Invulnerable || attacker.RiderAgent != null && !attacker.IsEnemyOf(victim))
        return;
      WeaponComponentData shieldOnBack;
      CombatLogData combatLog;
      this.GetAttackCollisionResults(attacker, victim, (GameEntity) null, 1f, in MissionWeapon.Invalid, false, false, false, ref collisionData, out shieldOnBack, out combatLog);
      if (collisionData.CollidedWithShieldOnBack && shieldOnBack != null && victim != null && victim.IsMainAgent)
        InformationManager.DisplayMessage(new InformationMessage(GameTexts.FindText("ui_hit_shield_on_back").ToString(), Color.ConvertStringToColor("#FFFFFFFF")));
      if ((double) collisionData.InflictedDamage <= 0.0)
        return;
      blow.BaseMagnitude = collisionData.BaseMagnitude;
      blow.MovementSpeedDamageModifier = collisionData.MovementSpeedDamageModifier;
      blow.InflictedDamage = collisionData.InflictedDamage;
      blow.SelfInflictedDamage = collisionData.SelfInflictedDamage;
      blow.AbsorbedByArmor = (float) collisionData.AbsorbedByArmor;
      blow.DamageCalculated = true;
      if (MissionGameModels.Current.AgentApplyDamageModel.DecideAgentKnockedBackByBlow(attacker, victim, in collisionData, (WeaponComponentData) null, in blow))
        blow.BlowFlag |= BlowFlags.KnockBack;
      else
        blow.BlowFlag &= ~BlowFlags.KnockBack;
      if (MissionGameModels.Current.AgentApplyDamageModel.DecideAgentKnockedDownByBlow(attacker, victim, in collisionData, (WeaponComponentData) null, in blow))
        blow.BlowFlag |= BlowFlags.KnockDown;
      this.RegisterBlow(attacker, victim, (GameEntity) null, blow, ref collisionData, new MissionWeapon(), ref combatLog);
    }
*/
