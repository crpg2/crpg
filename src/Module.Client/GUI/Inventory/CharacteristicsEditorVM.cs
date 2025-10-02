using Crpg.Module.Api.Models;
using Crpg.Module.Api.Models.Characters;
using Crpg.Module.Common;
using Crpg.Module.Helpers;

using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;

namespace Crpg.Module.GUI.Inventory;

public class CharacteristicsEditorVM : ViewModel
{
    private readonly CrpgConstants _constants = default!;
    private readonly Dictionary<string, CharacteristicsPlusMinusItemVM> _skillMap = new();
    private readonly Dictionary<string, CharacteristicsPlusMinusItemVM> _weaponMap = new();
    private readonly TimeSpan _apiUsageClickCooldown = TimeSpan.FromSeconds(5);
    private CrpgCharacterLoadoutBehaviorClient? UserLoadoutBehavior { get; set; }

    private bool _isVisible;

    private int _skillPoints;
    private int _attributePoints;
    private int _weaponProficiencyPoints;

    private UserAndCharacterInfoVM _userAndCharacterInfoVm;
    private MBBindingList<CharacteristicsPlusMinusItemVM> _weaponProficiencies = new();
    private MBBindingList<CharacteristicsPlusMinusItemVM> _skills = new();
    private CharacteristicsPlusMinusItemVM _strengthVm;
    private CharacteristicsPlusMinusItemVM _agilityVm;
    private CharacteristicsConvertItemVM _convertAttributeVm;
    private CharacteristicsConvertItemVM _convertSkillVm;
    private CrpgCharacterCharacteristics _initialCharacteristics = new();

    private DateTime _lastApiCharacteristicsRefreshClick = DateTime.MinValue;
    private DateTime _lastApiCharacteristicsApplyClick = DateTime.MinValue;

    internal event Action? OnEditCharacteristicsChanged; // event to notify parent VM to update CharacterInfoBuildEquipStatsVM (before API call)

    /// <summary>
    /// Initializes a new instance of the <see cref="CharacteristicsEditorVM"/> class.
    /// with default values for attributes, skills, and weapon proficiencies.
    /// Also wires event handlers and builds internal lookup dictionaries.
    /// </summary>
    public CharacteristicsEditorVM()
    {
        _userAndCharacterInfoVm = new UserAndCharacterInfoVM();

        // Initialize attribute VMs
        _strengthVm = new CharacteristicsPlusMinusItemVM("Strength", 0);
        _agilityVm = new CharacteristicsPlusMinusItemVM("Agility", 0);

        // skillpoints
        _skillPoints = 0;

        // Convert buttons
        _convertAttributeVm = new CharacteristicsConvertItemVM("Attributes - ", 0, true);
        _convertSkillVm = new CharacteristicsConvertItemVM("Skills - ", 0, true);

        // Wpf points
        _weaponProficiencyPoints = 0;

        // Wpf PlusMinus
        _weaponProficiencies.Add(new CharacteristicsPlusMinusItemVM("One-Handed", 0));
        _weaponProficiencies.Add(new CharacteristicsPlusMinusItemVM("Two-Handed", 0));
        _weaponProficiencies.Add(new CharacteristicsPlusMinusItemVM("Polearm", 0));
        _weaponProficiencies.Add(new CharacteristicsPlusMinusItemVM("Bow", 0));
        _weaponProficiencies.Add(new CharacteristicsPlusMinusItemVM("Crossbow", 0));
        _weaponProficiencies.Add(new CharacteristicsPlusMinusItemVM("Throwing", 0));

        // Skill PlusMinus
        _skills.Add(new CharacteristicsPlusMinusItemVM("Iron Flesh", 0));
        _skills.Add(new CharacteristicsPlusMinusItemVM("Power Strike", 0));
        _skills.Add(new CharacteristicsPlusMinusItemVM("Power Draw", 0));
        _skills.Add(new CharacteristicsPlusMinusItemVM("Power Throw", 0));
        _skills.Add(new CharacteristicsPlusMinusItemVM("Athletics", 0));
        _skills.Add(new CharacteristicsPlusMinusItemVM("Riding", 0));
        _skills.Add(new CharacteristicsPlusMinusItemVM("Weapon Master", 0));
        _skills.Add(new CharacteristicsPlusMinusItemVM("Mounted Archery", 0));
        _skills.Add(new CharacteristicsPlusMinusItemVM("Shield", 0));

        // Subscribe clicks
        WireAllEvents(true);

        // Build the dictionaries after adding items to lists
        foreach (var skill in _skills)
        {
            _skillMap[skill.ItemLabel] = skill;
        }

        foreach (var wp in _weaponProficiencies)
        {
            _weaponMap[wp.ItemLabel] = wp;
        }

        UserLoadoutBehavior = Mission.Current?.GetMissionBehavior<CrpgCharacterLoadoutBehaviorClient>()
            ?? throw new InvalidOperationException("CrpgCharacterLoadoutBehaviorClient is required");
        _constants = UserLoadoutBehavior.Constants;

        // UpdateAllButtonStates();
    }

