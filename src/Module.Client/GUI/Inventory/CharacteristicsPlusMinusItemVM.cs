using System;
using TaleWorlds.Library;

namespace Crpg.Module.GUI.Inventory;

public class CharacteristicsPlusMinusItemVM : ViewModel
{
    private string _itemLabel = string.Empty;
    private int _itemValue;
    private bool _isButtonMinusEnabled;
    private bool _isButtonPlusEnabled;

    public event Action<CharacteristicsPlusMinusItemVM, bool>? OnPlusClickedEvent;
    public event Action<CharacteristicsPlusMinusItemVM, bool>? OnMinusClickedEvent;

    public CharacteristicsPlusMinusItemVM(string label, int value)
    {
        _itemLabel = label;
        _itemValue = value;
    }

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
        set
        {
            SetField(ref _itemValue, value, nameof(ItemValue));
        }
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

    private bool _textStateDisabled = false;
    [DataSourceProperty]
    public bool TextStateDisabled
    {
        get => _textStateDisabled;
        set => SetField(ref _textStateDisabled, value, nameof(TextStateDisabled));
    }

    public void ExecutePlusClick()
    {
        OnPlusClickedEvent?.Invoke(this, false);
    }

    public void ExecuteMinusClick()
    {
        OnMinusClickedEvent?.Invoke(this, false);
    }

    public void ExecutePlusAlternateClick()
    {
        OnPlusClickedEvent?.Invoke(this, true);
    }

    public void ExecuteMinusAlternateClick()
    {
        OnMinusClickedEvent?.Invoke(this, true);
    }
}
