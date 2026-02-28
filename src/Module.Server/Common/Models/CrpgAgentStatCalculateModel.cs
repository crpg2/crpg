using Crpg.Module.Helpers;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using MathF = TaleWorlds.Library.MathF;

namespace Crpg.Module.Common.Models;

/// <summary>
/// Mostly copied from <see cref="MultiplayerAgentStatCalculateModel"/> but with the class division system removed.
/// </summary>
internal class CrpgAgentStatCalculateModel : AgentStatCalculateModel
{
    private static readonly HashSet<WeaponClass> WeaponClassesAffectedByPowerStrike =
    [
        WeaponClass.Dagger,
        WeaponClass.OneHandedSword,
        WeaponClass.TwoHandedSword,
        WeaponClass.OneHandedAxe,
        WeaponClass.TwoHandedAxe,
        WeaponClass.Mace,
        WeaponClass.Pick,
        WeaponClass.TwoHandedMace,
        WeaponClass.OneHandedPolearm,
        WeaponClass.TwoHandedPolearm,
        WeaponClass.LowGripPolearm
    ];

    private static readonly HashSet<WeaponClass> WeaponClassesAffectedByPowerDraw =
    [
        WeaponClass.Arrow
    ];

    private static readonly HashSet<WeaponClass> WeaponClassesAffectedByPowerThrow =
    [
        WeaponClass.Stone,
        WeaponClass.Boulder,
        WeaponClass.ThrowingAxe,
        WeaponClass.ThrowingKnife,
        WeaponClass.Javelin
    ];

    private readonly CrpgConstants _constants;

    public CrpgAgentStatCalculateModel(CrpgConstants constants)
    {
        _constants = constants;
    }

    public override int GetEffectiveSkill(
        Agent agent,
        SkillObject skill)
    {
        if (agent.Origin is CrpgBattleAgentOrigin crpgOrigin)
        {
            return crpgOrigin.Skills.Skills.GetPropertyValue(skill);
        }

        return base.GetEffectiveSkill(agent, skill);
    }

    public override float GetWeaponInaccuracy(
        Agent agent,
        WeaponComponentData weapon,
        int weaponSkill)
    {
        float inaccuracy = 0.0f;
        float skillComponentMultiplier = 1f;
        float damageTypeFactorForThrustThrowing = weapon.ThrustDamageType switch
        {
            DamageTypes.Blunt => 1.3f,
            DamageTypes.Pierce => 1.2f,
            DamageTypes.Cut => 1f,
            _ => 1.3f,
        };

        float damageTypeFactorForSwingThrowing = weapon.SwingDamageType switch
        {
            DamageTypes.Blunt => 1.3f,
            DamageTypes.Pierce => 1.2f,
            DamageTypes.Cut => 1f,
            _ => 1.3f,
        };

        float weaponClassMultiplier = weapon.WeaponClass switch
        {
            WeaponClass.Bow => 1.25f,
            WeaponClass.Crossbow => 0.5f,
            WeaponClass.Musket => 0.5f,
            WeaponClass.Pistol => 0.5f,
            WeaponClass.Stone => (float)Math.Pow(weapon.ThrustDamage * damageTypeFactorForThrustThrowing / 30f, 2f) * 1.0f,
            WeaponClass.ThrowingAxe => (float)Math.Pow(weapon.ThrustDamage * damageTypeFactorForSwingThrowing / 30f, 2f) * 1.15f,
            WeaponClass.ThrowingKnife => (float)Math.Pow(weapon.ThrustDamage * damageTypeFactorForThrustThrowing / 30f, 2f) * 1.15f,
            WeaponClass.Javelin => (float)Math.Pow(weapon.ThrustDamage * damageTypeFactorForThrustThrowing / 30f, 2f) * 1.15f,
            _ => 1f,
        };

        if (weapon.IsRangedWeapon)
        {
            float weaponComponent = 0.1f / (float)Math.Pow(weapon.Accuracy / 100f, 5f);
            float skillComponent = skillComponentMultiplier * 1000000f / (1000000f + 0.01f * (float)Math.Pow(weaponSkill, 4));
            inaccuracy = weaponComponent * skillComponent;
            inaccuracy *= weaponClassMultiplier;
        }
        else if (weapon.WeaponFlags.HasAllFlags(WeaponFlags.WideGrip))
        {
            inaccuracy = 1.0f - weaponSkill * 0.01f;
        }

        return MathF.Max(inaccuracy, 0.0f);
    }

    public override void InitializeAgentStats(
       Agent agent,
       Equipment spawnEquipment,
       AgentDrivenProperties agentDrivenProperties,
       AgentBuildData agentBuildData)
    {
        agentDrivenProperties.ArmorEncumbrance = spawnEquipment.GetTotalWeightOfArmor(agent.IsHuman);
        if (agent.IsHuman)
        {
            InitializeHumanAgentStats(agent, spawnEquipment, agentDrivenProperties);
        }
        else
        {
            InitializeMountAgentStats(agent, spawnEquipment, agentDrivenProperties);
        }
    }

    public override void UpdateAgentStats(Agent agent, AgentDrivenProperties agentDrivenProperties)
    {
        if (agent.IsHuman)
        {
            UpdateHumanAgentStats(agent, agentDrivenProperties);

            // If rider is mounted, update the mount's stats too
            if (agent.HasMount && agent.MountAgent != null)
            {
                agent.MountAgent.UpdateAgentProperties();
            }
        }
        else if (agent.IsMount)
        {
            UpdateMountAgentStats(agent, agentDrivenProperties);
        }
    }