    /// <summary>
    /// Loads a <see cref="CrpgCharacter"/> into the ViewModel, updating all attributes,
    /// skills, weapon proficiencies, and available points. Also enforces min values,
    /// rebuilds lookup dictionaries, and updates button states.
    /// </summary>
    internal void SetCrpgCharacterBasic(CrpgCharacter newCharacter)
    {
        if (newCharacter == null)
        {
            return;
        }

        // Basic info
        // temp update UserAndCharacterInfoVM here
        UserAndCharacterInfoVm.UpdateUserAndCharacterInfo();

        // Clone characteristics for min value enforcement
        _initialCharacteristics = CloneCharacteristics(newCharacter.Characteristics);

        // Attributes
        StrengthVm.ItemValue = newCharacter.Characteristics.Attributes.Strength;
        AgilityVm.ItemValue = newCharacter.Characteristics.Attributes.Agility;
        AttributePoints = newCharacter.Characteristics.Attributes.Points;

        // Skill points
        SkillPoints = newCharacter.Characteristics.Skills.Points;

        // Convert buttons
        ConvertAttribute.ItemValue = AttributePoints;
        ConvertSkill.ItemValue = SkillPoints;

        // Rebuild dictionaries to match current lists
        _skillMap.Clear();
        foreach (var skill in Skills)
        {
            _skillMap[skill.ItemLabel] = skill;
        }

        _weaponMap.Clear();
        foreach (var wpf in WeaponProficiencies)
        {
            _weaponMap[wpf.ItemLabel] = wpf;
        }

        var sk = newCharacter.Characteristics.Skills;

        // Update skills safely
        TrySetSkillValue("Iron Flesh", sk.IronFlesh);
        TrySetSkillValue("Power Strike", sk.PowerStrike);
        TrySetSkillValue("Power Draw", sk.PowerDraw);
        TrySetSkillValue("Power Throw", sk.PowerThrow);
        TrySetSkillValue("Athletics", sk.Athletics);
        TrySetSkillValue("Riding", sk.Riding);
        TrySetSkillValue("Weapon Master", sk.WeaponMaster);
        TrySetSkillValue("Mounted Archery", sk.MountedArchery);
        TrySetSkillValue("Shield", sk.Shield);

        // Weapon proficiencies
        WeaponProficiencyPoints = newCharacter.Characteristics.WeaponProficiencies.Points;
        var wp = newCharacter.Characteristics.WeaponProficiencies;

        TrySetWeaponProficiency("One-Handed", wp.OneHanded);
        TrySetWeaponProficiency("Two-Handed", wp.TwoHanded);
        TrySetWeaponProficiency("Polearm", wp.Polearm);
        TrySetWeaponProficiency("Bow", wp.Bow);
        TrySetWeaponProficiency("Crossbow", wp.Crossbow);
        TrySetWeaponProficiency("Throwing", wp.Throwing);

        // Update buttons & states
        UpdateAllButtonStates();

        // WireAllEvents(true);
    }

    internal void SetInitialCharacteristics(CrpgCharacterCharacteristics characteristics)
    {
        if (characteristics == null)
        {
            return;
        }

        _initialCharacteristics = CloneCharacteristics(characteristics);

        ResetToInitialCharacteristics();
    }

    internal void UpdateCharacteristicsPointsAfterConversion(int attributePoints, int skillPoints)
    {
        AttributePoints = attributePoints;
        SkillPoints = skillPoints;
        _initialCharacteristics.Attributes.Points = attributePoints;
        _initialCharacteristics.Skills.Points = skillPoints;
    }

