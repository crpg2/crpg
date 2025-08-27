using System.Collections.Generic;
using Crpg.Module.Api.Models.Characters;
using TaleWorlds.Library;

namespace Crpg.Module.GUI.Inventory;

public class CharacterInfoVM : ViewModel
{
    private string _characterNameText = string.Empty;
    private int _characterGeneration;
    private int _characterLevel;
    private string _characterExperienceText = string.Empty;
    private string _characterClassText = string.Empty;

    private MBBindingList<CharacterInfoPlusMinusItemVM> _weaponProficiencies = new();
    private MBBindingList<CharacterInfoPlusMinusItemVM> _skills = new();
    private CrpgCharacter _crpgCharacterBasic = new();
    private CharacterInfoConvertItemVM _convertAttribute;
    private CharacterInfoConvertItemVM _convertSkill;
    private CharacterInfoPlusMinusItemVM _strengthVm;
    private CharacterInfoPlusMinusItemVM _agilityVm;

    // NEW backing fields
    private int _skillPoints;
    private int _attributePoints;
    private int _strength;
    private int _agility;

    public CharacterInfoVM()
    {
        // Example: init from some character model
        CharacterNameText = "Dummy Name"; // string.Empty;
        CharacterGeneration = 3;
        CharacterLevel = 32;
        CharacterExperienceText = "9000";
        CharacterClassText = "Test class";

        WeaponProficiencies = new MBBindingList<CharacterInfoPlusMinusItemVM>
            {
                new("One-Handed", 60, 0, 300),
                new("Two-Handed", 40, 0, 300),
                new("Polearm", 20, 0, 300),
                new("Bow", 10, 0, 300),
                new("Crossbow", 0, 0, 300),
                new("Throwing", 5, 0, 300),
            };

        Skills = new MBBindingList<CharacterInfoPlusMinusItemVM>
            {
                new("Iron Flesh", 3, 0, 10),
                new("Power Strike", 4, 0, 10),
                new("Power Draw", 2, 0, 10),
                new("Power Throw", 1, 0, 10),
                new("Athletics", 5, 0, 10),
                new("Riding", 6, 0, 10),
                new("Weapon Master", 2, 0, 10),
                new("Mounted Archery", 0, 0, 10),
                new("Shield", 3, 0, 10),
            };

        _convertAttribute = new CharacterInfoConvertItemVM("Attributes", 0, true);
        _convertSkill = new CharacterInfoConvertItemVM("Skills", 0, true);
        _strengthVm = new CharacterInfoPlusMinusItemVM("Strength", 0, 0, 50);
        _agilityVm = new CharacterInfoPlusMinusItemVM("Agility", 0, 0, 50);
    }

    internal void SetCrpgCharacterBasic(CrpgCharacter newCharacter)
    {
        _crpgCharacterBasic = newCharacter;
        UpdateValuesFromCharacter(newCharacter);
    }

    private void UpdateValuesFromCharacter(CrpgCharacter? character)
    {
        character ??= _crpgCharacterBasic;

        PrintCharacterValues(character);

        CharacterNameText = character.Name;
        CharacterGeneration = character.Generation;
        CharacterLevel = character.Level;
        CharacterExperienceText = character.Experience.ToString();
        CharacterClassText = character.Class.ToString();

        // Weapon proficiencies
        var wp = character.Characteristics.WeaponProficiencies;
        WeaponProficiencies = new MBBindingList<CharacterInfoPlusMinusItemVM>
    {
        new("One-Handed", wp.OneHanded, 0, 300),
        new("Two-Handed", wp.TwoHanded, 0, 300),
        new("Polearm", wp.Polearm, 0, 300),
        new("Bow", wp.Bow, 0, 300),
        new("Crossbow", wp.Crossbow, 0, 300),
        new("Throwing", wp.Throwing, 0, 300),
    };

        // Skills
        var sk = character.Characteristics.Skills;
        Skills = new MBBindingList<CharacterInfoPlusMinusItemVM>
    {
        new("Iron Flesh", sk.IronFlesh, 0, 20),
        new("Power Strike", sk.PowerStrike, 0, 20),
        new("Power Draw", sk.PowerDraw, 0, 20),
        new("Power Throw", sk.PowerThrow, 0, 20),
        new("Athletics", sk.Athletics, 0, 20),
        new("Riding", sk.Riding, 0, 20),
        new("Weapon Master", sk.WeaponMaster, 0, 20),
        new("Mounted Archery", sk.MountedArchery, 0, 20),
        new("Shield", sk.Shield, 0, 20),
    };
        // Attributes and points
        SkillPoints = sk.Points;
        AttributePoints = character.Characteristics.Attributes.Points;
        Strength = character.Characteristics.Attributes.Strength;
        Agility = character.Characteristics.Attributes.Agility;

        ConvertAttribute = new CharacterInfoConvertItemVM("Attributes", AttributePoints, true);
        ConvertSkill = new CharacterInfoConvertItemVM("Skills", SkillPoints, true);
        StrengthVm = new CharacterInfoPlusMinusItemVM("Strength", Strength, 0, 50);
        AgilityVm = new CharacterInfoPlusMinusItemVM("Agility", Agility, 0, 50);
    }