    /// <summary>AI difficulty.</summary>
    public override float GetDifficultyModifier()
    {
        return 1f;
    }

    public override float GetBreatheHoldMaxDuration(Agent agent, float baseBreatheHoldMaxDuration)
    {
        return baseBreatheHoldMaxDuration;
    }

    public override float GetEquipmentStealthBonus(Agent agent)
    {
        return 1f;
    }

    public override float GetSneakAttackMultiplier(Agent agent, WeaponComponentData weapon)
    {
        return 1f;
    }

    public override bool CanAgentRideMount(Agent agent, Agent targetMount)
    {
        // TODO: check riding skills?
        return true;
    }

    public override float GetWeaponDamageMultiplier(Agent agent, WeaponComponentData weaponComponent)
    {
        if (WeaponClassesAffectedByPowerStrike.Contains(weaponComponent.WeaponClass))
        {
            int powerStrike = GetEffectiveSkill(agent, CrpgSkills.PowerStrike);
            return 1 + powerStrike * _constants.DamageFactorForPowerStrike;
        }

        if (WeaponClassesAffectedByPowerDraw.Contains(weaponComponent.WeaponClass))
        {
            int powerDraw = GetEffectiveSkill(agent, CrpgSkills.PowerDraw);
            return 1 + powerDraw * _constants.DamageFactorForPowerDraw;
        }

        if (WeaponClassesAffectedByPowerThrow.Contains(weaponComponent.WeaponClass))
        {
            int powerThrow = GetEffectiveSkill(agent, CrpgSkills.PowerThrow);
            return 1 + powerThrow * _constants.DamageFactorForPowerThrow;
        }

        return 1;
    }

    public override float GetKnockBackResistance(Agent agent)
    {
        return 0.25f; // Same value as MultiplayerAgentStatCalculateModel.
    }

    public override float GetKnockDownResistance(Agent agent, StrikeType strikeType = StrikeType.Invalid)
    {
        float knockDownResistance = 0.5f;
        if (agent.HasMount)
        {
            knockDownResistance += 0.1f;
        }
        else if (strikeType == StrikeType.Thrust)
        {
            knockDownResistance += 0.25f;
        }

        return knockDownResistance;
    }

    public override float GetDismountResistance(Agent agent)
    {
        // https://www.desmos.com/calculator/97pwiguths
        int ridingSkills = GetEffectiveSkill(agent, DefaultSkills.Riding);
        return 0.0035f * ridingSkills;
    }

    private void InitializeHumanAgentStats(Agent agent, Equipment equipment, AgentDrivenProperties props)
    {
        props.SetStat(DrivenProperty.UseRealisticBlocking, MultiplayerOptions.OptionType.UseRealisticBlocking.GetBoolValue() ? 1f : 0.0f);
        props.ArmorHead = equipment.GetHeadArmorSum();
        props.ArmorTorso = equipment.GetHumanBodyArmorSum();
        props.ArmorLegs = equipment.GetLegArmorSum();
        props.ArmorArms = equipment.GetArmArmorSum();

        int strengthAttribute = GetEffectiveSkill(agent, CrpgSkills.Strength);
        int ironFleshSkill = GetEffectiveSkill(agent, CrpgSkills.IronFlesh);
        agent.BaseHealthLimit = _constants.DefaultHealthPoints
                                + strengthAttribute * _constants.HealthPointsForStrength
                                + ironFleshSkill * _constants.HealthPointsForIronFlesh;
        agent.HealthLimit = agent.BaseHealthLimit;
        agent.Health = agent.HealthLimit;
    }

    private void InitializeMountAgentStats(Agent agent, Equipment equipment, AgentDrivenProperties props)
    {
        EquipmentElement mount = equipment[EquipmentIndex.Horse];
        EquipmentElement mountHarness = equipment[EquipmentIndex.HorseHarness];

        props.AiSpeciesIndex = agent.Monster.FamilyType;
        props.AttributeRiding = 1f;
        props.ArmorTorso = mountHarness.Item != null ? mountHarness.GetModifiedMountBodyArmor() : 0;
        props.MountChargeDamage = mount.GetModifiedMountCharge(in mountHarness) * 0.01f;
        props.MountDifficulty = mount.Item.Difficulty;
    }