    internal CrpgCharacterCharacteristics GetCrpgCharacteristicsFromVM()
    {
        var characteristics = new CrpgCharacterCharacteristics
        {
            // Attributes
            Attributes = new CrpgCharacterAttributes
            {
                Strength = StrengthVm.ItemValue,
                Agility = AgilityVm.ItemValue,
                Points = AttributePoints,
            },

            // Skills
            Skills = new CrpgCharacterSkills
            {
                IronFlesh = _skillMap.TryGetValue("Iron Flesh", out var ironFlesh) ? ironFlesh.ItemValue : 0,
                PowerStrike = _skillMap.TryGetValue("Power Strike", out var powerStrike) ? powerStrike.ItemValue : 0,
                PowerDraw = _skillMap.TryGetValue("Power Draw", out var powerDraw) ? powerDraw.ItemValue : 0,
                PowerThrow = _skillMap.TryGetValue("Power Throw", out var powerThrow) ? powerThrow.ItemValue : 0,
                Athletics = _skillMap.TryGetValue("Athletics", out var athletics) ? athletics.ItemValue : 0,
                Riding = _skillMap.TryGetValue("Riding", out var riding) ? riding.ItemValue : 0,
                WeaponMaster = _skillMap.TryGetValue("Weapon Master", out var weaponMaster) ? weaponMaster.ItemValue : 0,
                MountedArchery = _skillMap.TryGetValue("Mounted Archery", out var mountedArchery) ? mountedArchery.ItemValue : 0,
                Shield = _skillMap.TryGetValue("Shield", out var shield) ? shield.ItemValue : 0,
                Points = SkillPoints,
            },

            // Weapon Proficiencies
            WeaponProficiencies = new CrpgCharacterWeaponProficiencies
            {
                OneHanded = _weaponMap.TryGetValue("One-Handed", out var oneHanded) ? oneHanded.ItemValue : 0,
                TwoHanded = _weaponMap.TryGetValue("Two-Handed", out var twoHanded) ? twoHanded.ItemValue : 0,
                Polearm = _weaponMap.TryGetValue("Polearm", out var polearm) ? polearm.ItemValue : 0,
                Bow = _weaponMap.TryGetValue("Bow", out var bow) ? bow.ItemValue : 0,
                Crossbow = _weaponMap.TryGetValue("Crossbow", out var crossbow) ? crossbow.ItemValue : 0,
                Throwing = _weaponMap.TryGetValue("Throwing", out var throwing) ? throwing.ItemValue : 0,
                Points = WeaponProficiencyPoints,
            },
        };

        return characteristics;
    }

    /// <summary>
    /// Resets the VM to the initial characteristics stored in _initialCharacteristics.
    /// Restores attributes, skills, weapon proficiencies, and available points.
    /// </summary>
    private void ResetToInitialCharacteristics()
    {
        if (_initialCharacteristics == null)
        {
            InformationManager.DisplayMessage(new InformationMessage($"ResetToInitialCharacteristics(): _initialCharacteristics is null.", Colors.Yellow));
            return;
        }

        // Restore attributes
        StrengthVm.ItemValue = _initialCharacteristics.Attributes.Strength;
        AgilityVm.ItemValue = _initialCharacteristics.Attributes.Agility;
        AttributePoints = _initialCharacteristics.Attributes.Points;

        // Restore skills
        foreach (var skill in Skills)
        {
            skill.ItemValue = GetInitialSkillValue(skill.ItemLabel);
        }

        SkillPoints = _initialCharacteristics.Skills.Points;

        // Restore weapon proficiencies
        foreach (var wp in WeaponProficiencies)
        {
            wp.ItemValue = GetInitialWeaponProficiencyValue(wp.ItemLabel);
        }

        WeaponProficiencyPoints = _initialCharacteristics.WeaponProficiencies.Points;

        // Optionally, update convert buttons
        ConvertAttribute.ItemValue = _initialCharacteristics.Attributes.Points;
        ConvertSkill.ItemValue = _initialCharacteristics.Skills.Points;

        // Refresh button states and text states
        UpdateAllButtonStates();
        ValidateAndUpdateAllSkills();
        UpdateWeaponProficiencyTextStates();

        OnEditCharacteristicsChanged?.Invoke();

        InformationManager.DisplayMessage(new InformationMessage($"ResetToInitialCharacteristics():  completed.", Colors.Cyan));
    }

    private void ExecuteClickApplyChanges()
    {
        var now = DateTime.UtcNow;
        if (now - _lastApiCharacteristicsApplyClick < _apiUsageClickCooldown)
        {
            InformationManager.DisplayMessage(new InformationMessage("Please wait to send another API request.", Colors.Yellow));
            // too soon display msg or play sound
            return;
        }

        _lastApiCharacteristicsApplyClick = now;

        UserLoadoutBehavior?.RequestUpdateCharacterCharacteristics(GetCrpgCharacteristicsFromVM());
        InformationManager.DisplayMessage(new InformationMessage("Requesting to update API characteristics.", Colors.Cyan));
    }

