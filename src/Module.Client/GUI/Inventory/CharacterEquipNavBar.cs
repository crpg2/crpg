using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.Library;

namespace Crpg.Module.GUI.Inventory;

public class CharacterEquipNavBar : ViewModel
{
    internal event Action<string>? OnEquipNavButtonClicked;
    private bool _equipmentSelected;
    private bool _characteristicsSelected;

    public CharacterEquipNavBar()
    {
    }

    public void ExecuteClickEquipment()
    {
        OnEquipNavButtonClicked?.Invoke("Equipment");
    }

    public void ExecuteClickCharacteristics()
    {
        OnEquipNavButtonClicked?.Invoke("Characteristics");
    }

    [DataSourceProperty]
    public bool EquipmentSelected { get => _equipmentSelected; set => SetField(ref _equipmentSelected, value, nameof(EquipmentSelected)); }
    [DataSourceProperty]
    public bool CharacteristicsSelected { get => _characteristicsSelected; set => SetField(ref _characteristicsSelected, value, nameof(CharacteristicsSelected)); }
}
