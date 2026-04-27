using Crpg.Module.Api.Models;
using Crpg.Module.Api.Models.Characters;
using Crpg.Module.Common;
using Crpg.Module.Helpers;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.GUI.Inventory;

public class CharacteristicsEditorVM : ViewModel
{
    private readonly CrpgConstants _constants = default!;
    private readonly CrpgCharacterLoadoutBehaviorClient _userLoadout;
    private readonly Dictionary<string, CharacteristicsPlusMinusItemVM> _skillMap = new();
    private readonly Dictionary<string, CharacteristicsPlusMinusItemVM> _weaponMap = new();
    private readonly TimeSpan _apiUsageClickCooldown = TimeSpan.FromSeconds(5);

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
        _userLoadout = Mission.Current?.GetMissionBehavior<CrpgCharacterLoadoutBehaviorClient>()
            ?? throw new InvalidOperationException("CrpgCharacterLoadoutBehaviorClient is required");
        _constants = CrpgCharacterEquipUiHandler.Constants;

        // Initialize attribute VMs
        StrengthVm = new CharacteristicsPlusMinusItemVM("Strength", new TextObject("{=KC9dxc84}Strength").ToString(), 0,
            new TextObject("{=KC9dxc85}Strength \nIncreases your health by 1\nRequires: 1 attribute point").ToString());

        AgilityVm = new CharacteristicsPlusMinusItemVM("Agility", new TextObject("{=KC9dxc86}Agility").ToString(), 0,
            new TextObject("{=KC9dxc87}Agility \nIncreases your weapon points and makes you move a bit faster\nRequires: 1 attribute point").ToString());

        // skillpoints
        SkillPoints = 0;

        // Convert buttons
        ConvertAttribute = new CharacteristicsConvertItemVM(
            new TextObject("{=KC9dxc88}Attributes - ").ToString(), 0, true,
            new TextObject("{=KC9dxc89}Convert 1 Attribute point into 2 Skill Points").ToString());

        ConvertSkill = new CharacteristicsConvertItemVM(
            new TextObject("{=KC9dxc90}Skills - ").ToString(), 0, true,
            new TextObject("{=KC9dxc91}Convert 2 Skill points into 1 Attribute point.").ToString());

        // Wpf points
        WeaponProficiencyPoints = 0;

        // Wpf PlusMinus
        WeaponProficiencies.Add(new CharacteristicsPlusMinusItemVM("One-Handed", new TextObject("{=KC9dxc92}One-Handed").ToString(), 0));
        WeaponProficiencies.Add(new CharacteristicsPlusMinusItemVM("Two-Handed", new TextObject("{=KC9dxc93}Two-Handed").ToString(), 0));
        WeaponProficiencies.Add(new CharacteristicsPlusMinusItemVM("Polearm", new TextObject("{=KC9dxc94}Polearm").ToString(), 0));
        WeaponProficiencies.Add(new CharacteristicsPlusMinusItemVM("Bow", new TextObject("{=KC9dxc95}Bow").ToString(), 0));
        WeaponProficiencies.Add(new CharacteristicsPlusMinusItemVM("Crossbow", new TextObject("{=KC9dxc96}Crossbow").ToString(), 0));
        WeaponProficiencies.Add(new CharacteristicsPlusMinusItemVM("Throwing", new TextObject("{=KC9dxc97}Throwing").ToString(), 0));

        // Skill PlusMinus
        Skills.Add(new CharacteristicsPlusMinusItemVM("Iron Flesh", new TextObject("{=KC9dxc98}Iron Flesh").ToString(), 0,
            new TextObject("{=KC9dxc99}Iron flesh\nIncreases your health by {HEALTH_PER_LEVEL} per level\nRequires: 3 STR per level")
            .SetTextVariable("HEALTH_PER_LEVEL", _constants?.HealthPointsForIronFlesh ?? 0).ToString()));

        Skills.Add(new CharacteristicsPlusMinusItemVM("Power Strike", new TextObject("{=KC9dx100}Power Strike").ToString(), 0,
            new TextObject("{=KC9dx101}Power strike\nIncreases your damage {DAMAGE_FACTOR}% with melee weapons\nRequires: 3 STR per level")
            .SetTextVariable("DAMAGE_FACTOR", _constants?.DamageFactorForPowerStrike ?? 0).ToString()));

