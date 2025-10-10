using Crpg.Module.Api.Models.Characters;
using Crpg.Module.Common;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.GUI.Inventory;

// Majority of the formulas are ported from src/WebUI/src/Services/character-service.ts
// Some duplicated logic from src/Module.Server/Common/Models/CrpgCharacterStatCalculatedModel.cs
// If there any changes in either, they will need to be reflected here as well.
public class CharacterInfoBuildEquipStatsVM : ViewModel
{
    private CrpgCharacterLoadoutBehaviorClient? UserLoadoutBehavior { get; set; }
    private readonly CrpgCharacterStatCalculations _statCalcInstance = default!;
    private MBBindingList<CharacterInfoBuildEquipStatsItemVM> _stats = new();
    [DataSourceProperty]
    public MBBindingList<CharacterInfoBuildEquipStatsItemVM> Stats
    {
        get => _stats;
        set => SetField(ref _stats, value, nameof(Stats));
    }

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
    private bool _moveSpeedTextDisabledState = false;
    private bool _mountSpeedTextDisabledState = false;
    private bool _addMountSpeedTextDisabledState = false;

    public CharacterInfoBuildEquipStatsVM()
    {
        UserLoadoutBehavior = Mission.Current?.GetMissionBehavior<CrpgCharacterLoadoutBehaviorClient>();
        if (UserLoadoutBehavior == null)
        {
            InformationManager.DisplayMessage(new InformationMessage("CrpgCharacterLoadoutBehaviorClient is required but not found in current mission", Colors.Red));
            return;
        }

        _statCalcInstance = new CrpgCharacterStatCalculations(UserLoadoutBehavior.Constants);

        _stats = new MBBindingList<CharacterInfoBuildEquipStatsItemVM>
        {
            new("Cost", _cost, "Total cost of all equipped items", true),
            new("Avg repair cost", _avgRepairCost, "Average repair cost per hour of all equipped items", true),
            new("Health points", _healthPoints, "Effective health points based on your build."),
            new("Weight", _weight, "Weight"),
            new("Free weight", _freeWeight, "Free weight / Maximum weight before penalties"),
            new("Weight reduction", _weightReduction, "Remaining weight after deduction by free weight. Reduced by STR points"),
            new("Perceived weight", _perceivedWeight, "Percieved weight"),
            new("Time to max speed", _timeToMaxSpeed, "Acceleration time to reach max speed of running"),
            new("Naked speed", _nakedSpeed, "Speed of running with no equipment"),
            new("Current speed", _currentSpeed, "Current speed"),
            new("Max weapon length", _maxWeaponLength, "Max weapon length without a movement speed penalty when fighting. Depends on STR points"),
            new("Mov. speed penalty", _movementSpeedPenaltyWhenAttacking, "Extra movement speed penalty when attacking with a weapon based on length and STR"),
            new("Mount speed penalty", _mountSpeedPenalty, "Reduces speed based on rider percieved weight and harness weight"),
            new("Mount stats penalty", _additionalMountSpeedPenalty, "Penalties to mount stats based on weapon length and rider STR"),
        };
    }

