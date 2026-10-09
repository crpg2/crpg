using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Crpg.Module.GUI.Inventory;

public class CharacteristicsConvertItemVM : ViewModel
{
    private readonly string _tooltipText;

    internal event Action<CharacteristicsConvertItemVM>? OnConvertClickedEvent;

    public CharacteristicsConvertItemVM(string label, int value, bool isEnabled = true, string tooltipText = "")
    {
        ItemLabel = label;
        ItemValue = value;
        IsButtonEnabled = isEnabled;
        _tooltipText = tooltipText;
    }

    [DataSourceProperty]
    public string ItemLabel { get; set => SetField(ref field, value, nameof(ItemLabel)); }

    [DataSourceProperty]
    public int ItemValue { get; set => SetField(ref field, value, nameof(ItemValue)); }

    [DataSourceProperty]
    public bool IsButtonEnabled { get; set => SetField(ref field, value, nameof(IsButtonEnabled)); }

    // ===== Event handlers for UI actions =====
    private void ExecuteClick() => OnConvertClickedEvent?.Invoke(this);
    private void ExecuteBeginHint() => MBInformationManager.ShowHint(_tooltipText);
    private void ExecuteEndHint() => MBInformationManager.HideInformations();
}
