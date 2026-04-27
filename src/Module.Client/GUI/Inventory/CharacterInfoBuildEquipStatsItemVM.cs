using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;

public class CharacterInfoBuildEquipStatsItemVM : ViewModel
{
    public CharacterInfoBuildEquipStatsItemVM(string label, string value, string tooltipText, bool shouldShowGold = false, bool textDisabledState = false)
    {
        LabelText = label;
        ValueText = value;
        Tooltip = new BasicTooltipViewModel(() => tooltipText);
        ShouldShowGold = shouldShowGold;
        TextDisabledState = textDisabledState;
    }

    [DataSourceProperty]
    public string LabelText { get; set => SetField(ref field, value, nameof(LabelText)); } = string.Empty;

    [DataSourceProperty]
    public string ValueText { get; set => SetField(ref field, value, nameof(ValueText)); } = string.Empty;

    [DataSourceProperty]
    public BasicTooltipViewModel Tooltip { get; set => SetField(ref field, value, nameof(Tooltip)); } = default!;

    [DataSourceProperty]
    public bool ShouldShowGold { get; set => SetField(ref field, value, nameof(ShouldShowGold)); }

    [DataSourceProperty]
    public bool TextDisabledState { get; set => SetField(ref field, value, nameof(TextDisabledState)); }
}
