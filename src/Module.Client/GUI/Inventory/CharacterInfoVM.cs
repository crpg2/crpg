using System.Collections.Generic;
using Crpg.Module.Api.Models.Characters;
using Crpg.Module.Common;
using Crpg.Module.Helpers;
using Messages.FromClient.ToLobbyServer;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.GUI.Inventory;

public class CharacterInfoVM : ViewModel
{
    private readonly CrpgConstants _constants = default!;
    private string _characterNameText = string.Empty;
    private int _characterGeneration;
    private int _characterLevel;
    private string _characterExperienceText = string.Empty;
    private string _characterClassText = string.Empty;

    private MBBindingList<CharacterInfoPlusMinusItemVM> _weaponProficiencies = new();
    private MBBindingList<CharacterInfoPlusMinusItemVM> _skills = new();
    private CharacterInfoPlusMinusItemVM _strengthVm;
    private CharacterInfoPlusMinusItemVM _agilityVm;
    private CharacterInfoConvertItemVM _convertAttributeVm;
    private CharacterInfoConvertItemVM _convertSkillVm;

    private CrpgCharacterCharacteristics _initialCharacteristics = new();

    private int _skillPoints;
    private int _attributePoints;
    private int _strength;
    private int _agility;
    private int _weaponProficiencyPoints;

    private CrpgCharacter _crpgCharacterBasic = new();

    public CharacterInfoVM()
    {
        _characterNameText = string.Empty;
        _characterLevel = 0;
        _characterGeneration = 0;
        _characterExperienceText = string.Empty;
        _characterClassText = string.Empty;

        // Initialize attribute VMs
        _strengthVm = new CharacterInfoPlusMinusItemVM("Strength", 0, 0, 50);
        _agilityVm = new CharacterInfoPlusMinusItemVM("Agility", 0, 0, 50);

        // skillpoints
        _skillPoints = 0;

        // Convert buttons
        _convertAttributeVm = new CharacterInfoConvertItemVM("Attributes", 0, true);
        _convertSkillVm = new CharacterInfoConvertItemVM("Skills", 0, true);

        // Wpf points
        _weaponProficiencyPoints = 0;

        // Wpf PlusMinus
        _weaponProficiencies.Add(new CharacterInfoPlusMinusItemVM("One-Handed", 0, 0, 300));
        _weaponProficiencies.Add(new CharacterInfoPlusMinusItemVM("Two-Handed", 0, 0, 300));
        _weaponProficiencies.Add(new CharacterInfoPlusMinusItemVM("Polearm", 0, 0, 300));
        _weaponProficiencies.Add(new CharacterInfoPlusMinusItemVM("Crossbow", 0, 0, 300));
        _weaponProficiencies.Add(new CharacterInfoPlusMinusItemVM("Throwing", 0, 0, 300));

        // Skill PlusMinus
        _skills.Add(new CharacterInfoPlusMinusItemVM("Iron Flesh", 0, 0, 20));
        _skills.Add(new CharacterInfoPlusMinusItemVM("Power Strike", 0, 0, 20));
        _skills.Add(new CharacterInfoPlusMinusItemVM("Power Draw", 0, 0, 20));
        _skills.Add(new CharacterInfoPlusMinusItemVM("Power Throw", 0, 0, 20));
        _skills.Add(new CharacterInfoPlusMinusItemVM("Athletics", 0, 0, 20));
        _skills.Add(new CharacterInfoPlusMinusItemVM("Riding", 0, 0, 20));
        _skills.Add(new CharacterInfoPlusMinusItemVM("Weapon Master", 0, 0, 20));
        _skills.Add(new CharacterInfoPlusMinusItemVM("Mounted Archery", 0, 0, 20));
        _skills.Add(new CharacterInfoPlusMinusItemVM("Shield", 0, 0, 20));

        // Subscribe clicks
        SubscribeAllEvents();

        var behavior = Mission.Current.GetMissionBehavior<CrpgCharacterLoadoutBehaviorClient>()
            ?? throw new InvalidOperationException("CrpgCharacterLoadoutBehaviorClient is required");
        _constants = behavior.Constants;

        // UpdateAllButtonStates();
    }

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