    private void UpdateMountAgentStats(Agent agent, AgentDrivenProperties props)
    {
        EquipmentElement mount = agent.SpawnEquipment[EquipmentIndex.ArmorItemEndSlot];
        EquipmentElement mountHarness = agent.SpawnEquipment[EquipmentIndex.HorseHarness];

        int ridingSkill = agent.RiderAgent != null
            ? GetEffectiveSkill(agent.RiderAgent, DefaultSkills.Riding)
            : 100;
        props.MountManeuver = mount.GetModifiedMountManeuver(in mountHarness) * (0.5f + ridingSkill * 0.0025f) * 1.15f;

        float harnessWeight = mountHarness.Item?.Weight ?? 0f;
        float riderPerceivedWeight = agent.RiderAgent != null
            ? ComputePerceivedWeight(agent.RiderAgent)
            : 0f;

        float totalEffectiveLoad = harnessWeight + riderPerceivedWeight;

        const float maxLoadReference = 50f;
        float loadPercentage = Math.Min(totalEffectiveLoad / maxLoadReference, 1f); // Cap at 1.0

        float weightImpactOnSpeed = 1f / (1f + 0.25f * loadPercentage);
        float ridingImpactOnSpeed = (float)(
            0.7f +
            ridingSkill * 0.001f +
            1 / (2.2f + Math.Pow(2, -0.08f * (ridingSkill - 70f))));

        props.MountSpeed = (mount.GetModifiedMountSpeed(in mountHarness) + 1) * 0.209f * ridingImpactOnSpeed * weightImpactOnSpeed;
        props.TopSpeedReachDuration = Game.Current.BasicModels.RidingModel.CalculateAcceleration(in mount, in mountHarness, ridingSkill);
        props.MountDashAccelerationMultiplier = 1f / (2f + 8f * loadPercentage); // native between 1 and 0.1 . cRPG between 0.5 and 0.1

        // Mounted penalty to mount stats based on weapon length and strength
        if (agent.RiderAgent is { } rider)
        {
            int strengthSkill = GetEffectiveSkill(rider, CrpgSkills.Strength);
            EquipmentIndex weaponIndex = rider.GetPrimaryWieldedItemIndex();
            WeaponComponentData? riderWeapon = weaponIndex != EquipmentIndex.None
                ? rider.Equipment[weaponIndex].CurrentUsageItem
                : null;

            if (riderWeapon != null)
            {
                float maxLength = MaxWeaponLengthForStrLevel(strengthSkill);
                float ratio = Math.Min(maxLength / riderWeapon.WeaponLength, 1f);
                float weaponLengthPenalty = 0.8f + 0.2f * ratio;

                props.MountManeuver *= weaponLengthPenalty;
                props.MountSpeed *= weaponLengthPenalty;
                props.MountDashAccelerationMultiplier *= weaponLengthPenalty;
                props.MountChargeDamage *= weaponLengthPenalty;
            }
        }
    }

