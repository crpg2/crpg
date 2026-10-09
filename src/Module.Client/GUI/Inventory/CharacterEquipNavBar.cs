using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Crpg.Module.GUI.Inventory;

internal enum NavBarTab
{
    Equipment,
    Characteristics,
}

public class CharacterEquipNavBar : ViewModel
{
    internal event Action<NavBarTab>? OnEquipNavButtonClicked;

    internal NavBarTab ActiveTab
    {
        get;
        set
        {
            field = value;
            EquipmentSelected = value == NavBarTab.Equipment;
            CharacteristicsSelected = value == NavBarTab.Characteristics;
        }
    }

    = NavBarTab.Equipment;

    internal void ExecuteClickEquipment() => SelectTab(NavBarTab.Equipment);
    internal void ExecuteClickCharacteristics() => SelectTab(NavBarTab.Characteristics);

    internal void SelectTab(NavBarTab tab)
    {
        ActiveTab = tab;
        OnEquipNavButtonClicked?.Invoke(tab);
    }

    [DataSourceProperty]
    public bool EquipmentSelected { get; set => SetField(ref field, value, nameof(EquipmentSelected)); }
    [DataSourceProperty]
    public bool CharacteristicsSelected { get; set => SetField(ref field, value, nameof(CharacteristicsSelected)); }
    [DataSourceProperty]
    public string EquipmentButtonText { get; set => SetField(ref field, value, nameof(EquipmentButtonText)); }
        = new TextObject("{=KC9dxc50}Equipment").ToString();

    [DataSourceProperty]
    public string CharacteristicsButtonText { get; set => SetField(ref field, value, nameof(CharacteristicsButtonText)); }
        = new TextObject("{=KC9dxc51}Characteristics").ToString();
}