    private void SubscribeAllEvents()
    {
        // Clean up before re-subscribing
        UnsubscribeAllEvents();

        if (ConvertAttribute != null)
            ConvertAttribute.OnConvertClickedEvent += OnConvertClicked;

        if (ConvertSkill != null)
            ConvertSkill.OnConvertClickedEvent += OnConvertClicked;

        StrengthVm.OnPlusClickedEvent += OnPlusClicked;
        StrengthVm.OnMinusClickedEvent += OnMinusClicked;
        AgilityVm.OnPlusClickedEvent += OnPlusClicked;
        AgilityVm.OnMinusClickedEvent += OnMinusClicked;

        foreach (var skill in Skills)
        {
            skill.OnPlusClickedEvent += OnPlusClicked;
            skill.OnMinusClickedEvent += OnMinusClicked;
        }

        foreach (var wp in WeaponProficiencies)
        {
            wp.OnPlusClickedEvent += OnPlusClicked;
            wp.OnMinusClickedEvent += OnMinusClicked;
        }
    }

    private void UnsubscribeAllEvents()
    {
        if (ConvertAttribute != null)
            ConvertAttribute.OnConvertClickedEvent -= OnConvertClicked;

        if (ConvertSkill != null)
            ConvertSkill.OnConvertClickedEvent -= OnConvertClicked;

        StrengthVm.OnPlusClickedEvent -= OnPlusClicked;
        StrengthVm.OnMinusClickedEvent -= OnMinusClicked;
        AgilityVm.OnPlusClickedEvent -= OnPlusClicked;
        AgilityVm.OnMinusClickedEvent -= OnMinusClicked;

        foreach (var skill in Skills)
        {
            skill.OnPlusClickedEvent -= OnPlusClicked;
            skill.OnMinusClickedEvent -= OnMinusClicked;
        }

        foreach (var wp in WeaponProficiencies)
        {
            wp.OnPlusClickedEvent -= OnPlusClicked;
            wp.OnMinusClickedEvent -= OnMinusClicked;
        }
    }

    internal void SetCrpgCharacterBasic(CrpgCharacter newCharacter)
    {
        _crpgCharacterBasic = newCharacter;
        CharacterNameText = newCharacter.Name;
        CharacterLevel = newCharacter.Level;
        CharacterGeneration = newCharacter.Generation;
        CharacterExperienceText = newCharacter.Experience.ToString();
        CharacterClassText = newCharacter.Class.ToString();

        // Clone characteristics to store initial values for min enforcement
        _initialCharacteristics = CloneCharacteristics(newCharacter.Characteristics);

        // Attributes
        StrengthVm.ItemValue = newCharacter.Characteristics.Attributes.Strength;
        AgilityVm.ItemValue = newCharacter.Characteristics.Attributes.Agility;
        AttributePoints = newCharacter.Characteristics.Attributes.Points;

        // Skills
        SkillPoints = newCharacter.Characteristics.Skills.Points;

        // Update ConvertItemVMs
        ConvertAttribute.ItemValue = AttributePoints;
        ConvertSkill.ItemValue = SkillPoints;

        Skills.Clear();
        var sk = newCharacter.Characteristics.Skills;
        Skills.Add(new CharacterInfoPlusMinusItemVM("Iron Flesh", sk.IronFlesh, sk.IronFlesh, 20));
        Skills.Add(new CharacterInfoPlusMinusItemVM("Power Strike", sk.PowerStrike, sk.PowerStrike, 20));
        Skills.Add(new CharacterInfoPlusMinusItemVM("Power Draw", sk.PowerDraw, sk.PowerDraw, 20));
        Skills.Add(new CharacterInfoPlusMinusItemVM("Power Throw", sk.PowerThrow, sk.PowerThrow, 20));
        Skills.Add(new CharacterInfoPlusMinusItemVM("Athletics", sk.Athletics, sk.Athletics, 20));
        Skills.Add(new CharacterInfoPlusMinusItemVM("Riding", sk.Riding, sk.Riding, 20));
        Skills.Add(new CharacterInfoPlusMinusItemVM("Weapon Master", sk.WeaponMaster, sk.WeaponMaster, 20));
        Skills.Add(new CharacterInfoPlusMinusItemVM("Mounted Archery", sk.MountedArchery, sk.MountedArchery, 20));
        Skills.Add(new CharacterInfoPlusMinusItemVM("Shield", sk.Shield, sk.Shield, 20));

        // Weapon proficiencies
        WeaponProficiencyPoints = newCharacter.Characteristics.WeaponProficiencies.Points;
        WeaponProficiencies.Clear();
        var wp = newCharacter.Characteristics.WeaponProficiencies;
        WeaponProficiencies.Add(new CharacterInfoPlusMinusItemVM("One-Handed", wp.OneHanded, wp.OneHanded, 300));
        WeaponProficiencies.Add(new CharacterInfoPlusMinusItemVM("Two-Handed", wp.TwoHanded, wp.TwoHanded, 300));
        WeaponProficiencies.Add(new CharacterInfoPlusMinusItemVM("Polearm", wp.Polearm, wp.Polearm, 300));
        WeaponProficiencies.Add(new CharacterInfoPlusMinusItemVM("Bow", wp.Bow, wp.Bow, 300));
        WeaponProficiencies.Add(new CharacterInfoPlusMinusItemVM("Crossbow", wp.Crossbow, wp.Crossbow, 300));
        WeaponProficiencies.Add(new CharacterInfoPlusMinusItemVM("Throwing", wp.Throwing, wp.Throwing, 300));

        // Subscribe events after adding all items
        SubscribeAllEvents();

        // Update button states for min/max enforcement
        UpdateAllButtonStates();
    }

