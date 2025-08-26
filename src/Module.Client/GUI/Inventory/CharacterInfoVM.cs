using System.Collections.Generic;
using TaleWorlds.Library;

namespace Crpg.Module.GUI.Inventory;

public class CharacterInfoVM : ViewModel
{
    private string _characterNameText = string.Empty;
    private string _characterGenerationText = string.Empty;
    private string _characterLevelText = string.Empty;
    private string _characterExperienceText = string.Empty;
    private string _characterClassText = string.Empty;

    private MBBindingList<CharacterInfoPlusMinusItemVM> _attributesAndWpf = new();
    private MBBindingList<CharacterInfoPlusMinusItemVM> _skills = new();

    private CharacterInfoConvertItemVM _convertAttribute;
    private CharacterInfoConvertItemVM _convertSkill;

    public CharacterInfoVM()
    {
        // Example: init from some character model
        CharacterNameText = string.Empty;
        CharacterGenerationText = string.Empty;
        CharacterLevelText = string.Empty;
        CharacterExperienceText = string.Empty;
        CharacterClassText = string.Empty;

        AttributesAndWpf = new MBBindingList<CharacterInfoPlusMinusItemVM>
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
    }

    [DataSourceProperty]
    public string CharacterNameText
    {
        get => _characterNameText;
        set => SetField(ref _characterNameText, value, nameof(CharacterNameText));
    }

    [DataSourceProperty]
    public string CharacterGenerationText
    {
        get => _characterGenerationText;
        set => SetField(ref _characterGenerationText, value, nameof(CharacterGenerationText));
    }

    [DataSourceProperty]
    public string CharacterLevelText
    {
        get => _characterLevelText;
        set => SetField(ref _characterLevelText, value, nameof(CharacterLevelText));
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
    public MBBindingList<CharacterInfoPlusMinusItemVM> AttributesAndWpf
    {
        get => _attributesAndWpf;
        set => SetField(ref _attributesAndWpf, value, nameof(AttributesAndWpf));
    }

    [DataSourceProperty]
    public MBBindingList<CharacterInfoPlusMinusItemVM> Skills
    {
        get => _skills;
        set => SetField(ref _skills, value, nameof(Skills));
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
