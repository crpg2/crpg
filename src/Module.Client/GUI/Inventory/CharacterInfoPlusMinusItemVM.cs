using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.Library;

namespace Crpg.Module.GUI.Inventory;

public class CharacterInfoPlusMinusItemVM : ViewModel
{
    private string _itemLabel = string.Empty;
    private int _itemValue = -1;
    private bool _isButtonMinusEnabled;
    private bool _isButtonPlusEnabled;

    public CharacterInfoPlusMinusItemVM(string label, int value, int minValue, int maxValue)
    {
        _itemLabel = label;
        _value = value;
        _minValue = minValue;
        _maxValue = maxValue;
        RefreshValue();
    }

    private readonly int _minValue;
    private readonly int _maxValue;
    private int _value;

    [DataSourceProperty]
    public string ItemLabel
    {
        get => _itemLabel;
        set => SetField(ref _itemLabel, value, nameof(ItemLabel));
    }

    [DataSourceProperty]
    public int ItemValue
    {
        get => _itemValue;
        set => SetField(ref _itemValue, value, nameof(ItemValue));
    }

    [DataSourceProperty]
    public bool IsButtonMinusEnabled
    {
        get => _isButtonMinusEnabled;
        set => SetField(ref _isButtonMinusEnabled, value, nameof(IsButtonMinusEnabled));
    }

    [DataSourceProperty]
    public bool IsButtonPlusEnabled
    {
        get => _isButtonPlusEnabled;
        set => SetField(ref _isButtonPlusEnabled, value, nameof(IsButtonPlusEnabled));
    }

    // Called by minus button
    public void ExecuteClick(ButtonWidget widget)
    {
        if (widget.Id == "ButtonMinus")
        {
            OnMinusClicked();
        }
        else if (widget.Id == "ButtonPlus")
        {
            OnPlusClicked();
        }
    }

    private void OnMinusClicked()
    {
        if (_value > _minValue)
        {
            _value--;
            RefreshValue();
        }
    }

    private void OnPlusClicked()
    {
        if (_value < _maxValue)
        {
            _value++;
            RefreshValue();
        }
    }

    private void RefreshValue()
    {
        ItemValue = _value;
        IsButtonMinusEnabled = _value > _minValue;
        IsButtonPlusEnabled = _value < _maxValue;
    }
}
