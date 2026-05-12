using TaleWorlds.Library;

namespace Crpg.Module.GUI;

internal class CrpgMenuItemVm : ViewModel
{
    private readonly Action? _onExecute;
    private string _text = string.Empty;
    private bool _isEnabled = true;

    public CrpgMenuItemVm(string text, Action? onExecute = null, bool isEnabled = true)
    {
        _text = text;
        _isEnabled = isEnabled;
        _onExecute = onExecute;
    }

    [DataSourceProperty]
    public string Text
    {
        get => _text;
        set
        {
            if (_text == value)
            {
                return;
            }

            _text = value;
            OnPropertyChangedWithValue(value);
        }
    }

    [DataSourceProperty]
    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            if (_isEnabled == value)
            {
                return;
            }

            _isEnabled = value;
            OnPropertyChangedWithValue(value);
        }
    }

    /// <summary>Called by the XML prefab via Command.Click="ExecuteItem".</summary>
    public void ExecuteItem()
    {
        _onExecute?.Invoke();
    }
}