    // WARNING : for some reason UpdateHumanAgentStats is called twice everytime there is a change (respawn or weapon switch)
    // The first call will have crpgUser be null, and all resulting cRPG stats be null it is then overriden by the
    // second call that will have crpgUser properly set if for some reason a calculation relies on str or agi being
    // superior to 3, the first call will have them set to 0 which can rely on dividing by zero if you're dividing by
    // (str -3).
    private void UpdateHumanAgentStats(Agent agent, AgentDrivenProperties props)
    {
        // Dirty hack, part of the work-around to have skills without spawning custom characters. This hack should be
        // performed in InitializeHumanAgentStats but the MissionPeer is null there.
        if (GameNetwork.IsClientOrReplay) // Server-side the hacky AgentOrigin is directly passed to the AgentBuildData.
        {
            var crpgUser = agent.MissionPeer?.GetComponent<CrpgPeer>()?.User;
            if (crpgUser != null && agent.Origin is not CrpgBattleAgentOrigin)
            {
                var characteristics = crpgUser.Character.Characteristics;
                var mbSkills = CrpgCharacterBuilder.CreateCharacterSkills(characteristics);
                agent.Origin = new CrpgBattleAgentOrigin(agent.Origin?.Troop, mbSkills);
                InitializeAgentStats(agent, agent.SpawnEquipment, props, null!);
            }
        }

        var classDivision = MultiplayerClassDivisions.GetMPHeroClassForCharacter(agent.Character);

        MissionEquipment equipment = agent.Equipment;
        props.WeaponsEncumbrance = equipment.GetTotalWeightOfWeapons();
        EquipmentIndex wieldedItemIndex3 = agent.GetPrimaryWieldedItemIndex();
        WeaponComponentData? equippedItem = wieldedItemIndex3 != EquipmentIndex.None
            ? equipment[wieldedItemIndex3].CurrentUsageItem
            : null;
        ItemObject? primaryItem = wieldedItemIndex3 != EquipmentIndex.None
            ? equipment[wieldedItemIndex3].Item
            : null;
        EquipmentIndex wieldedItemIndex4 = agent.GetOffhandWieldedItemIndex();
        WeaponComponentData? secondaryItem = wieldedItemIndex4 != EquipmentIndex.None
            ? equipment[wieldedItemIndex4].CurrentUsageItem
            : null;

        int strengthSkill = Math.Max(GetEffectiveSkill(agent, CrpgSkills.Strength), 3);
        int athleticsSkill = GetEffectiveSkill(agent, DefaultSkills.Athletics);
        float totalEncumbrance = props.ArmorEncumbrance + props.WeaponsEncumbrance;
        float perceivedWeight = ComputePerceivedWeight(agent);
        props.TopSpeedReachDuration = 1.1f * (1f + perceivedWeight / 15f) * (20f / (20f + (float)Math.Pow(athleticsSkill / 120f, 2f))) + ImpactOfStrAndWeaponLengthOnTimeToMaxSpeed(equippedItem != null ? equippedItem.WeaponLength : 22, strengthSkill);
        float speed = 0.58f + 0.034f * athleticsSkill / 26f;
        // HeroMovementSpeedMultiplier should be the same as TroopMovementSpeedMultiplier as long as we don't set troop_movement_speed in the XML.
        props.MaxSpeedMultiplier = classDivision.HeroMovementSpeedMultiplier * MBMath.ClampFloat(
            speed * (float)Math.Pow(361f / (361f + (float)Math.Pow(perceivedWeight, 5f)), 0.055f),
            0.1f,
            1.5f);
        float bipedalCombatSpeedMinMultiplier = ManagedParameters.Instance.GetManagedParameter(ManagedParametersEnum.BipedalCombatSpeedMinMultiplier);
        float bipedalCombatSpeedMaxMultiplier = ManagedParameters.Instance.GetManagedParameter(ManagedParametersEnum.BipedalCombatSpeedMaxMultiplier);

        int itemSkill = GetEffectiveSkill(agent, equippedItem?.RelevantSkill ?? DefaultSkills.Athletics);
        // Use weapon master here instead of wpf so the archer with no melee wpf can still fight.
        int weaponMaster = GetEffectiveSkill(agent, CrpgSkills.WeaponMaster);
        props.SwingSpeedMultiplier = 0.85f + 0.00237f * (float)Math.Pow(itemSkill, 0.9f);
        props.ThrustOrRangedReadySpeedMultiplier = props.SwingSpeedMultiplier;
        props.HandlingMultiplier = 1.05f * _constants.HandlingFactorForWeaponMaster[Math.Min(weaponMaster, _constants.HandlingFactorForWeaponMaster.Length - 1)];
        props.ShieldBashStunDurationMultiplier = 1f;
        props.KickStunDurationMultiplier = 1f;
        props.ReloadSpeed = equippedItem == null ? props.SwingSpeedMultiplier : equippedItem.SwingSpeed / 100f * (0.6f + 0.0001f * itemSkill + 0.0000125f * itemSkill * itemSkill);
        props.MissileSpeedMultiplier = 1f;
        props.ReloadMovementPenaltyFactor = 1f;
        SetAllWeaponInaccuracy(agent, props, (int)wieldedItemIndex3, equippedItem);
        int ridingSkill = GetEffectiveSkill(agent, DefaultSkills.Riding);
        props.BipedalRangedReadySpeedMultiplier = ManagedParameters.Instance.GetManagedParameter(ManagedParametersEnum.BipedalRangedReadySpeedMultiplier);
        props.BipedalRangedReloadSpeedMultiplier = ManagedParameters.Instance.GetManagedParameter(ManagedParametersEnum.BipedalRangedReloadSpeedMultiplier);
        props.CombatMaxSpeedMultiplier = bipedalCombatSpeedMaxMultiplier;
        props.CrouchedSpeedMultiplier = 1f;

        if (equippedItem != null)
        {
            int weaponSkill = GetEffectiveSkillForWeapon(agent, equippedItem);
            props.WeaponInaccuracy = GetWeaponInaccuracy(agent, equippedItem, weaponSkill);
            if (agent.HasMount && !equippedItem.IsRangedWeapon)
            {
                // SwingSpeed Nerf on Horseback
                static double Polynomial(float x)
                {
                    double y = x / 100f;
                    return 0.1f * y + 0.01f * Math.Pow(y, 10);
                }

                double swingTimeFactor = 1 - Polynomial(125) + Polynomial(equippedItem.WeaponLength);

                float cappedSwingSpeedFactor = MBMath.ClampFloat((float)(1 / swingTimeFactor), 0.25f, 0.91f);
                props.SwingSpeedMultiplier *= HasSwingDamage(primaryItem) ? cappedSwingSpeedFactor : 1f;
                // Thrustspeed Nerf on Horseback
                props.ThrustOrRangedReadySpeedMultiplier *= 0.84f;
            }

            // Ranged Behavior
            if (equippedItem.IsRangedWeapon)
            {
                props.TopSpeedReachDuration += 1.5f;
                props.ThrustOrRangedReadySpeedMultiplier = equippedItem.ThrustSpeed / 160f + 0.0015f * itemSkill;
                float maxMovementAccuracyPenaltyMultiplier = Math.Max(0.0f, 1.0f - weaponSkill / 500.0f);
                float weaponMaxMovementAccuracyPenalty = 0.125f * maxMovementAccuracyPenaltyMultiplier;
                float weaponMaxUnsteadyAccuracyPenalty = 0.1f * maxMovementAccuracyPenaltyMultiplier;
                props.WeaponMaxMovementAccuracyPenalty = Math.Max(0.0f, weaponMaxMovementAccuracyPenalty);
                props.WeaponMaxUnsteadyAccuracyPenalty = Math.Max(0.0f, weaponMaxUnsteadyAccuracyPenalty);

                // Crossbows
                if (equippedItem.RelevantSkill == DefaultSkills.Crossbow)
                {
                    props.WeaponInaccuracy /= ImpactOfStrReqOnCrossbows(agent, 1f, primaryItem);
                    props.WeaponMaxMovementAccuracyPenalty *= ImpactOfStrReqOnCrossbows(agent, 0.2f, primaryItem);
                    props.WeaponMaxUnsteadyAccuracyPenalty = 0.5f / ImpactOfStrReqOnCrossbows(agent, 0.05f, primaryItem); // override to remove impact of wpf on this property
                    props.WeaponRotationalAccuracyPenaltyInRadians /= ImpactOfStrReqOnCrossbows(agent, 0.3f, primaryItem);
                    props.ThrustOrRangedReadySpeedMultiplier *= 0.4f * (float)Math.Pow(2, weaponSkill / 191f) * ImpactOfStrReqOnCrossbows(agent, 0.3f, primaryItem); // Multiplying make windup time slower a 0 wpf, faster at 80 wpf
                    props.ReloadSpeed *= ImpactOfStrReqOnCrossbows(agent, 0.15f, primaryItem);
                }

                // Bows
                if (equippedItem.WeaponClass == WeaponClass.Bow)
                {
                    // Movement Penalty
                    float scale = MBMath.ClampFloat((equippedItem.ThrustSpeed - 60.0f) / 75.0f, 0.0f, 1f);
                    props.WeaponMaxMovementAccuracyPenalty *= 6f;
                    props.WeaponMaxUnsteadyAccuracyPenalty *= 4.5f / MBMath.Lerp(0.75f, 2f, scale);

                    // Aim Speed
                    props.WeaponBestAccuracyWaitTime = 0.3f + (95.75f - equippedItem.ThrustSpeed) * 0.005f;
                    float amount = MBMath.ClampFloat((equippedItem.ThrustSpeed - 60.0f) / 75.0f, 0.0f, 1f);

                    // Hold Time
                    int powerDraw = GetEffectiveSkill(agent, CrpgSkills.PowerDraw);
                    props.WeaponUnsteadyBeginTime = 0.06f + weaponSkill * 0.00175f * MBMath.Lerp(1f, 2f, amount) + powerDraw * powerDraw / 10f * 0.35f;
                    props.WeaponUnsteadyEndTime = 2f + props.WeaponUnsteadyBeginTime;

                    // Rotation Penalty
                    props.WeaponRotationalAccuracyPenaltyInRadians = 0.1f * (150f / (150f + itemSkill));
                    props.BipedalRangedReadySpeedMultiplier = 0.5f;
                    props.BipedalRangedReloadSpeedMultiplier = 0.65F;
                }

                // Throwing
                else if (equippedItem.WeaponClass is WeaponClass.Javelin or WeaponClass.ThrowingAxe or WeaponClass.ThrowingKnife or WeaponClass.Stone)
                {
                    int powerThrow = GetEffectiveSkill(agent, CrpgSkills.PowerThrow);

                    float wpfImpactOnWindUp = 140f; // lower is better 160f
                    float wpfImpactOnReloadSpeed = 240f; // lower is better 200f

                    float damageImpactOnWindUp = equippedItem.ThrustDamage
                                                 * CrpgItemValueModel.CalculateDamageTypeFactorForThrown(equippedItem.ThrustDamageType)
                                                 / CrpgItemValueModel.CalculateDamageTypeFactorForThrown(DamageTypes.Cut);

                    props.WeaponMaxUnsteadyAccuracyPenalty = 0.0035f;
                    props.WeaponMaxMovementAccuracyPenalty = 0.1f;

                    props.WeaponRotationalAccuracyPenaltyInRadians = 0.1f; // this is accuracy loss when turning lower is better

                    props.WeaponBestAccuracyWaitTime = 0.00001f; // set to extremely low because as soon as windup is finished thrower is accurate

                    props.ThrustOrRangedReadySpeedMultiplier = MBMath.Lerp(0.3f, 0.75f, (float)Math.Pow(itemSkill / wpfImpactOnWindUp, 2.5f) * 40f / damageImpactOnWindUp); // WindupSpeed
                    props.ReloadSpeed *= MBMath.Lerp(0.8f, 1.0f, itemSkill / wpfImpactOnReloadSpeed); // this only affects picking a new throwing weapon

                    props.CombatMaxSpeedMultiplier *= 0.75f; // This is a slowdown when ready to throw. Higher is better, do not go above 1.0

                    // These do not matter if props.WeaponMaxUnsteadyAccuracyPenalty is set to 0f
                    props.WeaponUnsteadyBeginTime = 1.0f + weaponSkill * 0.006f + powerThrow * powerThrow / 10f * 1.6f; // Time at which your character becomes tired and the accuracy declines
                    props.WeaponUnsteadyEndTime = 10f + props.WeaponUnsteadyBeginTime; // time at which your character is completely tired.
                }

                // Rest? Will not touch. It may affect other mechanics like Catapults etc...
                else
                {
                    props.WeaponBestAccuracyWaitTime = 0.1f;
                    props.WeaponUnsteadyBeginTime = 0.0f;
                    props.WeaponUnsteadyEndTime = 0.0f;
                    props.WeaponRotationalAccuracyPenaltyInRadians = 0.1f;
                }
            }
            else
            {
                if (!(equippedItem.WeaponClass is WeaponClass.Banner or WeaponClass.Boulder or WeaponClass.Undefined))
                {
                    float adjustedWeaponLength = equippedItem.WeaponClass == WeaponClass.TwoHandedPolearm
                        ? Math.Max(0, equippedItem.WeaponLength - 30f)
                        : equippedItem.WeaponLength;

                    props.CombatMaxSpeedMultiplier =
                        MathF.Min(
                        MBMath.Lerp(
                        bipedalCombatSpeedMaxMultiplier,
                        bipedalCombatSpeedMinMultiplier,
                        MathF.Clamp((adjustedWeaponLength - 80) / 120f, 0, 1f)),
                        1f);
                }

                // does this govern couching?
                if (equippedItem.WeaponFlags.HasAllFlags(WeaponFlags.WideGrip))
                {
                    props.WeaponUnsteadyBeginTime = 1.0f + weaponSkill * 0.005f;
                    props.WeaponUnsteadyEndTime = 3.0f + weaponSkill * 0.01f;
                }

                if (equippedItem.WeaponClass is WeaponClass.TwoHandedPolearm)
                {
                    props.HandlingMultiplier *= 1.1f;
                }

                props.CombatMaxSpeedMultiplier *= ImpactOfStrAndWeaponLengthOnCombatMaxSpeedMultiplier(equippedItem.WeaponLength, strengthSkill);

                // Thrust speed nerf for OneHandedPolearms
                if (equippedItem.WeaponClass == WeaponClass.OneHandedPolearm)
                {
                    props.ThrustOrRangedReadySpeedMultiplier *= 0.9f;
                }
            }

            // Mounted Archery

            if (agent.HasMount && equippedItem.IsRangedWeapon)
            {
                int mountedArcherySkill = GetEffectiveSkill(agent, CrpgSkills.MountedArchery);

                float weaponMaxMovementAccuracyPenalty = 0.03f / _constants.MountedRangedSkillInaccuracy[mountedArcherySkill];
                float weaponMaxUnsteadyAccuracyPenalty = 0.15f / _constants.MountedRangedSkillInaccuracy[mountedArcherySkill];

                if (equippedItem.RelevantSkill == DefaultSkills.Crossbow)
                {
                    float crossbowPenaltyFactor = ImpactOfStrReqOnCrossbows(agent, 0.2f, primaryItem);
                    weaponMaxUnsteadyAccuracyPenalty /= crossbowPenaltyFactor;
                    weaponMaxMovementAccuracyPenalty /= crossbowPenaltyFactor;
                }

                props.WeaponMaxMovementAccuracyPenalty = MathF.Clamp(weaponMaxMovementAccuracyPenalty, 0f, 1f);
                props.WeaponMaxUnsteadyAccuracyPenalty = MathF.Clamp(weaponMaxUnsteadyAccuracyPenalty, 0f, 1f);

                // Mounted skill penalty
                props.WeaponInaccuracy /= _constants.MountedRangedSkillInaccuracy[mountedArcherySkill];

                // Encumbrance-based inaccuracy penalty (neutral at 20, quadratic growth)
                float encumbranceRatio = totalEncumbrance / 20.0f;
                float encumbranceMultiplier = MathF.Max(
                    1.0f + (MathF.Pow(encumbranceRatio, 2f) - 1.0f) * 0.75f,
                    1.0f);

                props.WeaponInaccuracy *= encumbranceMultiplier;

                // Reload & draw speed penalty: linearly drops from 1.0 at 20 to 0.25 at 30
                float reloadThrustMultiplier = totalEncumbrance <= 20f
                    ? 1.0f
                    : MathF.Max(1.0f - (totalEncumbrance - 20f) / 10f * 0.75f, 0.25f);

                props.ReloadSpeed *= reloadThrustMultiplier;
                props.ThrustOrRangedReadySpeedMultiplier *= reloadThrustMultiplier;

                // Clamp values to ensure gameplay safety
                props.ReloadSpeed = MathF.Clamp(props.ReloadSpeed, 0.25f, 1.0f);
                props.ThrustOrRangedReadySpeedMultiplier = MathF.Clamp(props.ThrustOrRangedReadySpeedMultiplier, 0.25f, 1.0f);
            }
        }

        int shieldSkill = GetEffectiveSkill(agent, CrpgSkills.Shield);
        float coverageFactorForShieldCoef = agent.HasMount
            ? _constants.CavalryCoverageFactorForShieldCoef
            : _constants.InfantryCoverageFactorForShieldCoef;
        props.AttributeShieldMissileCollisionBodySizeAdder = shieldSkill * coverageFactorForShieldCoef;
        float ridingAttribute = agent.MountAgent?.GetAgentDrivenPropertyValue(DrivenProperty.AttributeRiding) ?? 1f;
        props.AttributeRiding = ridingSkill * ridingAttribute;
        // TODO: AttributeHorseArchery doesn't seem to have any effect for now.
        /*
        props.AttributeHorseArchery = Game.Current.BasicModels.StrikeMagnitudeModel.CalculateHorseArcheryFactor(character);*/

        SetAiProperties(agent, props, equippedItem, secondaryItem);
    }