    private void ExecuteClickReset()
    {
        InformationManager.DisplayMessage(new InformationMessage("Resetting to initial characteristics.", Colors.Cyan));
        ResetToInitialCharacteristics();
    }

    private void ExecuteClickRefreshApi()
    {
        var now = DateTime.UtcNow;
        if (now - _lastApiCharacteristicsRefreshClick < _apiUsageClickCooldown)
        {
            // too soon display msg or play sound
            InformationManager.DisplayMessage(new InformationMessage("Please wait to send another API request.", Colors.Yellow));
            UISoundsHelper.PlayUISound("event:/ui/persuasion/critical_fail");
            return;
        }

        _lastApiCharacteristicsRefreshClick = now;
        UserLoadoutBehavior?.RequestGetUpdatedCharacterBasic();
        InformationManager.DisplayMessage(new InformationMessage("Requesting to refresh character from API.", Colors.Cyan));
        UISoundsHelper.PlayUISound("event:/ui/notification/trait_change");
    }

    /// <summary>
    /// Safely updates the value of a skill in the UI, using its string key.
    /// Displays an in-game message if the skill is not present in the lookup dictionary.
    /// </summary>
    private void TrySetSkillValue(string key, int value)
    {
        if (_skillMap.TryGetValue(key, out var vm))
        {
            vm.ItemValue = value;
        }
        else
        {
            InformationManager.DisplayMessage(
                new InformationMessage($"[CharacterInfoVM] Skill '{key}' not found in _skillMap!"));
        }
    }

    /// <summary>
    /// Safely updates the value of a weapon proficiency in the UI, using its string key.
    /// Displays an in-game message if the proficiency is not present in the lookup dictionary.
    /// </summary>
    private void TrySetWeaponProficiency(string key, int value)
    {
        if (_weaponMap.TryGetValue(key, out var vm))
        {
            vm.ItemValue = value;
        }
        else
        {
            InformationManager.DisplayMessage(
                new InformationMessage($"[CharacterInfoVM] Weapon Proficiency '{key}' not found in _weaponMap!"));
        }
    }

    /// <summary>
    /// Creates a deep copy of a character's characteristics so that initial values
    /// can be preserved for enforcing minimum values on reset/decrement operations.
    /// </summary>
    private CrpgCharacterCharacteristics CloneCharacteristics(CrpgCharacterCharacteristics source)
    {
        return new CrpgCharacterCharacteristics
        {
            Attributes = new CrpgCharacterAttributes
            {
                Strength = source.Attributes.Strength,
                Agility = source.Attributes.Agility,
                Points = source.Attributes.Points,
            },
            Skills = new CrpgCharacterSkills
            {
                IronFlesh = source.Skills.IronFlesh,
                PowerStrike = source.Skills.PowerStrike,
                PowerDraw = source.Skills.PowerDraw,
                PowerThrow = source.Skills.PowerThrow,
                Athletics = source.Skills.Athletics,
                Riding = source.Skills.Riding,
                WeaponMaster = source.Skills.WeaponMaster,
                MountedArchery = source.Skills.MountedArchery,
                Shield = source.Skills.Shield,
                Points = source.Skills.Points,
            },
            WeaponProficiencies = new CrpgCharacterWeaponProficiencies
            {
                OneHanded = source.WeaponProficiencies.OneHanded,
                TwoHanded = source.WeaponProficiencies.TwoHanded,
                Polearm = source.WeaponProficiencies.Polearm,
                Bow = source.WeaponProficiencies.Bow,
                Crossbow = source.WeaponProficiencies.Crossbow,
                Throwing = source.WeaponProficiencies.Throwing,
                Points = source.WeaponProficiencies.Points,
            },
        };
    }

    /// <summary>
    /// Subscribes or unsubscribes all event handlers for attributes, skills,
    /// weapon proficiencies, and conversion buttons in a single pass.
    /// </summary>
    private void WireAllEvents(bool subscribe)
    {
        // Convert buttons
        if (ConvertAttribute != null)
        {
            if (subscribe)
            {
                ConvertAttribute.OnConvertClickedEvent += OnConvertClicked;
            }
            else
            {
                ConvertAttribute.OnConvertClickedEvent -= OnConvertClicked;
            }
        }

        if (ConvertSkill != null)
        {
            if (subscribe)
            {
                ConvertSkill.OnConvertClickedEvent += OnConvertClicked;
            }
            else
            {
                ConvertSkill.OnConvertClickedEvent -= OnConvertClicked;
            }
        }

        // Attributes
        WirePlusMinusEvents(StrengthVm, subscribe);
        WirePlusMinusEvents(AgilityVm, subscribe);

        // Skills
        foreach (var skill in Skills)
        {
            WirePlusMinusEvents(skill, subscribe);
        }

        // Weapon proficiencies
        foreach (var wp in WeaponProficiencies)
        {
            WirePlusMinusEvents(wp, subscribe);
        }
    }

