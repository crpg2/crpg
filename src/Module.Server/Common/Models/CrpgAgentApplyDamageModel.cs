using Crpg.Module.Helpers;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using static TaleWorlds.MountAndBlade.Mission;

namespace Crpg.Module.Common.Models;

/// <summary>
/// Used to adjust dmg calculations.
/// </summary>
internal class CrpgAgentApplyDamageModel : MultiplayerAgentApplyDamageModel
{
    private readonly CrpgConstants _constants;

    public CrpgAgentApplyDamageModel(CrpgConstants constants)
    {
        _constants = constants;
    }

    public override float CalculateDamage(
        in AttackInformation attackInformation,
        in AttackCollisionData collisionData,
        in MissionWeapon weapon,
        float baseDamage)
    {
        List<WeaponClass> meleeClass = new()
        {
            WeaponClass.Dagger,
            WeaponClass.Mace,
            WeaponClass.TwoHandedMace,
            WeaponClass.OneHandedSword,
            WeaponClass.TwoHandedSword,
            WeaponClass.OneHandedAxe,
            WeaponClass.TwoHandedAxe,
            WeaponClass.Pick,
            WeaponClass.LowGripPolearm,
            WeaponClass.OneHandedPolearm,
            WeaponClass.TwoHandedPolearm,
        };
        List<WeaponClass> swordClass = new()
        {
            WeaponClass.Dagger,
            WeaponClass.OneHandedSword,
            WeaponClass.TwoHandedSword,
        };
        List<WeaponClass> axeClass = new()
        {
            WeaponClass.OneHandedAxe,
            WeaponClass.TwoHandedAxe,
        };
        float finalDamage = base.CalculateDamage(attackInformation, collisionData, weapon, baseDamage);

        if (IsPlayerCharacterAttackingVipBot(attackInformation))
        {
            return 0f;
        }

        if (weapon.IsEmpty)
        {
            // Increase fist damage with strength and glove armor.
            int strengthSkill = GetSkillValue(attackInformation.AttackerAgentOrigin, CrpgSkills.Strength);
            int glovearmor = GetGloveArmor(attackInformation.AttackerAgentOrigin);
            if (collisionData.IsAlternativeAttack) // Kick
            {
                return finalDamage * 0.75f * (1 + 0.02f * strengthSkill);
            }

            return finalDamage * 0.75f * (1 + 0.02f * strengthSkill + 0.04f * glovearmor);
        }

        if (IsPlayerCharacterAttackingDtvBot(attackInformation))
        {
            switch (weapon.CurrentUsageItem.WeaponClass)
            {
                case WeaponClass.Bolt:
                case WeaponClass.Cartridge:
                    finalDamage *= 2.5f;
                    break;
                case WeaponClass.Arrow:
                    finalDamage *= 1.5f;
                    break;
                case WeaponClass.Javelin:
                case WeaponClass.ThrowingAxe:
                case WeaponClass.ThrowingKnife:
                case WeaponClass.Stone:
                    finalDamage *= 2.0f;
                    break;
            }
        }

        if (IsPlayerCharacterAttackingDtvBoss(attackInformation))
        {
            if (weapon.Item.StringId.Contains("ballista_projectile"))
            {
                finalDamage *= 0.05f;
            }
        }

        // CalculateShieldDamage only has dmg as parameter. Therefore it cannot be used to get any Skill values.
        if (collisionData.AttackBlockedWithShield && finalDamage > 0)
        {
            int shieldSkill = GetSkillValue(attackInformation.VictimAgentOrigin, CrpgSkills.Shield);
            finalDamage /= MathHelper.RecursivePolynomialFunctionOfDegree2(shieldSkill, _constants.DurabilityFactorForShieldRecursiveCoefs);
            if (meleeClass.Contains(weapon.CurrentUsageItem.WeaponClass))
            {
                // in bannerlord/Src/TaleWorlds.MountAndBlade/MissionCombatMechanicsHelper.cs/GetAttackCollisionResults()
                // ComputeBlowDamageOnShield is fed the basemagnitude from ComputeBlowMagnitude() instead of the specialmagnitude
                // specialmagnitude takes in account the damagefactor which is the bladesharpness.
                //  specialmagnitude is the damage you deal to agents , while basemagnitude is the blow from strikemagnitudemodel
                //  basemagnitude only takes in account both sweetspots and speedbonus , but not the damage multiplicator that each weapon have
                finalDamage *=
                    collisionData.StrikeType == (int)StrikeType.Thrust
                        ? weapon.CurrentUsageItem.ThrustDamageFactor
                        : weapon.CurrentUsageItem.SwingDamageFactor;

                if (weapon.CurrentUsageItem.WeaponFlags.HasAnyFlag(WeaponFlags.BonusAgainstShield))
                {
                    // this bonus is on top of the native x2 in MissionCombatMechanicsHelper
                    // so the final bonus is 4.0 for one- and two- handed axes and 3.0 for everything else. We do this instead of nerfing the impact of shield skill so shield can stay virtually unbreakable against sword.
                    // it is the same logic as arrows not dealing a lot of damage to horse but spears dealing extra damage to horses
                    // As we want archer to fear cavs and cavs to fear spears, we want swords to fear shielders and shielders to fear axes.

                    finalDamage *= axeClass.Contains(weapon.CurrentUsageItem.WeaponClass) ? 2.0f : 1.5f;
                }
            }
        }

        // We want to decrease survivability of horses against melee weapon and especially against spears and pikes.
        // By doing that we ensure that cavalry stays an archer predator while punishing cav errors like running into a wall or an obstacle
        if (!attackInformation.IsVictimAgentHuman
            && !attackInformation.DoesAttackerHaveMountAgent
            && !weapon.CurrentUsageItem.IsConsumable
            && weapon.CurrentUsageItem.IsMeleeWeapon
            && !weapon.IsAnyConsumable())
        {
            if (
                collisionData.StrikeType == (int)StrikeType.Thrust
                && collisionData.DamageType == (int)DamageTypes.Pierce
                && weapon.CurrentUsageItem.IsPolearm)
            {
                finalDamage *= 1.85f;
            }
            else
            {
                finalDamage *= 1.4f;
            }
        }

        // For bashes (with and without shield) - Not for allies cause teamdmg might reduce the "finalDamage" below zero. That will break teamhits with bashes.
        else if (collisionData.IsAlternativeAttack && !attackInformation.IsFriendlyFire)
        {
            finalDamage = 1f;
        }

        if (attackInformation.DoesAttackerHaveMountAgent && attackInformation.IsAttackerAgentDoingPassiveAttack)
        {
            finalDamage *= 0.23f; // Decrease damage from couched lance.
        }

        return finalDamage;
    }

