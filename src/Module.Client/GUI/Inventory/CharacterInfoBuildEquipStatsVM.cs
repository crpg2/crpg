using Crpg.Module.Api.Models.Characters;
using Crpg.Module.Common;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Crpg.Module.GUI.Inventory;

// Majority of the formulas are ported from src/WebUI/src/Services/character-service.ts
// Some duplicated logic from src/Module.Server/Common/Models/CrpgCharacterStatCalculatedModel.cs
// If there any changes in either, they will need to be reflected here as well.
public class CharacterInfoBuildEquipStatsVM : ViewModel
{
    private readonly CrpgCharacterStatCalculations _statCalcInstance = default!;
    private enum StatIndex
    {
        Cost = 0,
        AvgRepairCost = 1,
        HealthPoints = 2,
        Weight = 3,
        FreeWeight = 4,
        WeightReduction = 5,
        PerceivedWeight = 6,
        TimeToMaxSpeed = 7,
        NakedSpeed = 8,
        CurrentSpeed = 9,
        MaxWeaponLength = 10,
        MovementSpeedPenaltyWhenAttacking = 11,
        MountSpeedPenalty = 12,
        AdditionalMountStatsPenalty = 13,
    }

    [DataSourceProperty]
    public MBBindingList<CharacterInfoBuildEquipStatsItemVM> Stats
    { get; set => SetField(ref field, value, nameof(Stats)); } = new();

    public CharacterInfoBuildEquipStatsVM()
    {
        _statCalcInstance = new CrpgCharacterStatCalculations(CrpgCharacterEquipUiHandler.Constants);
        Stats = new MBBindingList<CharacterInfoBuildEquipStatsItemVM>
        {
            new(new TextObject("{=KC9dxc52}Cost").ToString(), string.Empty, new TextObject("{=KC9dxc53}Total cost of all equipped items").ToString(), true),
            new(new TextObject("{=KC9dxc54}Avg repair cost").ToString(), string.Empty, new TextObject("{=KC9dxc55}Average repair cost per hour of all equipped items").ToString(), true),
            new(new TextObject("{=KC9dxc56}Health points").ToString(), string.Empty, new TextObject("{=KC9dxc57}Effective health points based on your build.").ToString()),
            new(new TextObject("{=KC9dxc58}Weight").ToString(), string.Empty, new TextObject("{=KC9dxc59}Weight").ToString()),
            new(new TextObject("{=KC9dxc60}Free weight").ToString(), string.Empty, new TextObject("{=KC9dxc61}Free weight / Maximum weight before penalties").ToString()),
            new(new TextObject("{=KC9dxc62}Weight reduction").ToString(), string.Empty, new TextObject("{=KC9dxc63}Remaining weight after deduction by free weight. Reduced by STR points").ToString()),
            new(new TextObject("{=KC9dxc64}Perceived weight").ToString(), string.Empty, new TextObject("{=KC9dxc65}Perceived weight").ToString()),
            new(new TextObject("{=KC9dxc66}Time to max speed").ToString(), string.Empty, new TextObject("{=KC9dxc67}Acceleration time to reach max speed of running").ToString()),
            new(new TextObject("{=KC9dxc68}Naked speed").ToString(), string.Empty, new TextObject("{=KC9dxc69}Speed of running with no equipment").ToString()),
            new(new TextObject("{=KC9dxc70}Current speed").ToString(), string.Empty, new TextObject("{=KC9dxc71}Current speed").ToString()),
            new(new TextObject("{=KC9dxc72}Max weapon length").ToString(), string.Empty, new TextObject("{=KC9dxc73}Max weapon length without a movement speed penalty when fighting. Depends on STR points").ToString()),
            new(new TextObject("{=KC9dxc74}Mov. speed penalty").ToString(), string.Empty, new TextObject("{=KC9dxc75}Extra movement speed penalty when attacking with a weapon based on length and STR").ToString()),
            new(new TextObject("{=KC9dxc76}Mount speed penalty").ToString(), string.Empty, new TextObject("{=KC9dxc77}Reduces speed based on rider perceived weight and harness weight").ToString()),
            new(new TextObject("{=KC9dxc78}Mount stats penalty").ToString(), string.Empty, new TextObject("{=KC9dxc79}Penalties to mount stats based on weapon length and rider STR").ToString()),
        };
    }