    /// <summary>
    /// Attaches or detaches plus/minus click event handlers for a given
    /// <see cref="CharacteristicsPlusMinusItemVM"/> instance.
    /// </summary>
    private void WirePlusMinusEvents(CharacteristicsPlusMinusItemVM item, bool subscribe)
    {
        if (item == null)
        {
            return;
        }

        if (subscribe)
        {
            item.OnPlusClickedEvent += OnPlusClicked;
            item.OnMinusClickedEvent += OnMinusClicked;
        }
        else
        {
            item.OnPlusClickedEvent -= OnPlusClicked;
            item.OnMinusClickedEvent -= OnMinusClicked;
        }
    }

    /// <summary>
    /// Handles logic when a convert button is clicked. Supports conversion of:
    /// <list type="bullet">
    ///   <item>1 Attribute point → 2 Skill points</item>
    ///   <item>2 Skill points → 1 Attribute point</item>
    /// </list>
    /// Updates skills, weapon proficiencies, and button states accordingly.
    /// </summary>
    private void OnConvertClicked(CharacteristicsConvertItemVM vm)
    {
        InformationManager.DisplayMessage(new InformationMessage($"OnConvertClicked: {vm.ItemLabel} : {vm.ItemValue}"));
        if (vm.Equals(ConvertAttribute))
        {
            // 1 attribute point = 2 skill points
            if (AttributePoints >= 1)
            {
                UserLoadoutBehavior?.RequestConvertCharacterCharacteristic(CrpgGameCharacteristicConversionRequest.AttributesToSkills);
                // AttributePoints--;
                // SkillPoints += 2;
            }
        }
        else if (vm.Equals(ConvertSkill))
        {
            // 2 skill points = 1 attribute point
            if (SkillPoints >= 2)
            {
                UserLoadoutBehavior?.RequestConvertCharacterCharacteristic(CrpgGameCharacteristicConversionRequest.SkillsToAttributes);
                // SkillPoints -= 2;
                // AttributePoints++;
            }
        }

        ValidateAndUpdateAllSkills();
        UpdateWeaponProficiencyTextStates();
        UpdateAllButtonStates();

        OnEditCharacteristicsChanged?.Invoke();
    }

    /// <summary>
    /// Handles logic when the plus button is clicked for attributes, skills,
    /// or weapon proficiencies. Increases values if requirements and available
    /// points are satisfied, and updates all related states. Also handles alternate clicks (+10) for WPF.
    /// </summary>
    private void OnPlusClicked(CharacteristicsPlusMinusItemVM item, bool wasAlternateClick)
    {
        if (!item.IsButtonPlusEnabled)
        {
            return;
        }

        if (item == StrengthVm && !wasAlternateClick)
        {
            if (AttributePoints > 0)
            {
                item.ItemValue++;
                AttributePoints--;
            }
        }
        else if (item == AgilityVm && !wasAlternateClick)
        {
            if (AttributePoints > 0)
            {
                item.ItemValue++;
                AttributePoints--;
            }
        }
        else if (Skills.Contains(item) && !wasAlternateClick)
        {
            if (SkillPoints > 0 && CheckSkillRequirement(item.ItemLabel, item.ItemValue + 1))
            {
                item.ItemValue++;
                SkillPoints--;
            }
        }
        else if (WeaponProficiencies.Contains(item))
        {
            if (wasAlternateClick)
            {
                int maxIncrease = 10;
                int increase = 1;

                for (int i = 1; i < maxIncrease; i++)
                {
                    int nextCost = WeaponProficiencyCost(item.ItemValue + 1) - WeaponProficiencyCost(item.ItemValue);
                    if (WeaponProficiencyPointsRemaining >= nextCost)
                    {
                        item.ItemValue++;
                        increase++;
                        UpdateWeaponProficiencyTextStates();
                        // UpdateAllButtonStates();
                    }
                    else
                    {
                        break;
                    }
                }

                Debug.Print($"Increased {item.ItemLabel} by {increase} (alt-click). Remaining points: {WeaponProficiencyPointsRemaining}");
            }
            else
            {
                int nextCost = WeaponProficiencyCost(item.ItemValue + 1) - WeaponProficiencyCost(item.ItemValue);
                if (WeaponProficiencyPointsRemaining >= nextCost)
                {
                    item.ItemValue++;
                }
            }
        }

        ValidateAndUpdateAllSkills();
        UpdateWeaponProficiencyTextStates();
        UpdateAllButtonStates();

        OnEditCharacteristicsChanged?.Invoke();
    }

