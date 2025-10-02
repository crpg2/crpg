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
    private readonly CrpgConstants _constants = default!;
    private readonly CrpgCharacterStatCalculations _statCalcInstance = default!;
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
        var behavior = Mission.Current?.GetMissionBehavior<CrpgCharacterLoadoutBehaviorClient>();
        if (behavior == null)
        {
            InformationManager.DisplayMessage(new InformationMessage("CrpgCharacterLoadoutBehaviorClient is required but not found in current mission", Colors.Red));
            return;
        }

        _constants = behavior.Constants;

        _statCalcInstance = new CrpgCharacterStatCalculations(_constants);
    }

    internal bool UpdateCharacterBuildEquipmentStatDisplay(Equipment equipment, CrpgCharacterCharacteristics characteristics)
    {
        try
        {
            // InformationManager.DisplayMessage(new InformationMessage("UpdateCharacterBuildEquipmentStatDisplay()", Colors.Red));
            var behavior = Mission.Current?.GetMissionBehavior<CrpgCharacterLoadoutBehaviorClient>();
            if (behavior == null)
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
            Cost = $"{totalCost:N0}";
            AvgRepairCost = $"{_statCalcInstance.ComputeAverageRepairCostPerHour(totalCost):N0} / h";
            HealthPoints = $"{_statCalcInstance.ComputeHealthPoints(characteristics)}";
            Weight = $"{_statCalcInstance.ComputeOverallWeight(equipment):F3}";
            FreeWeight = $"{Math.Min(_statCalcInstance.ComputeOverallWeight(equipment), charSpeedStats.FreeWeight):F2}/{charSpeedStats.FreeWeight:F2}";
            WeightReduction = $"{(charSpeedStats.WeightReductionFactor - 1) * 100:F2}%";
            PerceivedWeight = $"{charSpeedStats.PerceivedWeight:F3}";
            TimeToMaxSpeed = $"{charSpeedStats.TimeToMaxSpeed:F3}s";
            NakedSpeed = $"{charSpeedStats.NakedSpeed:F3}";
            CurrentSpeed = $"{charSpeedStats.CurrentSpeed:F3}";
            MaxWeaponLength = $"{charSpeedStats.MaxWeaponLength:F3}";
            MovementSpeedPenaltyWhenAttacking = $"{charSpeedStats.MovementSpeedPenaltyWhenAttacking:F2}%";

            MoveSpeedTextDisabledState = charSpeedStats.MovementSpeedPenaltyWhenAttacking < 0;

            double penaltyPercent = 0;
            if (!mount.IsEmpty) // has a horse
            {
                penaltyPercent = mountSpeedStats.SpeedReduction * 100.0;
                MountSpeedPenalty = $"{penaltyPercent:F2}%";
                MountSpeedTextDisabledState = mountSpeedStats.SpeedReduction < 0;

                // Turn multiplier into penalty percent
                penaltyPercent = (mountWeaponLengthPenalty - 1.0) * 100.0;
                AdditionalMountSpeedPenalty = $"{penaltyPercent:F2}%";
                AddMountSpeedTextDisabledState = penaltyPercent < 0;
            }
            else
            {
                MountSpeedPenalty = "--";
                AdditionalMountSpeedPenalty = "0.00%";
                MountSpeedTextDisabledState = false;
                AddMountSpeedTextDisabledState = false;
            }

            // InformationManager.DisplayMessage(new InformationMessage("UpdateCharacterBuildEquipmentStatDisplay() FINISHED"));
        }
        catch (Exception ex)
        {
            InformationManager.DisplayMessage(new InformationMessage($"[ERROR] {ex.Message}", Colors.Red));
            InformationManager.DisplayMessage(new InformationMessage($"[ERROR] {ex.StackTrace}", Colors.Red));
        }

        return true;
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

    [DataSourceProperty]
    public bool MoveSpeedTextDisabledState { get => _moveSpeedTextDisabledState; set => SetField(ref _moveSpeedTextDisabledState, value, nameof(MoveSpeedTextDisabledState)); }

    [DataSourceProperty]
    public bool MountSpeedTextDisabledState { get => _mountSpeedTextDisabledState; set => SetField(ref _mountSpeedTextDisabledState, value, nameof(MountSpeedTextDisabledState)); }

    [DataSourceProperty]
    public bool AddMountSpeedTextDisabledState { get => _addMountSpeedTextDisabledState; set => SetField(ref _addMountSpeedTextDisabledState, value, nameof(AddMountSpeedTextDisabledState)); }
}