    public override float GetDamageMultiplierForBodyPart(BoneBodyPartType bodyPart, DamageTypes type, bool isHuman, bool isMissile)
    {
        if (isMissile)
        {
            return isHuman ? CalculateRangedDamageMultiplierForHumanBodyPart(bodyPart, type) : CalculateRangedDamageMultiplierForNonHumanBodyPart(bodyPart, type);
        }
        else
        {
            return isHuman ? CalculateMeleeDamageMultiplierForHumanBodyPart(bodyPart, type) : CalculateMeleeDamageMultiplierForNonHumanBodyPart(bodyPart, type);
        }
    }

    public float CalculateRangedDamageMultiplierForHumanBodyPart(BoneBodyPartType bodyPart, DamageTypes type)
    {
        float result = 1f;
        switch (bodyPart)
        {
            case BoneBodyPartType.None:
                result = 1f;
                break;
            case BoneBodyPartType.Head:
            case BoneBodyPartType.Neck:
                switch (type)
                {
                    case DamageTypes.Invalid:
                        result = 2f;
                        break;
                    case DamageTypes.Cut:
                    case DamageTypes.Blunt:
                        result = 1.2f;
                        break;
                    case DamageTypes.Pierce:
                        result = 1.7f;
                        break;
                }

                break;
            case BoneBodyPartType.Chest:
            case BoneBodyPartType.Abdomen:
            case BoneBodyPartType.ShoulderLeft:
            case BoneBodyPartType.ShoulderRight:
                result = 0.9f;
                break;
            case BoneBodyPartType.ArmLeft:
            case BoneBodyPartType.ArmRight:
            case BoneBodyPartType.Legs:
                result = 0.75f;
                break;
        }

        return result;
    }