    /// <summary>
    /// Handles logic when the minus button is clicked for attributes, skills,
    /// or weapon proficiencies. Decreases values down to their initial minimums,
    /// refunds points, and updates all related states. Also handles alternate clicks (-10) for WPF.
    /// </summary>
    private void OnMinusClicked(CharacteristicsPlusMinusItemVM item, bool wasAlternateClick)
    {
        if (!item.IsButtonMinusEnabled)
        {
            return;
        }

        if (item == StrengthVm && !wasAlternateClick)
        {
            if (item.ItemValue > _initialCharacteristics.Attributes.Strength && item.ItemValue > 0)
            {
                item.ItemValue--;
                AttributePoints++;
            }
        }
        else if (item == AgilityVm && !wasAlternateClick)
        {
            if (item.ItemValue > _initialCharacteristics.Attributes.Agility)
            {
                item.ItemValue--;
                AttributePoints++;
            }
        }
        else if (Skills.Contains(item) && !wasAlternateClick)
        {
            int minValue = GetInitialSkillValue(item.ItemLabel);
            if (item.ItemValue > minValue)
            {
                item.ItemValue--;
                SkillPoints++;
            }
        }
        else if (WeaponProficiencies.Contains(item))
        {
            int minValue = GetInitialWeaponProficiencyValue(item.ItemLabel);

            if (wasAlternateClick)
            {
                int decrease = Math.Min(10, item.ItemValue - minValue);
                if (decrease > 0)
                {
                    item.ItemValue -= decrease;
                    // Points auto-recalculate from getter, nothing to assign here
                }
            }
            else if (item.ItemValue > minValue)
            {
                item.ItemValue--;
            }
        }

        ValidateAndUpdateAllSkills();
        UpdateWeaponProficiencyTextStates();
        UpdateAllButtonStates();

        OnEditCharacteristicsChanged?.Invoke();
    }

    /// <summary>
    /// Recalculates and applies the enabled/disabled state of all plus, minus,
    /// and convert buttons based on current points, requirements, and min/max rules.
    /// </summary>
    private void UpdateAllButtonStates()
    {
        // Skill/Attribute Points
        ConvertSkill.IsButtonEnabled = ConvertSkill.ItemValue >= 2;
        ConvertAttribute.IsButtonEnabled = ConvertAttribute.ItemValue >= 1;

        // Attributes
        StrengthVm.IsButtonMinusEnabled = StrengthVm.ItemValue > _initialCharacteristics.Attributes.Strength;
        StrengthVm.IsButtonPlusEnabled = AttributePoints > 0;

        AgilityVm.IsButtonMinusEnabled = AgilityVm.ItemValue > _initialCharacteristics.Attributes.Agility;
        AgilityVm.IsButtonPlusEnabled = AttributePoints > 0;

        // Skills
        foreach (var skill in Skills)
        {
            int minValue = GetInitialSkillValue(skill.ItemLabel);
            skill.IsButtonMinusEnabled = skill.ItemValue > minValue;
            skill.IsButtonPlusEnabled = SkillPoints > 0 &&
                                        CheckSkillRequirement(skill.ItemLabel, skill.ItemValue + 1);
        }

        // Weapon proficiencies
        int remainingWP = WeaponProficiencyPointsRemaining;
        foreach (var wp in WeaponProficiencies)
        {
            int minValue = GetInitialWeaponProficiencyValue(wp.ItemLabel);
            int nextCost = WeaponProficiencyCost(wp.ItemValue + 1) - WeaponProficiencyCost(wp.ItemValue);

            // Plus enabled only if we have enough scaled points (computed canonical remaining)
            wp.IsButtonPlusEnabled = remainingWP >= nextCost;
            wp.IsButtonMinusEnabled = wp.ItemValue > minValue;
        }
    }

    private void UpdateWeaponProficiencyTextStates()
    {
        int rem = WeaponProficiencyPointsRemaining;
        WeaponProficiencyPoints = rem; // or format how you want
                                       // Optionally: show breakdown (initial + agility delta + wm delta - spent) for debugging
        bool isStateDisabled = !ValidateWeaponProficiencyCap();

        foreach (var wp in WeaponProficiencies)
        {
            wp.TextStateDisabled = isStateDisabled;
        }

        InformationManager.DisplayMessage(new InformationMessage(WeaponProficiencyDebugBreakdown()));
    }