    private void OnConvertClicked(CharacterInfoConvertItemVM vm)
    {
        InformationManager.DisplayMessage(new InformationMessage($"OnConvertClicked: {vm.ItemLabel} : {vm.ItemValue}"));
        // convert attribute/skill points
        if (vm.ItemLabel == "Attributes")
        {
            // 1 attribute point = 2 skill points
            if (AttributePoints >= 1)
            {
                AttributePoints--;
                SkillPoints += 2;
            }
        }
        else if (vm.ItemLabel == "Skills")
        {
            // 2 skill points = 1 attribute point
            if (SkillPoints >= 2)
            {
                SkillPoints -= 2;
                AttributePoints++;
            }
        }

        // EnforceSkillRequirements();
        UpdateAllSkillTextStates();
        UpdateWeaponProficiencyTextStates();
        UpdateAllButtonStates();
    }

    private void OnPlusClicked(CharacterInfoPlusMinusItemVM item)
    {
        if (!item.IsButtonPlusEnabled)
        {
            return;
        }

        if (item == StrengthVm)
        {
            if (AttributePoints > 0)
            {
                item.ItemValue++;
                AttributePoints--;
                // ConvertAttribute.ItemValue = AttributePoints;
            }
        }
        else if (item == AgilityVm)
        {
            if (AttributePoints > 0)
            {
                item.ItemValue++;
                AttributePoints--;
                // ConvertAttribute.ItemValue = AttributePoints;
                WeaponProficiencyPoints += WeaponProficienciesPointsForAgility(item.ItemValue)
                                          - WeaponProficienciesPointsForAgility(item.ItemValue - 1);
            }
        }
        else if (Skills.Contains(item))
        {
            if (SkillPoints > 0 && CheckSkillRequirement(item.ItemLabel, item.ItemValue + 1))
            {
                item.ItemValue++;
                SkillPoints--;
                // ConvertSkill.ItemValue = SkillPoints;
                if (item.ItemLabel == "Weapon Master")
                {
                    WeaponProficiencyPoints += WeaponProficienciesPointsForWeaponMaster(item.ItemValue)
                                              - WeaponProficienciesPointsForWeaponMaster(item.ItemValue - 1);
                }
            }
        }
        else if (WeaponProficiencies.Contains(item))
        {
            int cost = WeaponProficiencyCost(item.ItemValue + 1) - WeaponProficiencyCost(item.ItemValue);
            if (WeaponProficiencyPoints >= cost)
            {
                item.ItemValue++;
                WeaponProficiencyPoints -= cost;
            }
        }

        UpdateAllSkillTextStates();
        UpdateWeaponProficiencyTextStates();
        UpdateAllButtonStates();
    }