    public float CalculateRangedDamageMultiplierForNonHumanBodyPart(BoneBodyPartType bodyPart, DamageTypes type)
    {
        float result = 1f;
        switch (bodyPart)
        {
            case BoneBodyPartType.None:
                result = 1f;
                break;
            case BoneBodyPartType.Head:
            case BoneBodyPartType.Neck:
                switch (type)
                {
                    case DamageTypes.Invalid:
                        result = 2f;
                        break;
                    case DamageTypes.Cut:
                    case DamageTypes.Pierce:
                    case DamageTypes.Blunt:
                        result = 1.2f;
                        break;
                }

                break;
            case BoneBodyPartType.Chest:
            case BoneBodyPartType.Abdomen:
            case BoneBodyPartType.ShoulderLeft:
            case BoneBodyPartType.ShoulderRight:
            case BoneBodyPartType.ArmLeft:
            case BoneBodyPartType.ArmRight:
            case BoneBodyPartType.Legs:
                result = 0.8f;
                break;
        }

        return result;
    }

    public float CalculateMeleeDamageMultiplierForHumanBodyPart(BoneBodyPartType bodyPart, DamageTypes type)
    {
        float result = 1f;
        switch (bodyPart)
        {
            case BoneBodyPartType.None:
                result = 1f;
                break;
            case BoneBodyPartType.Head:
            case BoneBodyPartType.Neck:
                switch (type)
                {
                    case DamageTypes.Invalid:
                        result = 2f;
                        break;
                    case DamageTypes.Cut:
                    case DamageTypes.Blunt:
                        result = 1.2f;
                        break;
                    case DamageTypes.Pierce:
                        result = 1.3f;
                        break;
                }

                break;
            case BoneBodyPartType.Chest:
            case BoneBodyPartType.Abdomen:
            case BoneBodyPartType.ShoulderLeft:
            case BoneBodyPartType.ShoulderRight:
            case BoneBodyPartType.ArmLeft:
            case BoneBodyPartType.ArmRight:
                result = 1f;
                break;
            case BoneBodyPartType.Legs:
                result = 0.8f;
                break;
        }

        return result;
    }

    public float CalculateMeleeDamageMultiplierForNonHumanBodyPart(BoneBodyPartType bodyPart, DamageTypes type)
    {
        float result = 1f;
        switch (bodyPart)
        {
            case BoneBodyPartType.None:
                result = 1f;
                break;
            case BoneBodyPartType.Head:
            case BoneBodyPartType.Neck:
                switch (type)
                {
                    case DamageTypes.Invalid:
                        result = 2f;
                        break;
                    case DamageTypes.Cut:
                    case DamageTypes.Blunt:
                        result = 1.2f;
                        break;
                    case DamageTypes.Pierce:
                        result = 1.3f;
                        break;
                }

                break;
            case BoneBodyPartType.Chest:
            case BoneBodyPartType.Abdomen:
            case BoneBodyPartType.ShoulderLeft:
            case BoneBodyPartType.ShoulderRight:
            case BoneBodyPartType.ArmLeft:
            case BoneBodyPartType.ArmRight:
            case BoneBodyPartType.Legs:
                result = 0.8f;
                break;
        }

        return result;
    }

    public override void CalculateDefendedBlowStunMultipliers(
        Agent attackerAgent,
        Agent defenderAgent,
        CombatCollisionResult collisionResult,
        WeaponComponentData attackerWeapon,
        WeaponComponentData defenderWeapon,
        out float attackerStunMultiplier,
        out float defenderStunMultiplier)
    {
        attackerStunMultiplier = 1f;
        if (collisionResult == CombatCollisionResult.Blocked && defenderAgent.WieldedOffhandWeapon.IsShield())
        {
            int shieldSkill = 0;
            if (defenderAgent.Origin is CrpgBattleAgentOrigin crpgOrigin)
            {
                shieldSkill = crpgOrigin.Skills.GetPropertyValue(CrpgSkills.Shield);
            }

            defenderStunMultiplier = 1 / MathHelper.RecursivePolynomialFunctionOfDegree2(shieldSkill, _constants.ShieldDefendStunMultiplierForSkillRecursiveCoefs);

            return;
        }

        defenderStunMultiplier = 1f;
    }