    private string WeaponProficiencyDebugBreakdown()
    {
        int initial = _initialCharacteristics.WeaponProficiencies.Points;
        int agilityDelta = WeaponProficienciesPointsForAgility(AgilityVm.ItemValue) - WeaponProficienciesPointsForAgility(_initialCharacteristics.Attributes.Agility);
        int wmCurrent = Skills.First(s => s.ItemLabel == "Weapon Master").ItemValue;
        int wmDelta = WeaponProficienciesPointsForWeaponMaster(wmCurrent) - WeaponProficienciesPointsForWeaponMaster(_initialCharacteristics.Skills.WeaponMaster);
        int spent = WeaponProficiencies.Sum(wp => WeaponProficiencyCost(wp.ItemValue) - WeaponProficiencyCost(GetInitialWeaponProficiencyValue(wp.ItemLabel)));
        int remaining = initial + agilityDelta + wmDelta - spent;

        return $"WP initial={initial}, agilityDelta={agilityDelta}, wmDelta={wmDelta}, spent={spent}, remaining={remaining}";
    }

    private int GetInitialSkillValue(string skillName) => skillName switch
    {
        "Iron Flesh" => _initialCharacteristics.Skills.IronFlesh,
        "Power Strike" => _initialCharacteristics.Skills.PowerStrike,
        "Power Draw" => _initialCharacteristics.Skills.PowerDraw,
        "Power Throw" => _initialCharacteristics.Skills.PowerThrow,
        "Athletics" => _initialCharacteristics.Skills.Athletics,
        "Riding" => _initialCharacteristics.Skills.Riding,
        "Weapon Master" => _initialCharacteristics.Skills.WeaponMaster,
        "Mounted Archery" => _initialCharacteristics.Skills.MountedArchery,
        "Shield" => _initialCharacteristics.Skills.Shield,
        _ => 0,
    };

    private int GetInitialWeaponProficiencyValue(string wpName) => wpName switch
    {
        "One-Handed" => _initialCharacteristics.WeaponProficiencies.OneHanded,
        "Two-Handed" => _initialCharacteristics.WeaponProficiencies.TwoHanded,
        "Polearm" => _initialCharacteristics.WeaponProficiencies.Polearm,
        "Bow" => _initialCharacteristics.WeaponProficiencies.Bow,
        "Crossbow" => _initialCharacteristics.WeaponProficiencies.Crossbow,
        "Throwing" => _initialCharacteristics.WeaponProficiencies.Throwing,
        _ => 0,
    };

    private int WeaponProficiencyPointsRemaining
    {
        get
        {
            // initial base points
            int initialPoints = _initialCharacteristics.WeaponProficiencies.Points;

            // agility contribution delta (current - initial)
            int agilityDelta = WeaponProficienciesPointsForAgility(AgilityVm.ItemValue)
                              - WeaponProficienciesPointsForAgility(_initialCharacteristics.Attributes.Agility);

            // weapon-master skill contribution delta (current - initial)
            var weaponMasterVm = Skills.FirstOrDefault(s => s.ItemLabel == "Weapon Master");
            int weaponMasterCurrent = weaponMasterVm?.ItemValue ?? _initialCharacteristics.Skills.WeaponMaster;
            int weaponMasterInitial = _initialCharacteristics.Skills.WeaponMaster;
            int weaponMasterDelta = WeaponProficienciesPointsForWeaponMaster(weaponMasterCurrent)
                                    - WeaponProficienciesPointsForWeaponMaster(weaponMasterInitial);

            // cost already spent by weapon proficiencies (current - initial)
            int spentOnWPs = WeaponProficiencies.Sum(wp =>
                WeaponProficiencyCost(wp.ItemValue) - WeaponProficiencyCost(GetInitialWeaponProficiencyValue(wp.ItemLabel)));

            // total remaining = initial + deltas - spent
            return initialPoints + agilityDelta + weaponMasterDelta - spentOnWPs;
        }
    }

    private int WeaponProficienciesPointsForAgility(int agility) => agility * _constants.WeaponProficiencyPointsForAgility;

    private int WeaponProficienciesPointsForWeaponMaster(int weaponMaster) =>
        (int)MathHelper.ApplyPolynomialFunction(weaponMaster, _constants.WeaponProficiencyPointsForWeaponMasterCoefs);

