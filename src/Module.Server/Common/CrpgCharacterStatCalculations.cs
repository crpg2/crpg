using Crpg.Module.Api.Models.Characters;
using TaleWorlds.Core;

namespace Crpg.Module.Common;

internal class CrpgCharacterStatCalculations
{
    private readonly CrpgConstants _constants;

    public CrpgCharacterStatCalculations(CrpgConstants constants)
    {
        _constants = constants ?? throw new ArgumentNullException(nameof(constants));
    }

    internal static CharacterSpeedStats ComputeSpeedStats(
                int strength,
                int athletics,
                int agility,
                double totalEncumbrance,
                double longestWeaponLength)
    {
        const double awfulScaler = 3231477.548;

        double[] weightReductionPolynomialFactor =
        {
            30 / awfulScaler,
            0.00005 / awfulScaler,
            1000000 / awfulScaler,
            0,
        };

        double weightReductionFactor =
            1 / (1 + ApplyPolynomialFunction(strength - 3, weightReductionPolynomialFactor));

        double freeWeight = 2.5 * (1 + (strength - 3) / 30.0);

        double perceivedWeight = Math.Max(totalEncumbrance - freeWeight, 0) * weightReductionFactor;

        double nakedSpeed = 0.58 + (0.034 * (20 * athletics + 2 * agility)) / 26.0;

        double currentSpeed = Clamp(
            nakedSpeed * Math.Pow(361 / (361 + Math.Pow(perceivedWeight, 5)), 0.055),
            0.1,
            1.5);

        double maxWeaponLength = Math.Min(
            22 + (strength - 3) * 7.5 + Math.Pow(Math.Min(strength - 3, 24) * 0.115, 7.75),
            650);

        double timeToMaxSpeedWeaponLengthTerm =
            Math.Max((1.2 * (longestWeaponLength - maxWeaponLength)) / maxWeaponLength, 0);

        double timeToMaxSpeed =
            0.8
            * (1 + perceivedWeight / 15)
            * (20 / (20 + Math.Pow((20 * athletics + 3 * agility) / 120.0, 2)))
            + timeToMaxSpeedWeaponLengthTerm;

        double movementSpeedPenaltyWhenAttacking =
            100 * (Math.Min(0.8 + (0.2 * (maxWeaponLength + 1)) / (longestWeaponLength + 1), 1) - 1);

        return new CharacterSpeedStats
        {
            CurrentSpeed = currentSpeed,
            FreeWeight = freeWeight,
            MaxWeaponLength = maxWeaponLength,
            MovementSpeedPenaltyWhenAttacking = movementSpeedPenaltyWhenAttacking,
            NakedSpeed = nakedSpeed,
            PerceivedWeight = perceivedWeight,
            TimeToMaxSpeed = timeToMaxSpeed,
            WeightReductionFactor = weightReductionFactor,
        };
    }

    internal static CharacterMountSpeedStats ComputeMountSpeedStats(
            double baseSpeed,
            double harnessWeight,
            double riderPerceivedWeight)
    {
        double totalEffectiveLoad = harnessWeight + riderPerceivedWeight;
        const double maxLoadReference = 50;

        double loadPercentage = Math.Min(totalEffectiveLoad / maxLoadReference, 1);

        double weightImpactOnSpeed = 1 / (1 + 0.333 * loadPercentage);

        double effectiveSpeed = (baseSpeed + 1) * 0.209 * weightImpactOnSpeed;
        double unmodifiedSpeed = (baseSpeed + 1) * 0.209;

        double speedReduction = (effectiveSpeed / unmodifiedSpeed) - 1;
        double acceleration = 1 / (2 + 8 * loadPercentage);

        return new CharacterMountSpeedStats
        {
            SpeedReduction = speedReduction,
            MountAcceleration = acceleration,
            EffectiveSpeed = effectiveSpeed,
            WeightImpactOnSpeed = weightImpactOnSpeed,
            LoadPercentage = loadPercentage,
        };
    }

    internal static double ComputeWeaponLengthMountPenalty(double weaponLength, int strength)
    {
        if (weaponLength <= 0 || strength <= 0)
        {
            return 1; // no penalty
        }

        double maxLength =
            22 + (strength - 3) * 7.5
            + Math.Pow(Math.Min(strength - 3, 24) * 0.115, 7.75);

        double ratio = Math.Min(maxLength / weaponLength, 1);
        double penaltyFactor = 0.8 + 0.2 * ratio;

        return penaltyFactor; // 1 = no penalty, <1 = reduction
    }