    /// <summary>
    /// Copied from <see cref="AgentStatCalculateModel.SetAiRelatedProperties(Agent, AgentDrivenProperties, WeaponComponentData, WeaponComponentData)"/> to enable customisation of AI stats.
    /// </summary>
    private void SetAiProperties(Agent agent, AgentDrivenProperties props, WeaponComponentData? equippedItem, WeaponComponentData? secondaryItem)
    {
        const float levelMultiplier = 1f;
        int meleeSkill = GetMeleeSkill(agent, equippedItem, secondaryItem);
        int equippedItemSkill = GetEffectiveSkill(
            agent,
            equippedItem == null ? DefaultSkills.Athletics : equippedItem.RelevantSkill);
        float meleeLevel = CalculateAILevel(agent, meleeSkill) * levelMultiplier;
        float equippedItemLevel = CalculateAILevel(agent, equippedItemSkill) * levelMultiplier;
        float defenseLevel = meleeLevel + agent.Defensiveness;
        float difficultyModifier = GetDifficultyModifier();

        Debug.Assert(meleeLevel is >= 0 and <= 1f, $"{nameof(meleeLevel)} out of range");
        Debug.Assert(equippedItemLevel is >= 0 and <= 1f, $"{nameof(equippedItemLevel)} out of range");

        props.AiRangedHorsebackMissileRange = 0.3f + 0.4f * equippedItemLevel;
        props.AiFacingMissileWatch = -0.96f + meleeLevel * 0.06f;
        props.AiFlyingMissileCheckRadius = 8f - 6f * meleeLevel;
        props.AiShootFreq = 0.3f + 0.7f * equippedItemLevel;
        props.AiWaitBeforeShootFactor = agent.PropertyModifiers.resetAiWaitBeforeShootFactor ? 0f : 1f - 0.5f * equippedItemLevel;
        // Chance the AI parries. https://www.desmos.com/calculator/8tgpm4ajou
        props.AIBlockOnDecideAbility = MBMath.Lerp(0.2f, 0.99f, MBMath.ClampFloat(MathF.Pow(meleeLevel, 1.4f), 0f, 1f));
        // Chance the AI blocks (shield). https://www.desmos.com/calculator/geifioxm1g
        props.AIParryOnDecideAbility = MBMath.Lerp(0.2f, 0.95f, MBMath.ClampFloat(MathF.Pow(meleeLevel, 1.2f), 0f, 1f));
        // Chance the AI parries when it's attacking.
        props.AIParryOnAttackAbility = meleeLevel;
        // Chance the AI parries on chained attacks. https://www.desmos.com/calculator/pkclpc2ndr
        props.AIParryOnAttackingContinueAbility = MBMath.Lerp(0.2f, 0.95f, meleeLevel);
        // Chance the AI changes a parry direction. https://www.desmos.com/calculator/souzxw9965
        props.AiParryDecisionChangeValue = 0.05f + 0.7f * meleeLevel;
        props.AIRealizeBlockingFromIncorrectSideAbility = MBMath.ClampFloat(MathF.Pow(meleeLevel, 2.5f) - 0.01f, 0f, 1f);
        props.AiTryChamberAttackOnDecide = (meleeLevel - 0.15f) * 0.1f;
        props.AIAttackOnParryChance = 0.08f - 0.02f * agent.Defensiveness;
        props.AiAttackOnParryTiming = -0.2f + 0.3f * meleeLevel;
        props.AIDecideOnAttackChance = 0.5f * agent.Defensiveness;
        props.AiKick = -0.1f + (meleeLevel > 0.4f ? 0.4f : meleeLevel);
        props.AiAttackCalculationMaxTimeFactor = meleeLevel;
        props.AiDecideOnAttackWhenReceiveHitTiming = -0.25f * (1f - meleeLevel);
        props.AiDecideOnAttackContinueAction = -0.5f * (1f - meleeLevel);
        props.AiDecideOnAttackingContinue = 0.1f * meleeLevel;
        props.AIDecideOnRealizeEnemyBlockingAttackAbility = MBMath.ClampFloat(MathF.Pow(meleeLevel, 2.5f) - 0.1f, 0f, 1f);
        props.AiAttackingShieldDefenseChance = 0.2f + 0.3f * meleeLevel;
        props.AiAttackingShieldDefenseTimer = -0.3f + 0.3f * meleeLevel;
        props.AiRandomizedDefendDirectionChance = 1f - MathF.Pow(meleeLevel, 3f);
        props.AiShooterError = 0.008f;
        props.AISetNoAttackTimerAfterBeingHitAbility = MBMath.Lerp(0.33f, 1f, meleeLevel);
        props.AISetNoAttackTimerAfterBeingParriedAbility = MBMath.Lerp(0.2f, 1f, meleeLevel * meleeLevel);
        props.AISetNoDefendTimerAfterHittingAbility = MBMath.Lerp(0.1f, 0.99f, meleeLevel * meleeLevel);
        props.AISetNoDefendTimerAfterParryingAbility = MBMath.Lerp(0.15f, 1f, meleeLevel * meleeLevel);
        props.AIEstimateStunDurationPrecision = 1f - MBMath.Lerp(0.2f, 1f, meleeLevel);
        props.AIHoldingReadyMaxDuration = MBMath.Lerp(0.25f, 0f, MathF.Min(1f, meleeLevel * 2f));
        props.AIHoldingReadyVariationPercentage = meleeLevel;
        props.AiRaiseShieldDelayTimeBase = -0.75f + 0.5f * meleeLevel;
        props.AiUseShieldAgainstEnemyMissileProbability = 0.1f + meleeLevel * 0.6f + defenseLevel * 0.2f;
        props.AiCheckApplyMovementInterval = (2f - difficultyModifier) * (0.05f + 0.005f * (1.1f - meleeLevel));
        props.AiCheckCalculateMovementInterval = agent.HasMount || agent.IsMount ? 0.25f : (2f - difficultyModifier) * 0.25f;
        props.AiCheckDecideSimpleBehaviorInterval = (2f - difficultyModifier) * (agent.GetAgentFlags().HasAnyFlag(AgentFlag.CanWieldWeapon) ? 1.5f : 0.2f);
        props.AiCheckDoSimpleBehaviorInterval = 2f - difficultyModifier;
        props.AiMovementDelayFactor = 4f / (3f + equippedItemLevel);
        props.AiDefendWithShieldDecisionChanceValue = MathF.Min(2f, 0.5f + meleeLevel + 0.6f * defenseLevel);
        props.AiMoveEnemySideTimeValue = -2.5f + 0.5f * meleeLevel;
        props.AiMinimumDistanceToContinueFactor = 2f + 0.3f * (3f - meleeLevel);
        props.AiChargeHorsebackTargetDistFactor = 1.5f * (3f - meleeLevel);
        props.AiWaitBeforeShootFactor = agent.PropertyModifiers.resetAiWaitBeforeShootFactor ? 0f : 1f - 0.5f * equippedItemLevel;
        float equippedItemLevelComplement = 1f - equippedItemLevel;
        // "leading" a target means aiming ahead of a moving target. The AI will roll between min (shoot behind) and max
        // (shoot ahead). https://www.desmos.com/calculator/qrdv3rthef
        props.AiRangerLeadErrorMin = -equippedItemLevelComplement * 0.45f;
        props.AiRangerLeadErrorMax = equippedItemLevelComplement * 0.25f;
        // Aiming error. https://www.desmos.com/calculator/pqgcp48aoe
        props.AiRangerVerticalErrorMultiplier = equippedItemLevelComplement * 0.1f;
        props.AiRangerHorizontalErrorMultiplier = equippedItemLevelComplement * 0.035f;
        props.AiRangerVerticalErrorMultiplier = equippedItemLevelComplement * 0.15f;
        props.AiRangerHorizontalErrorMultiplier = equippedItemLevelComplement * 0.05f;
        props.AIAttackOnDecideChance = MathF.Clamp(0.1f * CalculateAIAttackOnDecideMaxValue() * (3f - agent.Defensiveness), 0.05f, 1f);
        props.SetStat(DrivenProperty.UseRealisticBlocking, agent.Controller != AgentControllerType.Player ? 1f : 0f);
        props.AiWeaponFavorMultiplierMelee = 1f;
        props.AiWeaponFavorMultiplierRanged = 1f;
        props.AiWeaponFavorMultiplierPolearm = 1f;
    }