    private int WeaponProficiencyCost(int wpf) =>
        (int)MathHelper.ApplyPolynomialFunction(wpf, _constants.WeaponProficiencyCostCoefs);

    private int WeaponProficienciesPointsForLevel(int level) =>
        (int)MathHelper.ApplyPolynomialFunction(level, _constants.WeaponProficiencyPointsForLevelCoefs);

    private bool CheckSkillRequirement(string skillName, int level) => skillName switch
    {
        "Iron Flesh" => level <= StrengthVm.ItemValue / 3,
        "Power Strike" => level <= StrengthVm.ItemValue / 3,
        "Power Draw" => level <= StrengthVm.ItemValue / 3,
        "Power Throw" => level <= StrengthVm.ItemValue / 6,
        "Athletics" => level <= AgilityVm.ItemValue / 3,
        "Riding" => level <= AgilityVm.ItemValue / 3,
        "Weapon Master" => level <= AgilityVm.ItemValue / 3,
        "Mounted Archery" => level <= AgilityVm.ItemValue / 6,
        "Shield" => level <= AgilityVm.ItemValue / 6,
        _ => true,
    };

    private void ValidateAndUpdateAllSkills()
    {
        foreach (var skill in Skills)
        {
            if (skill == null)
            {
                continue;
            }

            bool isValid = CheckSkillRequirement(skill.ItemLabel, skill.ItemValue);
            skill.TextStateDisabled = !isValid;
        }
    }

    /// <summary>
    /// Validates that the total spent weapon proficiency points
    /// does not exceed the maximum possible points available
    /// based on Agility + Weapon Master.
    /// </summary>
    private bool ValidateWeaponProficiencyCap()
    {
        int remaining = WeaponProficiencyPointsRemaining;

        // For debugging: show breakdown
        int totalSpent = WeaponProficiencies.Sum(wp => WeaponProficiencyCost(wp.ItemValue));

        // valid if remaining >= 0
        return remaining >= 0;
    }

    [DataSourceProperty]
    public bool IsVisible { get => _isVisible; set => SetField(ref _isVisible, value, nameof(IsVisible)); }

    [DataSourceProperty]
    public CharacteristicsConvertItemVM ConvertAttribute { get => _convertAttributeVm; set => SetField(ref _convertAttributeVm, value, nameof(ConvertAttribute)); }

    [DataSourceProperty]
    public CharacteristicsConvertItemVM ConvertSkill { get => _convertSkillVm; set => SetField(ref _convertSkillVm, value, nameof(ConvertSkill)); }

    [DataSourceProperty]
    public CharacteristicsPlusMinusItemVM StrengthVm { get => _strengthVm; set => SetField(ref _strengthVm, value, nameof(StrengthVm)); }

    [DataSourceProperty]
    public CharacteristicsPlusMinusItemVM AgilityVm { get => _agilityVm; set => SetField(ref _agilityVm, value, nameof(AgilityVm)); }

    [DataSourceProperty]
    public MBBindingList<CharacteristicsPlusMinusItemVM> Skills { get => _skills; set => SetField(ref _skills, value, nameof(Skills)); }

    [DataSourceProperty]
    public MBBindingList<CharacteristicsPlusMinusItemVM> WeaponProficiencies { get => _weaponProficiencies; set => SetField(ref _weaponProficiencies, value, nameof(WeaponProficiencies)); }

    [DataSourceProperty]
    public UserAndCharacterInfoVM UserAndCharacterInfoVm { get => _userAndCharacterInfoVm; set => SetField(ref _userAndCharacterInfoVm, value, nameof(UserAndCharacterInfoVm)); }

    [DataSourceProperty]
    public int AttributePoints
    {
        get => _attributePoints;
        set
        {
            if (SetField(ref _attributePoints, value, nameof(AttributePoints)))
            {
                ConvertAttribute.ItemValue = value;
                UpdateAllButtonStates();
            }
        }
    }

    [DataSourceProperty]
    public int SkillPoints
    {
        get => _skillPoints;
        set
        {
            if (SetField(ref _skillPoints, value, nameof(SkillPoints)))
            {
                ConvertSkill.ItemValue = value;
                UpdateAllButtonStates();
            }
        }
    }

    [DataSourceProperty]
    public int WeaponProficiencyPoints
    {
        get => _weaponProficiencyPoints;
        set
        {
            if (SetField(ref _weaponProficiencyPoints, value, nameof(WeaponProficiencyPoints)))
            {
                UpdateAllButtonStates();
            }
        }
    }
}
