using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common.Models;

/// <summary>
/// Used to adjust raw dmg calculations.
/// </summary>
internal class CrpgStrikeMagnitudeModel : MultiplayerStrikeMagnitudeModel
{
    /// <summary>
    /// This constants was introduced to decorelate damage from the physics system.
    /// Now damage dealts by a weapon only depends on the blade damage factor and where the blade hit the defender.
    /// </summary>
    public const float BladeDamageFactorToDamageRatio = 10f;

    private readonly CrpgConstants _constants;

    public CrpgStrikeMagnitudeModel(CrpgConstants constants)
    {
        _constants = constants;
    }

    public override float GetBluntDamageFactorByDamageType(DamageTypes damageType)
    {
        float result = 0f;
        switch (damageType)
        {
            case DamageTypes.Blunt:
                result = 1f; // Native 1f
                break;
            case DamageTypes.Cut:
                result = 0.35f; // Native .1f
                break;
            case DamageTypes.Pierce:
                result = .45f; // Native .25f
                break;
        }

        return result;
    }

    public override float CalculateStrikeMagnitudeForSwing(
        in AttackInformation attackInformation,
        in AttackCollisionData collisionData,
        in MissionWeapon weapon,
        float swingSpeed,
        float impactPoint,
        float extraLinearSpeed)
    {
        float impactPointFactor;
        float swingSpeedPercentage = swingSpeed * 4.5454545f / weapon.CurrentUsageItem.SwingSpeed; // should be replaced by attack progress , but needs validation
        float extraLinearSpeedSign = Math.Sign(extraLinearSpeed);
        float magnitudeBonusFromExtraSpeed = extraLinearSpeedSign * (float)(Math.Pow(extraLinearSpeedSign * extraLinearSpeed / 20f, 0.7f) + Math.Pow(extraLinearSpeed / 22f, 4f));
        switch (weapon.CurrentUsageItem.WeaponClass)
        {
            case WeaponClass.OneHandedAxe:
            case WeaponClass.TwoHandedAxe:
            case WeaponClass.Mace:
            case WeaponClass.TwoHandedMace:
            case WeaponClass.OneHandedPolearm:
            case WeaponClass.TwoHandedPolearm:
            case WeaponClass.LowGripPolearm:
                impactPointFactor = (float)Math.Pow(10f, -1.3f * Math.Pow(impactPoint - 0.85f, 2f));
                return BladeDamageFactorToDamageRatio *
                    (0.4f + 0.6f * impactPointFactor) *
                    (float)(Math.Pow(swingSpeedPercentage, 5f) + magnitudeBonusFromExtraSpeed);

            default: // Weapon that do not have a wooden handle
                impactPointFactor = (float)Math.Pow(10f, -1.5f * Math.Pow(impactPoint - 0.8f, 2f));
                return BladeDamageFactorToDamageRatio
                    * (0.7f + 0.3f * impactPointFactor)
                    * (float)(Math.Pow(swingSpeedPercentage, 5f) + magnitudeBonusFromExtraSpeed);
        }
    }
    public override float CalculateStrikeMagnitudeForThrust(
        in AttackInformation attackInformation,
        in AttackCollisionData collisionData,
        in MissionWeapon weapon,
        float thrustWeaponSpeed,
        float extraLinearSpeed,
        bool isThrown = false)
    {
        float thrustSpeedPercentage = thrustWeaponSpeed * 11.7647057f / weapon.CurrentUsageItem.ThrustSpeed; // should be replaced by attack progress , but needs validation
        float extraLinearSpeedSign = Math.Sign(extraLinearSpeed);
        float magnitudeBonusFromExtraSpeed = extraLinearSpeedSign * (float)(Math.Pow(extraLinearSpeedSign * extraLinearSpeed / 20f, 0.7f) + Math.Pow(extraLinearSpeed / 22f, 4f));
        switch (weapon.CurrentUsageItem.WeaponClass)
        {
            case WeaponClass.OneHandedSword:
            case WeaponClass.Dagger:
                 return
                    BladeDamageFactorToDamageRatio
                    * (float)(Math.Pow(thrustSpeedPercentage, 2f) + magnitudeBonusFromExtraSpeed);
            default:
                 return BladeDamageFactorToDamageRatio *
                    (float)(Math.Pow(thrustSpeedPercentage, 6f) + magnitudeBonusFromExtraSpeed);
        }
    }

    public override float CalculateAdjustedArmorForBlow(
        float baseArmor,
        BasicCharacterObject attackerCharacter,
        BasicCharacterObject attackerCaptainCharacter,
        BasicCharacterObject victimCharacter,
        BasicCharacterObject victimCaptainCharacter,
        WeaponComponentData weaponComponent)
    {
        if (weaponComponent == null)
        {
            return baseArmor;
        }

        return baseArmor * weaponComponent.WeaponClass switch
        {
            WeaponClass.Arrow => 1.2f,
            WeaponClass.Bolt => 0.9f,
            WeaponClass.Stone => 0.95f,
            WeaponClass.Boulder => 0.9f,
            WeaponClass.ThrowingAxe => 1.3f,
            WeaponClass.ThrowingKnife => 1.2f,
            WeaponClass.Javelin => 1.1f,
            _ => 1f,
        };
    }
}