    private float ImpactOfStrAndWeaponLengthOnCombatMaxSpeedMultiplier(int weaponLength, int strengthSkill)
    {
        return Math.Min(MBMath.Lerp(0.8f, 1f, MaxWeaponLengthForStrLevel(strengthSkill) / (float)weaponLength), 1f);
    }

    private float ImpactOfStrAndWeaponLengthOnTimeToMaxSpeed(int weaponLength, int strengthSkill)
    {
        return (float)Math.Max(1.2 * (weaponLength - MaxWeaponLengthForStrLevel(strengthSkill)) / MaxWeaponLengthForStrLevel(strengthSkill), 0f);
    }

    private int MaxWeaponLengthForStrLevel(int strengthSkill)
    {
        int uncappedMaxWeaponLength = (int)(22 + (strengthSkill - 3) * 7.5 + Math.Pow(Math.Min(strengthSkill - 3, 24) * 0.115f, 7.75f));
        return Math.Min(uncappedMaxWeaponLength, 650);
    }

    private float ImpactOfStrReqOnCrossbows(Agent agent, float impact, ItemObject? equippedItem)
    {
        if (equippedItem == null)
        {
            return 1;
        }

        float distanceToStrRequirement = CrossbowDistanceToStrRequirement(agent, equippedItem);
        return 1 / (1 + distanceToStrRequirement * impact);
    }