    internal void UpdateCharacterBuildEquipmentStatDisplay(Equipment equipment, CrpgCharacterCharacteristics characteristics)
    {
        try
        {
            float overallWeight = _statCalcInstance.ComputeOverallWeight(equipment);
            float longestWeaponLength = _statCalcInstance.ComputeLongestWeaponLength(equipment);

            var charSpeedStats = CrpgCharacterStatCalculations.ComputeSpeedStats(characteristics.Attributes.Strength,
                characteristics.Skills.Athletics,
                characteristics.Attributes.Agility,
                overallWeight,
                longestWeaponLength);

            var mount = equipment[EquipmentIndex.Horse];
            var mountHarness = equipment[EquipmentIndex.HorseHarness];

            float harnessWeight = mountHarness.Item?.Weight ?? 0f;
            float mountSpeed = mount.Item?.HorseComponent?.Speed ?? 0f;

            var mountSpeedStats = CrpgCharacterStatCalculations.ComputeMountSpeedStats(mountSpeed, harnessWeight, charSpeedStats.PerceivedWeight);

            double mountWeaponLengthPenalty = CrpgCharacterStatCalculations.ComputeWeaponLengthMountPenalty(longestWeaponLength, characteristics.Attributes.Strength);

            int totalCost = _statCalcInstance.ComputeOverallPrice(equipment);

            // Stuff to display in UI
            string cost = $"{totalCost:N0}";
            string avgRepairCost = $"{_statCalcInstance.ComputeAverageRepairCostPerHour(totalCost):N0} / h";
            string healthPoints = $"{_statCalcInstance.ComputeHealthPoints(characteristics)}";
            string weight = $"{overallWeight:F3}";
            string freeWeight = $"{Math.Min(overallWeight, charSpeedStats.FreeWeight):F2}/{charSpeedStats.FreeWeight:F2}";
            string weightReduction = $"{(charSpeedStats.WeightReductionFactor - 1) * 100:F2}%";
            string perceivedWeight = $"{charSpeedStats.PerceivedWeight:F3}";
            string timeToMaxSpeed = $"{charSpeedStats.TimeToMaxSpeed:F3}s";
            string nakedSpeed = $"{charSpeedStats.NakedSpeed:F3}";
            string currentSpeed = $"{charSpeedStats.CurrentSpeed:F3}";
            string maxWeaponLength = $"{charSpeedStats.MaxWeaponLength:F3}";
            string movementSpeedPenaltyWhenAttacking = $"{charSpeedStats.MovementSpeedPenaltyWhenAttacking:F2}%";
            string mountSpeedPenalty;
            string additionalMountStatsPenalty;

            bool moveSpeedTextDisabledState = charSpeedStats.MovementSpeedPenaltyWhenAttacking < 0;
            bool mountSpeedTextDisabledState = false;
            bool addMountStatsTextDisabledState = false;

            double penaltyPercent;

            // Always calculate the additional mount stats penalty
            penaltyPercent = (mountWeaponLengthPenalty - 1.0) * 100.0;
            additionalMountStatsPenalty = $"{penaltyPercent:F2}%";
            addMountStatsTextDisabledState = penaltyPercent < 0;

            if (!mount.IsEmpty) // has a horse
            {
                penaltyPercent = mountSpeedStats.SpeedReduction * 100.0;
                mountSpeedPenalty = $"{penaltyPercent:F2}%";
                mountSpeedTextDisabledState = mountSpeedStats.SpeedReduction < 0;
            }
            else
            {
                mountSpeedPenalty = "--";
                mountSpeedTextDisabledState = false;
            }

            UpdateStat(StatIndex.Cost, cost, true);
            UpdateStat(StatIndex.AvgRepairCost, avgRepairCost, true);
            UpdateStat(StatIndex.HealthPoints, healthPoints);
            UpdateStat(StatIndex.Weight, weight);
            UpdateStat(StatIndex.FreeWeight, freeWeight);
            UpdateStat(StatIndex.WeightReduction, weightReduction);
            UpdateStat(StatIndex.PerceivedWeight, perceivedWeight);
            UpdateStat(StatIndex.TimeToMaxSpeed, timeToMaxSpeed);
            UpdateStat(StatIndex.NakedSpeed, nakedSpeed);
            UpdateStat(StatIndex.CurrentSpeed, currentSpeed);
            UpdateStat(StatIndex.MaxWeaponLength, maxWeaponLength);
            UpdateStat(StatIndex.MovementSpeedPenaltyWhenAttacking, movementSpeedPenaltyWhenAttacking, false, moveSpeedTextDisabledState);
            UpdateStat(StatIndex.MountSpeedPenalty, mountSpeedPenalty, false, mountSpeedTextDisabledState);
            UpdateStat(StatIndex.AdditionalMountStatsPenalty, additionalMountStatsPenalty, false, addMountStatsTextDisabledState);

            OnPropertyChanged(nameof(Stats));
        }
        catch (Exception ex)
        {
            InformationManager.DisplayMessage(new InformationMessage($"[ERROR] {ex.Message}", Colors.Red));
            InformationManager.DisplayMessage(new InformationMessage($"[ERROR] {ex.StackTrace}", Colors.Red));
            Debug.Print($"[ERROR] {ex.Message}\n{ex.StackTrace}");
        }
    }

    private void UpdateStat(StatIndex index, string value, bool shouldShowGold = false, bool isValueDisabled = false)
    {
        if ((int)index < Stats.Count)
        {
            Stats[(int)index].ValueText = value;
            Stats[(int)index].ShouldShowGold = shouldShowGold;
            Stats[(int)index].TextDisabledState = isValueDisabled;
        }
    }
}