    // TODO : Consider reworking once https://forums.taleworlds.com/index.php?threads/missioncombatmechanicshelper-getdefendcollisionresults-bypass-strikemagnitudecalculationmodel.459379 is fixed
    public override bool DecideCrushedThrough(
        Agent attackerAgent,
        Agent defenderAgent,
        float totalAttackEnergy,
        Agent.UsageDirection attackDirection,
        StrikeType strikeType,
        WeaponComponentData defendItem,
        bool isPassiveUsage)
    {
        EquipmentIndex wieldedItemIndex = attackerAgent.GetWieldedItemIndex(Agent.HandIndex.OffHand);
        if (wieldedItemIndex == EquipmentIndex.None)
        {
            wieldedItemIndex = attackerAgent.GetWieldedItemIndex(Agent.HandIndex.MainHand);
        }

        var weaponComponentData = wieldedItemIndex != EquipmentIndex.None
            ? attackerAgent.Equipment[wieldedItemIndex].CurrentUsageItem
            : null;
        if (weaponComponentData == null
            || isPassiveUsage
            || !weaponComponentData.WeaponFlags.HasAnyFlag(WeaponFlags.CanCrushThrough)
            || strikeType != StrikeType.Swing
            || attackDirection != Agent.UsageDirection.AttackUp)
        {
            return false;
        }

        float attackerPower = 3f * GetSkillValue(attackerAgent.Origin, CrpgSkills.PowerStrike);

        float defenderStrengthSkill = GetSkillValue(defenderAgent.Origin, CrpgSkills.Strength);
        float defenderShieldSkill = GetSkillValue(defenderAgent.Origin, CrpgSkills.Shield);
        float defenderDefendPower = defendItem != null && defendItem.IsShield
            ? Math.Max(defenderShieldSkill * 6 + 3, defenderStrengthSkill)
            : defenderStrengthSkill;
        int randomNumber = MBRandom.RandomInt(0, 1000);
        return randomNumber / 10f < Math.Pow(attackerPower / defenderDefendPower / 2.5f, 1.8f) * 100f;
    }

    private int GetSkillValue(IAgentOriginBase agentOrigin, SkillObject skill)
    {
        if (agentOrigin is CrpgBattleAgentOrigin crpgOrigin)
        {
            return crpgOrigin.Skills.GetPropertyValue(skill);
        }

        return 0;
    }

    private bool IsPlayerCharacterAttackingVipBot(AttackInformation attackInformation)
    {
        if (attackInformation.AttackerAgentOrigin is CrpgBattleAgentOrigin)
        {
            bool isVictimTheVipBot = attackInformation.VictimAgentCharacter != null
                ? attackInformation.VictimAgentCharacter.StringId.StartsWith("crpg_dtv_vip_")
                : false;

            return isVictimTheVipBot;
        }

        return false;
    }

    private bool IsPlayerCharacterAttackingDtvBot(AttackInformation attackInformation)
    {
        if (attackInformation.AttackerAgentOrigin is CrpgBattleAgentOrigin)
        {
            bool isVictimDtvBot = attackInformation.VictimAgentCharacter != null
                ? attackInformation.VictimAgentCharacter.StringId.StartsWith("crpg_dtv_")
                : false;

            return isVictimDtvBot;
        }

        return false;
    }

    private bool IsPlayerCharacterAttackingDtvBoss(AttackInformation attackInformation)
    {
        if (attackInformation.AttackerAgentOrigin is CrpgBattleAgentOrigin)
        {
            bool isVictimDtvBoss = attackInformation.VictimAgentCharacter != null
                ? attackInformation.VictimAgentCharacter.StringId.EndsWith("_boss")
                : false;

            return isVictimDtvBoss;
        }

        return false;
    }

    private int GetGloveArmor(IAgentOriginBase agentOrigin)
    {
        if (agentOrigin is CrpgBattleAgentOrigin crpgOrigin)
        {
            return crpgOrigin.ArmorItems.FirstOrDefault(a => a.type == ItemObject.ItemTypeEnum.HandArmor).armor?.ArmArmor ?? 0;
        }

        return 0;
    }
}
