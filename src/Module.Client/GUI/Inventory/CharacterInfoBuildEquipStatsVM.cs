using Crpg.Module.Api.Models.Characters;
using Crpg.Module.Common;
using Crpg.Module.Common.Models;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.GUI.Inventory;

// Majority of the formulas are ported from src/WebUI/src/Services/character-service.ts
// Some duplicated logic from src/Module.Server/Common/Models/CrpgCharacterStatCalculatedModel.cs
// If there any changes in either, they will need to be reflected here as well.
public class CharacterInfoBuildEquipStatsVM : ViewModel
{
    private readonly CrpgConstants _constants = default!;
    private string _cost = string.Empty;
    private string _avgRepairCost = string.Empty;
    private string _healthPoints = string.Empty;
    private string _weight = string.Empty;
    private string _freeWeight = string.Empty;
    private string _weightReduction = string.Empty;
    private string _perceivedWeight = string.Empty;
    private string _timeToMaxSpeed = string.Empty;
    private string _nakedSpeed = string.Empty;
    private string _currentSpeed = string.Empty;
    private string _maxWeaponLength = string.Empty;
    private string _movementSpeedPenaltyWhenAttacking = string.Empty;
    private string _mountSpeedPenalty = string.Empty;
    private string _additionalMountSpeedPenalty = string.Empty;

    public CharacterInfoBuildEquipStatsVM()
    {
        var behavior = Mission.Current?.GetMissionBehavior<CrpgCharacterLoadoutBehaviorClient>();
        if (behavior == null)
        {
            InformationManager.DisplayMessage(new InformationMessage("CrpgCharacterLoadoutBehaviorClient is required but not found in current mission", Colors.Red));
            return;
        }

        _constants = behavior.Constants;
    }

    public bool UpdateCharacterBuildEquipmentStatDisplay()
    {
        try
        {
            InformationManager.DisplayMessage(new InformationMessage("UpdateCharacterBuildEquipmentStatDisplay()", Colors.Red));
            var behavior = Mission.Current?.GetMissionBehavior<CrpgCharacterLoadoutBehaviorClient>();
            if (behavior == null)
            {
                InformationManager.DisplayMessage(new InformationMessage("CrpgCharacterLoadoutBehaviorClient is required but not found in current mission", Colors.Red));
                return false;
            }

            var equipment = behavior.GetCrpgUserCharacterEquipment();
            var characteristics = behavior.UserCharacter.Characteristics;

            var charSpeedStats = ComputeSpeedStats(characteristics.Attributes.Strength,
                characteristics.Skills.Athletics,
                characteristics.Attributes.Agility,
                ComputeOverallWeight(equipment),
                ComputeLongestWeaponLength(equipment));

            var mount = equipment[EquipmentIndex.Horse];
            var mountHarness = equipment[EquipmentIndex.HorseHarness];

            float harnessWeight = mountHarness.Item?.Weight ?? 0f;
            float mountSpeed = mount.Item?.HorseComponent?.Speed ?? 0f;

            var mountSpeedStats = ComputeMountSpeedStats(mountSpeed, harnessWeight, charSpeedStats.PerceivedWeight);

            double mountWeaponLengthPenalty = ComputeWeaponLengthMountPenalty(ComputeLongestWeaponLength(equipment), characteristics.Attributes.Strength);

            int totalCost = ComputeOverallPrice(equipment);

            // Stuff to display in UI
            Cost = $"{totalCost:N0}";
            AvgRepairCost = $"{ComputeAverageRepairCostPerHour(totalCost):N0} / h";
            HealthPoints = $"{ComputeHealthPoints(characteristics)}";
            Weight = $"{ComputeOverallWeight(equipment):F3}";
            FreeWeight = $"{Math.Min(ComputeOverallWeight(equipment), charSpeedStats.FreeWeight):F2}/{charSpeedStats.FreeWeight}";
            WeightReduction = $"{(charSpeedStats.WeightReductionFactor - 1) * 100:F2}%";
            PerceivedWeight = $"{charSpeedStats.PerceivedWeight:F3}";
            TimeToMaxSpeed = $"{charSpeedStats.TimeToMaxSpeed:F3}s";
            NakedSpeed = $"{charSpeedStats.NakedSpeed:F3}";
            CurrentSpeed = $"{charSpeedStats.CurrentSpeed:F3}";
            MaxWeaponLength = $"{charSpeedStats.MaxWeaponLength:F3}";
            MovementSpeedPenaltyWhenAttacking = $"{charSpeedStats.MovementSpeedPenaltyWhenAttacking}%";

            if (!mount.IsEmpty) // has a horse
            {
                MountSpeedPenalty = $"{mountSpeedStats.SpeedReduction:F3}";
                AdditionalMountSpeedPenalty = $"{mountWeaponLengthPenalty:F2}%";
            }
            else
            {
                MountSpeedPenalty = "--";
                AdditionalMountSpeedPenalty = "0.00%";
            }


            InformationManager.DisplayMessage(new InformationMessage("UpdateCharacterBuildEquipmentStatDisplay() FINISHED", Colors.Red));
        }
        catch (Exception ex)
        {
            InformationManager.DisplayMessage(new InformationMessage($"[ERROR] {ex.Message}", Colors.Red));
            InformationManager.DisplayMessage(new InformationMessage($"[ERROR] {ex.StackTrace}", Colors.Red));
        }

        return true;
    }

