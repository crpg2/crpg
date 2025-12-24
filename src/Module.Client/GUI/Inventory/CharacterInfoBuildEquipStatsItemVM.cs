using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;

public class CharacterInfoBuildEquipStatsItemVM : ViewModel
{
    private string _labelText;
    private string _valueText;
    private bool _shouldShowGold;
    private bool _textDisabledState;
    private BasicTooltipViewModel _tooltip;

    public CharacterInfoBuildEquipStatsItemVM(string label, string value, string tooltipText, bool shouldShowGold = false, bool textDisabledState = false)
    {
        _labelText = label;
        _valueText = value;
        _tooltip = new BasicTooltipViewModel(() => tooltipText);
        _shouldShowGold = shouldShowGold;
        _textDisabledState = textDisabledState;
    }

    [DataSourceProperty]
    public string LabelText
    {
        get => _labelText;
        set => SetField(ref _labelText, value, nameof(LabelText));
    }

    [DataSourceProperty]
    public string ValueText
    {
        get => _valueText;
        set => SetField(ref _valueText, value, nameof(ValueText));
    }

    [DataSourceProperty]
    public BasicTooltipViewModel Tooltip
    {
        get => _tooltip;
        set => SetField(ref _tooltip, value, nameof(Tooltip));
    }

    [DataSourceProperty]
    public bool ShouldShowGold
    {
        get => _shouldShowGold;
        set => SetField(ref _shouldShowGold, value, nameof(ShouldShowGold));
    }

    [DataSourceProperty]
    public bool TextDisabledState
    {
        get => _textDisabledState;
        set => SetField(ref _textDisabledState, value, nameof(TextDisabledState));
    }
}