    private void OnMinusClicked(CharacterInfoPlusMinusItemVM item)
    {
        if (!item.IsButtonMinusEnabled)
        {
            return;
        }

        if (item == StrengthVm)
        {
            if (item.ItemValue > _initialCharacteristics.Attributes.Strength && item.ItemValue > 0)
            {
                item.ItemValue--;
                AttributePoints++;
            }
        }
        else if (item == AgilityVm)
        {
            if (item.ItemValue > _initialCharacteristics.Attributes.Agility)
            {
                int oldValue = item.ItemValue;
                item.ItemValue--;
                AttributePoints++;
                WeaponProficiencyPoints -= WeaponProficienciesPointsForAgility(oldValue)
                                          - WeaponProficienciesPointsForAgility(item.ItemValue);
            }
        }
        else if (Skills.Contains(item))
        {
            int minValue = GetInitialSkillValue(item.ItemLabel);
            if (item.ItemValue > minValue)
            {
                int oldValue = item.ItemValue;
                item.ItemValue--;
                SkillPoints++;
                if (item.ItemLabel == "Weapon Master")
                {
                    WeaponProficiencyPoints -= WeaponProficienciesPointsForWeaponMaster(oldValue)
                                              - WeaponProficienciesPointsForWeaponMaster(item.ItemValue);
                }
            }
        }
        else if (WeaponProficiencies.Contains(item))
        {
            int minValue = GetInitialWeaponProficiencyValue(item.ItemLabel);
            if (item.ItemValue > minValue)
            {
                int oldValue = item.ItemValue;
                item.ItemValue--;
                WeaponProficiencyPoints += WeaponProficiencyCost(oldValue) - WeaponProficiencyCost(item.ItemValue);
            }
        }
        UpdateAllSkillTextStates();
        UpdateWeaponProficiencyTextStates();
        UpdateAllButtonStates();
    }

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
                                        CheckSkillRequirement(skill.ItemLabel, skill.ItemValue + 1) &&
                                        skill.ItemValue < 20;
        }

        // Weapon proficiencies
        foreach (var wp in WeaponProficiencies)
        {
            int minValue = GetInitialWeaponProficiencyValue(wp.ItemLabel);
            int nextCost = WeaponProficiencyCost(wp.ItemValue + 1) - WeaponProficiencyCost(wp.ItemValue);

            // Plus enabled only if we have enough scaled points
            wp.IsButtonPlusEnabled = wp.ItemValue < 300 && WeaponProficiencyPoints >= nextCost;
            wp.IsButtonMinusEnabled = wp.ItemValue > minValue;
        }
    }

    // Helper methods for min values
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

    // Matching methods
    private int WeaponProficienciesPointsForAgility(int agility) =>
        agility * _constants.WeaponProficiencyPointsForAgility;

    private int WeaponProficienciesPointsForWeaponMaster(int weaponMaster) =>
        (int)MathHelper.ApplyPolynomialFunction(weaponMaster, _constants.WeaponProficiencyPointsForWeaponMasterCoefs);

    private int WeaponProficiencyCost(int wpf) =>
        (int)MathHelper.ApplyPolynomialFunction(wpf, _constants.WeaponProficiencyCostCoefs);

    /// <summary>
    /// Polynomial function for WPF points gained from level progression.
    /// </summary>
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

    /// <summary>
    /// Validates whether the given skill's current value satisfies its attribute requirements.
    /// </summary>
    private bool ValidateSkillRequirements(CharacterInfoPlusMinusItemVM vm)
    {
        if (vm == null)
        {
            return true;
        }

        return CheckSkillRequirement(vm.ItemLabel, vm.ItemValue);
    }

    /// <summary>
    /// Updates the TextState of the given skill VM based on whether it satisfies requirements.
    /// </summary>
    private void UpdateSkillTextState(CharacterInfoPlusMinusItemVM vm)
    {
        if (vm == null)
        {
            return;
        }

        vm.TextStateDisabled = ValidateSkillRequirements(vm) ? false : true;
    }

    /// <summary>
    /// Updates all skills' TextStates to reflect whether they are valid.
    /// </summary>
    private void UpdateAllSkillTextStates()
    {
        foreach (var skill in Skills)
        {
            UpdateSkillTextState(skill);
        }
    }

    private void UpdateWeaponProficiencyTextStates()
    {
        bool isValidWpf = ValidateWeaponProficiencyCap();

        InformationManager.DisplayMessage(new InformationMessage($"UpdateWeaponProficiencyTextStates: isValidWpf{isValidWpf}"));
        foreach (var wp in WeaponProficiencies)
        {
            wp.TextStateDisabled = !isValidWpf;
        }
    }

    /// <summary>
    /// Validates that the total spent weapon proficiency points
    /// does not exceed the maximum possible points available
    /// based on Agility + Weapon Master + Level.
    /// </summary>

    private bool ValidateWeaponProficiencyCap()
    {
        int lvlPoints = WeaponProficienciesPointsForLevel(CharacterLevel);

        int agiAllocated = AgilityVm.ItemValue - _initialCharacteristics.Attributes.Agility;
        int agiPoints = WeaponProficienciesPointsForAgility(agiAllocated);

        int wmAllocated = (Skills.FirstOrDefault(s => s.ItemLabel == "Weapon Master")?.ItemValue ?? 0)
                          - _initialCharacteristics.Skills.WeaponMaster;
        int wmPoints = WeaponProficienciesPointsForWeaponMaster(wmAllocated);

        int maxPoints = lvlPoints + agiPoints + wmPoints;

        int totalSpent = WeaponProficiencies.Sum(wp => WeaponProficiencyCost(wp.ItemValue));

        InformationManager.DisplayMessage(new InformationMessage(
            $"WPF Cap -> lvl:{lvlPoints}, agi:{agiPoints}, wm:{wmPoints}, max:{maxPoints}, spent:{totalSpent}"));

        return totalSpent <= maxPoints;
    }
    private bool ValidateWeaponProficiencyCapOld()
    {
        // 1. Calculate the theoretical max points for this character
        int lvlPoints = WeaponProficienciesPointsForLevel(CharacterLevel);
        int agiPoints = WeaponProficienciesPointsForAgility(AgilityVm.ItemValue);
        int wmPoints = WeaponProficienciesPointsForWeaponMaster(
                Skills.FirstOrDefault(s => s.ItemLabel == "Weapon Master")?.ItemValue ?? 0);

        int maxPoints = lvlPoints + agiPoints + wmPoints;

        // 2. Calculate how much has been spent across all proficiencies
        int totalSpent = WeaponProficiencies.Sum(wp => WeaponProficiencyCost(wp.ItemValue));

        InformationManager.DisplayMessage(new InformationMessage($"UpdateWeaponProficiencyTextStates: lvl: {lvlPoints} agi: {agiPoints} wm: {wmPoints} max: {maxPoints} spent: {totalSpent}"));

        // 3. Compare
        return totalSpent <= maxPoints;
    }

    [DataSourceProperty]
    public CharacterInfoConvertItemVM ConvertAttribute
    {
        get => _convertAttributeVm;
        set => SetField(ref _convertAttributeVm, value, nameof(ConvertAttribute));
    }

    [DataSourceProperty]
    public CharacterInfoConvertItemVM ConvertSkill
    {
        get => _convertSkillVm;
        set => SetField(ref _convertSkillVm, value, nameof(ConvertSkill));
    }

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

    [DataSourceProperty]
    public CharacterInfoPlusMinusItemVM StrengthVm { get => _strengthVm; set => SetField(ref _strengthVm, value, nameof(StrengthVm)); }
    [DataSourceProperty]
    public CharacterInfoPlusMinusItemVM AgilityVm { get => _agilityVm; set => SetField(ref _agilityVm, value, nameof(AgilityVm)); }
    [DataSourceProperty]
    public MBBindingList<CharacterInfoPlusMinusItemVM> Skills { get => _skills; set => SetField(ref _skills, value, nameof(Skills)); }
    [DataSourceProperty]
    public MBBindingList<CharacterInfoPlusMinusItemVM> WeaponProficiencies { get => _weaponProficiencies; set => SetField(ref _weaponProficiencies, value, nameof(WeaponProficiencies)); }
    [DataSourceProperty]
    public string CharacterNameText { get => _characterNameText; set => SetField(ref _characterNameText, value, nameof(CharacterNameText)); }
    [DataSourceProperty]
    public int CharacterLevel { get => _characterLevel; set => SetField(ref _characterLevel, value, nameof(CharacterLevel)); }
    [DataSourceProperty]
    public int CharacterGeneration { get => _characterGeneration; set => SetField(ref _characterGeneration, value, nameof(CharacterGeneration)); }
    [DataSourceProperty]
    public string CharacterExperienceText { get => _characterExperienceText; set => SetField(ref _characterExperienceText, value, nameof(CharacterExperienceText)); }
    [DataSourceProperty]
    public string CharacterClassText { get => _characterClassText; set => SetField(ref _characterClassText, value, nameof(CharacterClassText)); }
}