    internal bool UpdateCharacterBuildEquipmentStatDisplay(Equipment equipment, CrpgCharacterCharacteristics characteristics)
    {
        try
        {
            if (UserLoadoutBehavior == null)
            {
                InformationManager.DisplayMessage(new InformationMessage("CrpgCharacterLoadoutBehaviorClient is required but not found in current mission", Colors.Red));
                return false;
            }

            var charSpeedStats = CrpgCharacterStatCalculations.ComputeSpeedStats(characteristics.Attributes.Strength,
                characteristics.Skills.Athletics,
                characteristics.Attributes.Agility,
                _statCalcInstance.ComputeOverallWeight(equipment),
                _statCalcInstance.ComputeLongestWeaponLength(equipment));

            var mount = equipment[EquipmentIndex.Horse];
            var mountHarness = equipment[EquipmentIndex.HorseHarness];

            float harnessWeight = mountHarness.Item?.Weight ?? 0f;
            float mountSpeed = mount.Item?.HorseComponent?.Speed ?? 0f;

            var mountSpeedStats = CrpgCharacterStatCalculations.ComputeMountSpeedStats(mountSpeed, harnessWeight, charSpeedStats.PerceivedWeight);

            double mountWeaponLengthPenalty = CrpgCharacterStatCalculations.ComputeWeaponLengthMountPenalty(_statCalcInstance.ComputeLongestWeaponLength(equipment), characteristics.Attributes.Strength);

            int totalCost = _statCalcInstance.ComputeOverallPrice(equipment);

            // Stuff to display in UI
            _cost = $"{totalCost:N0}";
            _avgRepairCost = $"{_statCalcInstance.ComputeAverageRepairCostPerHour(totalCost):N0} / h";
            _healthPoints = $"{_statCalcInstance.ComputeHealthPoints(characteristics)}";
            _weight = $"{_statCalcInstance.ComputeOverallWeight(equipment):F3}";
            _freeWeight = $"{Math.Min(_statCalcInstance.ComputeOverallWeight(equipment), charSpeedStats.FreeWeight):F2}/{charSpeedStats.FreeWeight:F2}";
            _weightReduction = $"{(charSpeedStats.WeightReductionFactor - 1) * 100:F2}%";
            _perceivedWeight = $"{charSpeedStats.PerceivedWeight:F3}";
            _timeToMaxSpeed = $"{charSpeedStats.TimeToMaxSpeed:F3}s";
            _nakedSpeed = $"{charSpeedStats.NakedSpeed:F3}";
            _currentSpeed = $"{charSpeedStats.CurrentSpeed:F3}";
            _maxWeaponLength = $"{charSpeedStats.MaxWeaponLength:F3}";
            _movementSpeedPenaltyWhenAttacking = $"{charSpeedStats.MovementSpeedPenaltyWhenAttacking:F2}%";

            _moveSpeedTextDisabledState = charSpeedStats.MovementSpeedPenaltyWhenAttacking < 0;

            double penaltyPercent = 0;
            if (!mount.IsEmpty) // has a horse
            {
                penaltyPercent = mountSpeedStats.SpeedReduction * 100.0;
                _mountSpeedPenalty = $"{penaltyPercent:F2}%";
                _mountSpeedTextDisabledState = mountSpeedStats.SpeedReduction < 0;

                // Turn multiplier into penalty percent
                penaltyPercent = (mountWeaponLengthPenalty - 1.0) * 100.0;
                _additionalMountSpeedPenalty = $"{penaltyPercent:F2}%";
                _addMountSpeedTextDisabledState = penaltyPercent < 0;
            }
            else
            {
                _mountSpeedPenalty = "--";
                _additionalMountSpeedPenalty = "0.00%";
                _mountSpeedTextDisabledState = false;
                _addMountSpeedTextDisabledState = false;
            }

            _stats = new MBBindingList<CharacterInfoBuildEquipStatsItemVM>
            {
                new("Cost", _cost, "Total cost of all equipped items", true),
                new("Avg repair cost", _avgRepairCost, "Average repair cost per hour of all equipped items", true),
                new("Health points", _healthPoints, "Effective health points based on your build."),
                new("Weight", _weight, "Weight"),
                new("Free weight", _freeWeight, "Free weight / Maximum weight before penalties"),
                new("Weight reduction", _weightReduction, "Remaining weight after deduction by free weight. Reduced by STR points"),
                new("Perceived weight", _perceivedWeight, "Percieved weight"),
                new("Time to max speed", _timeToMaxSpeed, "Acceleration time to reach max speed of running"),
                new("Naked speed", _nakedSpeed, "Speed of running with no equipment"),
                new("Current speed", _currentSpeed, "Current speed"),
                new("Max weapon length", _maxWeaponLength, "Max weapon length without a movement speed penalty when fighting. Depends on STR points"),
                new("Mov. speed penalty", _movementSpeedPenaltyWhenAttacking, "Extra movement speed penalty when attacking with a weapon based on length and STR", false, _moveSpeedTextDisabledState),
                new("Mount speed penalty", _mountSpeedPenalty, "Reduces speed based on rider percieved weight and harness weight", false, _mountSpeedTextDisabledState),
                new("Mount stats penalty", _additionalMountSpeedPenalty, "Penalties to mount stats based on weapon length and rider STR", false, _addMountSpeedTextDisabledState),
            };
            OnPropertyChanged(nameof(Stats));
        }
        catch (Exception ex)
        {
            InformationManager.DisplayMessage(new InformationMessage($"[ERROR] {ex.Message}", Colors.Red));
            InformationManager.DisplayMessage(new InformationMessage($"[ERROR] {ex.StackTrace}", Colors.Red));
            Debug.Print($"[ERROR] {ex.Message}\n{ex.StackTrace}");
        }

        return true;
    }
}