    private float CrossbowDistanceToStrRequirement(Agent agent, ItemObject? equippedItem)
    {
        if (equippedItem == null)
        {
            return 0;
        }

        int strengthAttribute = GetEffectiveSkill(agent, CrpgSkills.Strength);
        float setRequirement = CrpgItemRequirementModel.ComputeItemRequirement(equippedItem);
        return Math.Max(setRequirement - strengthAttribute, 0);
    }

    private bool HasSwingDamage(ItemObject? equippedItem)
    {
        if (equippedItem == null)
        {
            return false;
        }

        return equippedItem.WeaponComponent.Weapons.Any(a => a.SwingDamage > 0);
    }

    private float ComputePerceivedWeight(Agent agent)
    {
        if (agent.AgentDrivenProperties == null)
        {
            return 0f;
        }

        int strengthSkill = Math.Max(GetEffectiveSkill(agent, CrpgSkills.Strength), 3);
        const float awfulScaler = 3231477.548f;

        float[] weightReductionPolynomialFactor =
        [
            30f / awfulScaler,
            0.00005f / awfulScaler,
            0.5f / awfulScaler,
            1000000f / awfulScaler,
            0f,
        ];

        float weightReductionFactor = 1f / (1f + MathHelper.ApplyPolynomialFunction(strengthSkill - 3, weightReductionPolynomialFactor));
        float totalEncumbrance = agent.AgentDrivenProperties.ArmorEncumbrance + agent.AgentDrivenProperties.WeaponsEncumbrance;
        float freeWeight = 2.5f * (1 + (strengthSkill - 3f) / 30f);

        return Math.Max(totalEncumbrance - freeWeight, 0f) * weightReductionFactor;
    }
}