        Skills.Add(new CharacteristicsPlusMinusItemVM("Power Draw", new TextObject("{=KC9dx102}Power Draw").ToString(), 0,
            new TextObject("{=KC9dx103}Power draw\nIncreases damage by {DAMAGE_FACTOR}% and your steadiness with bows\nRequires: 3 STR per level")
            .SetTextVariable("DAMAGE_FACTOR", _constants?.DamageFactorForPowerDraw ?? 0).ToString()));

        Skills.Add(new CharacteristicsPlusMinusItemVM("Power Throw", new TextObject("{=KC9dx104}Power Throw").ToString(), 0,
            new TextObject("{=KC9dx105}Power throw\nIncreases damage by {DAMAGE_FACTOR}% and your steadiness with throwing weapons\nRequires: 6 STR per level")
            .SetTextVariable("DAMAGE_FACTOR", _constants?.DamageFactorForPowerThrow ?? 0).ToString()));

        Skills.Add(new CharacteristicsPlusMinusItemVM("Athletics", new TextObject("{=KC9dx106}Athletics").ToString(), 0,
            new TextObject("{=KC9dx107}Athletics\nIncreases your maximum running speed and decreases the time it takes to reach it\nRequires: 3 AGI per level").ToString()));

        Skills.Add(new CharacteristicsPlusMinusItemVM("Riding", new TextObject("{=KC9dx108}Riding").ToString(), 0,
            new TextObject("{=KC9dx109}Riding\nIncreases riding speed, acceleration, maneuver, and dismount resistance\nRequires: 3 AGI per level").ToString()));

        Skills.Add(new CharacteristicsPlusMinusItemVM("Weapon Master", new TextObject("{=KC9dx110}Weapon Master").ToString(), 0,
            new TextObject("{=KC9dx111}Weapon master\nGives weapon proficiency points\nRequires: 3 AGI per level").ToString()));

        Skills.Add(new CharacteristicsPlusMinusItemVM("Mounted Archery", new TextObject("{=KC9dx112}Mounted Archery").ToString(), 0,
            new TextObject("{=KC9dx113}Mounted archery\nReduces penalty for using ranged weapons on a moving mount\nRequires: 6 AGI per level").ToString()));

        Skills.Add(new CharacteristicsPlusMinusItemVM("Shield", new TextObject("{=KC9dx114}Shield").ToString(), 0,
            new TextObject("{=KC9dx115}Shield\nImproves shield durability. Increases coverage from ranged attacks. Reduces stun from weapons when hit\nRequires: 6 AGI per level").ToString()));

        // Subscribe clicks
        WireAllEvents(true);

        // Build the dictionaries after adding items to lists
        foreach (var skill in Skills)
        {
            _skillMap[skill.ItemKey] = skill;
        }

        foreach (var wp in WeaponProficiencies)
        {
            _weaponMap[wp.ItemKey] = wp;
        }
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

        // Clone characteristics for min value enforcement
        _initialCharacteristics = CloneCharacteristics(newCharacter.Characteristics);

        // Attributes
        StrengthVm.ItemValue = newCharacter.Characteristics.Attributes.Strength;
        AgilityVm.ItemValue = newCharacter.Characteristics.Attributes.Agility;
        AttributePoints = newCharacter.Characteristics.Attributes.Points;

        // Skill points
        SkillPoints = newCharacter.Characteristics.Skills.Points;
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
        bool gainedAttribute = attributePoints > _initialCharacteristics.Attributes.Points;

        // update initial first
        _initialCharacteristics.Attributes.Points = attributePoints;
        _initialCharacteristics.Skills.Points = skillPoints;

        AttributePoints = attributePoints;
        SkillPoints = skillPoints;

        // UpdateCharacteristicsPointsAfterConversion
        CrpgCharacterEquipVM.RequestStatusMessage(gainedAttribute
            ? new TextObject("{=KC9dx116}Converted 2 skill points into 1 attribute point.").ToString()
            : new TextObject("{=KC9dx117}Converted 1 attribute point into 2 skill points.").ToString());
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
        // Restore attributes
        StrengthVm.ItemValue = _initialCharacteristics.Attributes.Strength;
        AgilityVm.ItemValue = _initialCharacteristics.Attributes.Agility;
        AttributePoints = _initialCharacteristics.Attributes.Points;

        // Restore skills
        foreach (var skill in Skills)
        {
            skill.ItemValue = GetInitialSkillValue(skill.ItemKey);
        }

        SkillPoints = _initialCharacteristics.Skills.Points;