    private void PrintCharacterValues(CrpgCharacter character)
    {
        Debug.Print($"Name: {character.Name}");
        Debug.Print($"Generation: {character.Generation}");
        Debug.Print($"Level: {character.Level}");
        Debug.Print($"Experience: {character.Experience}");
        Debug.Print($"Class: {character.Class}");

        // Weapon proficiencies
        var wp = character.Characteristics.WeaponProficiencies;
        Debug.Print("Weapon Proficiencies:");
        foreach (var prop in wp.GetType().GetProperties())
        {
            var value = prop.GetValue(wp);
            Debug.Print($"  {prop.Name}: {value}");
        }

        // Skills
        var sk = character.Characteristics.Skills;
        Debug.Print("Skills:");
        foreach (var prop in sk.GetType().GetProperties())
        {
            var value = prop.GetValue(sk);
            Debug.Print($"  {prop.Name}: {value}");
        }
        Debug.Print($"  SkillPoints: {sk.Points}");

        // Attributes
        var attr = character.Characteristics.Attributes;
        Debug.Print("Attributes:");
        foreach (var prop in attr.GetType().GetProperties())
        {
            var value = prop.GetValue(attr);
            Debug.Print($"  {prop.Name}: {value}");
        }
    }

    [DataSourceProperty]
    public int SkillPoints
    {
        get => _skillPoints;
        set => SetField(ref _skillPoints, value, nameof(SkillPoints));
    }

    [DataSourceProperty]
    public int AttributePoints
    {
        get => _attributePoints;
        set => SetField(ref _attributePoints, value, nameof(AttributePoints));
    }

    [DataSourceProperty]
    public int Strength
    {
        get => _strength;
        set => SetField(ref _strength, value, nameof(Strength));
    }

    [DataSourceProperty]
    public int Agility
    {
        get => _agility;
        set => SetField(ref _agility, value, nameof(Agility));
    }

    [DataSourceProperty]
    public string CharacterNameText
    {
        get => _characterNameText;
        set => SetField(ref _characterNameText, value, nameof(CharacterNameText));
    }

    [DataSourceProperty]
    public int CharacterGeneration
    {
        get => _characterGeneration;
        set => SetField(ref _characterGeneration, value, nameof(CharacterGeneration));
    }

    [DataSourceProperty]
    public int CharacterLevel
    {
        get => _characterLevel;
        set => SetField(ref _characterLevel, value, nameof(CharacterLevel));
    }

    [DataSourceProperty]
    public string CharacterExperienceText
    {
        get => _characterExperienceText;
        set => SetField(ref _characterExperienceText, value, nameof(CharacterExperienceText));
    }

    [DataSourceProperty]
    public string CharacterClassText
    {
        get => _characterClassText;
        set => SetField(ref _characterClassText, value, nameof(CharacterClassText));
    }

    [DataSourceProperty]
    public MBBindingList<CharacterInfoPlusMinusItemVM> WeaponProficiencies
    {
        get => _weaponProficiencies;
        set => SetField(ref _weaponProficiencies, value, nameof(WeaponProficiencies));
    }

    [DataSourceProperty]
    public MBBindingList<CharacterInfoPlusMinusItemVM> Skills
    {
        get => _skills;
        set => SetField(ref _skills, value, nameof(Skills));
    }

    [DataSourceProperty]
    public CharacterInfoPlusMinusItemVM StrengthVm
    {
        get => _strengthVm;
        set => SetField(ref _strengthVm, value, nameof(StrengthVm));
    }

    [DataSourceProperty]
    public CharacterInfoPlusMinusItemVM AgilityVm
    {
        get => _agilityVm;
        set => SetField(ref _agilityVm, value, nameof(AgilityVm));
    }

    [DataSourceProperty]
    public CharacterInfoConvertItemVM ConvertAttribute
    {
        get => _convertAttribute;
        set => SetField(ref _convertAttribute, value, nameof(ConvertAttribute));
    }

    [DataSourceProperty]
    public CharacterInfoConvertItemVM ConvertSkill
    {
        get => _convertSkill;
        set => SetField(ref _convertSkill, value, nameof(ConvertSkill));
    }
}
