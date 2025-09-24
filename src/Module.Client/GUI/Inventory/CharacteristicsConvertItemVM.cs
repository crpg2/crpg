using TaleWorlds.Library;

namespace Crpg.Module.GUI.Inventory;

public class CharacteristicsConvertItemVM : ViewModel
{
    private string _itemLabel;
    private int _itemValue;
    private bool _isButtonEnabled;

    public event Action<CharacteristicsConvertItemVM>? OnConvertClickedEvent;

    public CharacteristicsConvertItemVM(string label, int value, bool isEnabled)
    {
        _itemLabel = label;
        _itemValue = value;
        _isButtonEnabled = isEnabled;
    }

    [DataSourceProperty]
    public string ItemLabel { get => _itemLabel; set => SetField(ref _itemLabel, value, nameof(ItemLabel)); }

    [DataSourceProperty]
    public int ItemValue { get => _itemValue; set => SetField(ref _itemValue, value, nameof(ItemValue)); }

    [DataSourceProperty]
    public bool IsButtonEnabled { get => _isButtonEnabled; set => SetField(ref _isButtonEnabled, value, nameof(IsButtonEnabled)); }

    public void ExecuteClick()
    {
        OnConvertClickedEvent?.Invoke(this);
    }
}