        // Restore weapon proficiencies
        foreach (var wp in WeaponProficiencies)
        {
            wp.ItemValue = GetInitialWeaponProficiencyValue(wp.ItemKey);
        }

        WeaponProficiencyPoints = _initialCharacteristics.WeaponProficiencies.Points;

        // Optionally, update convert buttons
        // ConvertAttribute.ItemValue = _initialCharacteristics.Attributes.Points;
        // ConvertSkill.ItemValue = _initialCharacteristics.Skills.Points;

        // Refresh button states and text states
        UpdateAllButtonStates();
        ValidateAndUpdateAllSkills();
        UpdateWeaponProficiencyTextStates();

        OnEditCharacteristicsChanged?.Invoke();

        // InformationManager.DisplayMessage(new InformationMessage($"ResetToInitialCharacteristics():  completed.", Colors.Cyan));
    }

    private void ExecuteClickApplyChanges()
    {
        var now = DateTime.UtcNow;
        if (now - _lastApiCharacteristicsApplyClick < _apiUsageClickCooldown)
        {
            SoundEvent.PlaySound2D("event:/ui/notification/alert");
            CrpgCharacterEquipVM.RequestStatusMessage(
                new TextObject("{=KC9dx118}Please wait to send another API request.").ToString(), isError: true);

            return;
        }

        if (!ValidateCharacteristicsBeforeApply(out string errorMessage))
        {
            SoundEvent.PlaySound2D("event:/ui/notification/alert");
            CrpgCharacterEquipVM.RequestStatusMessage(
                new TextObject("{=KC9dx119}Cannot apply: {ERROR_MESSAGE}")
                    .SetTextVariable("ERROR_MESSAGE", errorMessage)
                    .ToString(), isError: true);
            return;
        }

        _lastApiCharacteristicsApplyClick = now;
        _userLoadout?.RequestUpdateCharacterCharacteristics(GetCrpgCharacteristicsFromVM());
        SoundEvent.PlaySound2D("event:/ui/notification/trait_change");
    }

    private void ExecuteClickReset()
    {
        CrpgCharacterEquipVM.RequestStatusMessage(new TextObject("{=KC9dx120}Unapplied changes reset to initial state.").ToString());
        ResetToInitialCharacteristics();
    }

    private void ExecuteClickRefreshApi()
    {
        var now = DateTime.UtcNow;
        if (now - _lastApiCharacteristicsRefreshClick < _apiUsageClickCooldown)
        {
            // too soon display msg or play sound
            CrpgCharacterEquipVM.RequestStatusMessage(new TextObject("{=KC9dx118}Please wait to send another API request.").ToString(), isError: true);
            SoundEvent.PlaySound2D("event:/ui/notification/alert");
            return;
        }

        _lastApiCharacteristicsRefreshClick = now;
        _userLoadout?.RequestGetUpdatedCharacterBasic();
        CrpgCharacterEquipVM.RequestStatusMessage(new TextObject("{=KC9dx121}Updating Characteristics from API.").ToString());
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
        if (subscribe)
        {
            ConvertAttribute.OnConvertClickedEvent += OnConvertClicked;
            ConvertSkill.OnConvertClickedEvent += OnConvertClicked;
        }
        else
        {
            ConvertAttribute.OnConvertClickedEvent -= OnConvertClicked;
            ConvertSkill.OnConvertClickedEvent -= OnConvertClicked;
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
        if (vm.Equals(ConvertAttribute))
        {
            // 1 attribute point = 2 skill points
            if (AttributePoints >= 1)
            {
                _userLoadout?.RequestConvertCharacterCharacteristic(CrpgGameCharacteristicConversionRequest.AttributesToSkills);
            }
        }
        else if (vm.Equals(ConvertSkill))
        {
            // 2 skill points = 1 attribute point
            if (SkillPoints >= 2)
            {
                _userLoadout?.RequestConvertCharacterCharacteristic(CrpgGameCharacteristicConversionRequest.SkillsToAttributes);
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

        if ((item == StrengthVm || item == AgilityVm) && !wasAlternateClick)
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
                    }
                    else
                    {
                        break;
                    }
                }
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
            int minValue = GetInitialSkillValue(item.ItemKey);
            if (item.ItemValue > minValue)
            {
                item.ItemValue--;
                SkillPoints++;
            }
        }
        else if (WeaponProficiencies.Contains(item))
        {
            int minValue = GetInitialWeaponProficiencyValue(item.ItemKey);

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
            int minValue = GetInitialSkillValue(skill.ItemKey);
            skill.IsButtonMinusEnabled = skill.ItemValue > minValue;
            skill.IsButtonPlusEnabled = SkillPoints > 0 &&
                CheckSkillRequirement(skill.ItemKey, skill.ItemValue + 1);
        }

        // Weapon proficiencies
        int remainingWP = WeaponProficiencyPointsRemaining;
        foreach (var wp in WeaponProficiencies)
        {
            int minValue = GetInitialWeaponProficiencyValue(wp.ItemKey);
            int nextCost = WeaponProficiencyCost(wp.ItemValue + 1) - WeaponProficiencyCost(wp.ItemValue);
            wp.IsButtonPlusEnabled = remainingWP >= nextCost;
            wp.IsButtonMinusEnabled = wp.ItemValue > minValue;
        }
    }

    private void UpdateWeaponProficiencyTextStates()
    {
        int rem = WeaponProficiencyPointsRemaining;
        WeaponProficiencyPoints = rem;
        bool isOverCap = rem < 0;

        foreach (var wp in WeaponProficiencies)
        {
            int initialValue = GetInitialWeaponProficiencyValue(wp.ItemKey);
            wp.TextStateDisabled = isOverCap && wp.ItemValue > initialValue;
        }
    }

    private string WeaponProficiencyDebugBreakdown()
    {
        int initial = _initialCharacteristics.WeaponProficiencies.Points;
        int agilityDelta = WeaponProficienciesPointsForAgility(AgilityVm.ItemValue) - WeaponProficienciesPointsForAgility(_initialCharacteristics.Attributes.Agility);
        int wmCurrent = Skills.First(s => s.ItemKey == "Weapon Master").ItemValue;
        int wmDelta = WeaponProficienciesPointsForWeaponMaster(wmCurrent) - WeaponProficienciesPointsForWeaponMaster(_initialCharacteristics.Skills.WeaponMaster);
        int spent = WeaponProficiencies.Sum(wp => WeaponProficiencyCost(wp.ItemValue) - WeaponProficiencyCost(GetInitialWeaponProficiencyValue(wp.ItemKey)));
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
            var weaponMasterVm = Skills.FirstOrDefault(s => s.ItemKey == "Weapon Master");
            int weaponMasterCurrent = weaponMasterVm?.ItemValue ?? _initialCharacteristics.Skills.WeaponMaster;
            int weaponMasterInitial = _initialCharacteristics.Skills.WeaponMaster;
            int weaponMasterDelta = WeaponProficienciesPointsForWeaponMaster(weaponMasterCurrent)
                                    - WeaponProficienciesPointsForWeaponMaster(weaponMasterInitial);

            // cost already spent by weapon proficiencies (current - initial)
            int spentOnWPs = WeaponProficiencies.Sum(wp =>
                WeaponProficiencyCost(wp.ItemValue) - WeaponProficiencyCost(GetInitialWeaponProficiencyValue(wp.ItemKey)));

            // total remaining = initial + deltas - spent
            return initialPoints + agilityDelta + weaponMasterDelta - spentOnWPs;
        }
    }

    private int WeaponProficienciesPointsForAgility(int agility) => agility * _constants.WeaponProficiencyPointsForAgility;

    private int WeaponProficienciesPointsForWeaponMaster(int weaponMaster) =>
        (int)MathHelper.ApplyPolynomialFunction(weaponMaster, _constants.WeaponProficiencyPointsForWeaponMasterCoefs);

    private int WeaponProficiencyCost(int wpf) =>
        (int)MathHelper.ApplyPolynomialFunction(wpf, _constants.WeaponProficiencyCostCoefs);

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
            bool isValid = CheckSkillRequirement(skill.ItemKey, skill.ItemValue);
            skill.TextStateDisabled = !isValid;
        }
    }

    private bool ValidateCharacteristicsBeforeApply(out string errorMessage)
    {
        errorMessage = string.Empty;

        // Check if any changes have been made from initial values
        bool hasChanges =
            StrengthVm.ItemValue != _initialCharacteristics.Attributes.Strength ||
            AgilityVm.ItemValue != _initialCharacteristics.Attributes.Agility ||
            AttributePoints != _initialCharacteristics.Attributes.Points ||
            SkillPoints != _initialCharacteristics.Skills.Points ||
            WeaponProficiencyPointsRemaining != _initialCharacteristics.WeaponProficiencies.Points ||
            Skills.Any(s => s.ItemValue != GetInitialSkillValue(s.ItemKey)) ||
            WeaponProficiencies.Any(wp => wp.ItemValue != GetInitialWeaponProficiencyValue(wp.ItemKey))
;

        if (!hasChanges)
        {
            errorMessage = new TextObject("{=KC9dx122}No changes have been made.").ToString();
            return false;
        }

        // Validate skill requirements
        foreach (var skill in Skills)
        {
            if (!CheckSkillRequirement(skill.ItemKey, skill.ItemValue))
            {
                errorMessage = new TextObject("{=KC9dx123}{SKILL_NAME} requires more STR/AGI than you have allocated.")
                    .SetTextVariable("SKILL_NAME", skill.ItemLabel)
                    .ToString();

                return false;
            }
        }

        // Validate weapon proficiency cap
        if (WeaponProficiencyPointsRemaining < 0)
        {
            errorMessage = new TextObject("{=KC9dx124}Weapon proficiency points exceeded. Remaining: {REMAINING}")
                .SetTextVariable("REMAINING", WeaponProficiencyPointsRemaining)
                .ToString();
            return false;
        }

        // Validate no negative points
        if (AttributePoints < 0)
        {
            errorMessage = new TextObject("{=KC9dx125}Attribute points cannot be negative.").ToString();
            return false;
        }

        if (SkillPoints < 0)
        {
            errorMessage = new TextObject("{=KC9dx126}Skill points cannot be negative.").ToString();
            return false;
        }

        return true;
    }

    [DataSourceProperty]
    public bool IsVisible { get; set => SetField(ref field, value, nameof(IsVisible)); }

    [DataSourceProperty]
    public CharacteristicsConvertItemVM ConvertAttribute { get; set => SetField(ref field, value, nameof(ConvertAttribute)); }

    [DataSourceProperty]
    public CharacteristicsConvertItemVM ConvertSkill { get; set => SetField(ref field, value, nameof(ConvertSkill)); }

    [DataSourceProperty]
    public CharacteristicsPlusMinusItemVM StrengthVm { get; set => SetField(ref field, value, nameof(StrengthVm)); }

    [DataSourceProperty]
    public CharacteristicsPlusMinusItemVM AgilityVm { get; set => SetField(ref field, value, nameof(AgilityVm)); }

    [DataSourceProperty]
    public MBBindingList<CharacteristicsPlusMinusItemVM> Skills { get; set => SetField(ref field, value, nameof(Skills)); } = new();

    [DataSourceProperty]
    public MBBindingList<CharacteristicsPlusMinusItemVM> WeaponProficiencies { get; set => SetField(ref field, value, nameof(WeaponProficiencies)); } = new();

    [DataSourceProperty]
    public int AttributePoints
    {
        get;
        set
        {
            if (SetField(ref field, value, nameof(AttributePoints)))
            {
                ConvertAttribute.ItemValue = value;
                UpdateAllButtonStates();
            }
        }
    }

    [DataSourceProperty]
    public int SkillPoints
    {
        get;
        set
        {
            if (SetField(ref field, value, nameof(SkillPoints)))
            {
                ConvertSkill.ItemValue = value;
                UpdateAllButtonStates();
            }
        }
    }

    [DataSourceProperty]
    public int WeaponProficiencyPoints
    {
        get;
        set
        {
            if (SetField(ref field, value, nameof(WeaponProficiencyPoints)))
            {
                UpdateAllButtonStates();
            }
        }
    }

    [DataSourceProperty]
    public string WeaponProficiencyPointsLabel { get; set => SetField(ref field, value, nameof(WeaponProficiencyPointsLabel)); }
        = new TextObject("{=KC9dxc80}Weapon Proficiencies - ").ToString();
    [DataSourceProperty]
    public string ApiRefreshButtonText { get; set => SetField(ref field, value, nameof(ApiRefreshButtonText)); }
        = new TextObject("{=KC9dxc81}API Refresh").ToString();
    [DataSourceProperty]
    public string ApplyChangesButtonText { get; set => SetField(ref field, value, nameof(ApplyChangesButtonText)); }
        = new TextObject("{=KC9dxc82}Apply Changes").ToString();
    [DataSourceProperty]
    public string ResetChangesButtonText { get; set => SetField(ref field, value, nameof(ResetChangesButtonText)); }
        = new TextObject("{=KC9dxc83}Reset Changes").ToString();
}
