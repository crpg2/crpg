using System;
using TaleWorlds.Library;

namespace Crpg.Module.GUI.Inventory
{
    public class CharacterInfoPlusMinusItemVM : ViewModel
    {
        private string _itemLabel = string.Empty;
        private int _itemValue;
        private bool _isButtonMinusEnabled;
        private bool _isButtonPlusEnabled;

        private readonly int _minValue;
        private readonly int _maxValue;

        public event Action<CharacterInfoPlusMinusItemVM>? OnPlusClickedEvent;
        public event Action<CharacterInfoPlusMinusItemVM>? OnMinusClickedEvent;

        public CharacterInfoPlusMinusItemVM(string label, int value, int minValue, int maxValue)
        {
            _itemLabel = label;
            _itemValue = value;
            _minValue = minValue;
            _maxValue = maxValue;
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
                if (value < _minValue)
                    value = _minValue;
                if (value > _maxValue)
                    value = _maxValue;
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
            OnPlusClickedEvent?.Invoke(this);
        }

        public void ExecuteMinusClick()
        {
            OnMinusClickedEvent?.Invoke(this);
        }

    }
}
