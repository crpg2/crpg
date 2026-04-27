using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;

namespace Crpg.Module.GUI.Inventory;

public class CharacteristicsPlusMinusItemVM : ViewModel
{
    internal event Action<CharacteristicsPlusMinusItemVM, bool>? OnPlusClickedEvent;
    internal event Action<CharacteristicsPlusMinusItemVM, bool>? OnMinusClickedEvent;

    public CharacteristicsPlusMinusItemVM(string itemKey, string label, int value, string tooltipText = "")
    {
        ItemKey = itemKey;
        ItemLabel = label;
        ItemValue = value;
        Tooltip = new BasicTooltipViewModel(() => tooltipText);
    }

    public string ItemKey { get; } // stable English identifier, never localized

    [DataSourceProperty]
    public string ItemLabel { get; set => SetField(ref field, value, nameof(ItemLabel)); } = string.Empty;

    [DataSourceProperty]
    public int ItemValue { get; set => SetField(ref field, value, nameof(ItemValue)); }

    [DataSourceProperty]
    public bool IsButtonMinusEnabled { get; set => SetField(ref field, value, nameof(IsButtonMinusEnabled)); }

    [DataSourceProperty]
    public bool IsButtonPlusEnabled { get; set => SetField(ref field, value, nameof(IsButtonPlusEnabled)); }

    [DataSourceProperty]
    public bool TextStateDisabled { get; set => SetField(ref field, value, nameof(TextStateDisabled)); }

    [DataSourceProperty]
    public BasicTooltipViewModel? Tooltip { get; set => SetField(ref field, value, nameof(Tooltip)); }

    // ===== Event handlers for UI actions =====
    private void ExecutePlusClick() => OnPlusClickedEvent?.Invoke(this, false);
    private void ExecuteMinusClick() => OnMinusClickedEvent?.Invoke(this, false);
    private void ExecutePlusAlternateClick() => OnPlusClickedEvent?.Invoke(this, true);
    private void ExecuteMinusAlternateClick() => OnMinusClickedEvent?.Invoke(this, true);
}