    // ---------------- Helpers ----------------

    internal static double Clamp(double value, double min, double max) =>
        Math.Max(min, Math.Min(max, value));

    internal static double ApplyPolynomialFunction(double x, double[] coefficients)
    {
        // coefficients[0]*x^3 + coefficients[1]*x^2 + coefficients[2]*x + coefficients[3]
        double result = 0;
        int power = coefficients.Length - 1;
        for (int i = 0; i < coefficients.Length; i++)
        {
            result += coefficients[i] * Math.Pow(x, power - i);
        }

        return result;
    }

    // Based on src/WebUI/src/Services/character-service.ts
    internal int ComputeHealthPoints(CrpgCharacterCharacteristics characteristics)
    {
        return _constants.DefaultHealthPoints + characteristics.Skills.IronFlesh * _constants.HealthPointsForIronFlesh
            + characteristics.Attributes.Strength * _constants.HealthPointsForStrength;
    }

    internal int ComputeOverallPrice(Equipment equipment)
    {
        int totalPrice = 0;
        for (int i = 0; i < (int)EquipmentIndex.NumEquipmentSetSlots; i++)
        {
            var element = equipment[i];
            if (element.Item == null)
            {
                continue;
            }

            var item = element.Item;
            totalPrice += item.Value;
        }

        return totalPrice;
    }

    internal int ComputeAverageRepairCostPerHour(int price)
    {
        return (int)Math.Floor(price * _constants.ItemRepairCostPerSecond * 3600 * _constants.ItemBreakChance);
    }

    internal int ComputeLongestWeaponLength(Equipment equipment)
    {
        int longestLength = 0;
        for (int i = (int)EquipmentIndex.WeaponItemBeginSlot; i < (int)EquipmentIndex.NumAllWeaponSlots; i++)
        {
            var element = equipment[i];
            if (element.Item == null)
            {
                continue;
            }

            var item = element.Item;

            // Check if item type is one of the valid weapon categories
            if (item.Type == ItemObject.ItemTypeEnum.OneHandedWeapon ||
                item.Type == ItemObject.ItemTypeEnum.TwoHandedWeapon ||
                item.Type == ItemObject.ItemTypeEnum.Polearm)
            {
                int length = item.Weapons[0].WeaponLength;
                if (length > longestLength)
                {
                    longestLength = length;
                }
            }
        }

        return longestLength;
    }

    internal float ComputeOverallWeight(Equipment equipment)
    {
        float totalWeight = 0f;

        for (int i = 0; i < (int)EquipmentIndex.NumEquipmentSetSlots; i++)
        {
            var element = equipment[i];
            if (element.Item == null)
            {
                continue;
            }

            var item = element.Item;

            // Skip mounts & harnesses
            if (item.Type == ItemObject.ItemTypeEnum.Horse ||
                item.Type == ItemObject.ItemTypeEnum.HorseHarness)
            {
                continue;
            }

            // Handle stackable ammo (arrows, bolts, bullets, thrown weapons)
            if (item.Type == ItemObject.ItemTypeEnum.Arrows ||
                item.Type == ItemObject.ItemTypeEnum.Bolts ||
                item.Type == ItemObject.ItemTypeEnum.Bullets ||
                item.Type == ItemObject.ItemTypeEnum.Thrown)
            {
                // Multiply weight by stack size
                totalWeight += item.Weight * item.Weapons[0].MaxDataValue;
                // In Bannerlord, MaxDataValue for ammo weapons = stack amount
            }
            else
            {
                // Normal items, just add base weight
                totalWeight += item.Weight;
            }
        }

        return (float)Math.Round(totalWeight, 2); // match TS roundFloat
    }

    // ---------------- DTOs ----------------

    public class CharacterSpeedStats
    {
        public double CurrentSpeed { get; set; }
        public double FreeWeight { get; set; }
        public double MaxWeaponLength { get; set; }
        public double MovementSpeedPenaltyWhenAttacking { get; set; }
        public double NakedSpeed { get; set; }
        public double PerceivedWeight { get; set; }
        public double TimeToMaxSpeed { get; set; }
        public double WeightReductionFactor { get; set; }
    }

    public class CharacterMountSpeedStats
    {
        public double SpeedReduction { get; set; }
        public double MountAcceleration { get; set; }
        public double EffectiveSpeed { get; set; }
        public double WeightImpactOnSpeed { get; set; }
        public double LoadPercentage { get; set; }
    }
}