    private static CharacterSpeedStats ComputeSpeedStats(
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

    private static CharacterMountSpeedStats ComputeMountSpeedStats(
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

    private static double ComputeWeaponLengthMountPenalty(double weaponLength, int strength)
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

    private static double Clamp(double value, double min, double max) =>
        Math.Max(min, Math.Min(max, value));

    private static double ApplyPolynomialFunction(double x, double[] coefficients)
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
    private int ComputeHealthPoints(CrpgCharacterCharacteristics characteristics)
    {
        return _constants.DefaultHealthPoints + characteristics.Skills.IronFlesh * _constants.HealthPointsForIronFlesh
            + characteristics.Attributes.Strength * _constants.HealthPointsForStrength;
    }

    private int ComputeOverallPrice(Equipment equipment)
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

    private int ComputeAverageRepairCostPerHour(int price)
    {
        return (int)Math.Floor(price * _constants.ItemRepairCostPerSecond * 3600 * _constants.ItemBreakChance);
    }

    private int ComputeLongestWeaponLength(Equipment equipment)
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

    private float ComputeOverallWeight(Equipment equipment)
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

    [DataSourceProperty]
    public string Cost { get => _cost; set => SetField(ref _cost, value, nameof(Cost)); }
    [DataSourceProperty]
    public string AvgRepairCost { get => _avgRepairCost; set => SetField(ref _avgRepairCost, value, nameof(AvgRepairCost)); }
    [DataSourceProperty]
    public string HealthPoints { get => _healthPoints; set => SetField(ref _healthPoints, value, nameof(HealthPoints)); }

    [DataSourceProperty]
    public string Weight { get => _weight; set => SetField(ref _weight, value, nameof(Weight)); }

    [DataSourceProperty]
    public string FreeWeight { get => _freeWeight; set => SetField(ref _freeWeight, value, nameof(FreeWeight)); }

    [DataSourceProperty]
    public string WeightReduction { get => _weightReduction; set => SetField(ref _weightReduction, value, nameof(WeightReduction)); }

    [DataSourceProperty]
    public string PerceivedWeight { get => _perceivedWeight; set => SetField(ref _perceivedWeight, value, nameof(PerceivedWeight)); }

    [DataSourceProperty]
    public string TimeToMaxSpeed { get => _timeToMaxSpeed; set => SetField(ref _timeToMaxSpeed, value, nameof(TimeToMaxSpeed)); }

    [DataSourceProperty]
    public string NakedSpeed { get => _nakedSpeed; set => SetField(ref _nakedSpeed, value, nameof(NakedSpeed)); }

    [DataSourceProperty]
    public string CurrentSpeed { get => _currentSpeed; set => SetField(ref _currentSpeed, value, nameof(CurrentSpeed)); }

    [DataSourceProperty]
    public string MaxWeaponLength { get => _maxWeaponLength; set => SetField(ref _maxWeaponLength, value, nameof(MaxWeaponLength)); }

    [DataSourceProperty]
    public string MovementSpeedPenaltyWhenAttacking { get => _movementSpeedPenaltyWhenAttacking; set => SetField(ref _movementSpeedPenaltyWhenAttacking, value, nameof(MovementSpeedPenaltyWhenAttacking)); }

    [DataSourceProperty]
    public string MountSpeedPenalty { get => _mountSpeedPenalty; set => SetField(ref _mountSpeedPenalty, value, nameof(MountSpeedPenalty)); }

    [DataSourceProperty]
    public string AdditionalMountSpeedPenalty { get => _additionalMountSpeedPenalty; set => SetField(ref _additionalMountSpeedPenalty, value, nameof(AdditionalMountSpeedPenalty)); }
}
