using TaleWorlds.Library;

namespace Crpg.Module.GUI.AnimationMenu;

/// <summary>
/// ViewModel for a single row in the emote menu.
/// Works for both category navigation rows and emote play rows.
/// The XML prefab binds Text, IsEnabled, and calls ExecuteItem() on click.
/// </summary>
internal class CrpgAnimationMenuItemVm : ViewModel
{
    private string _text = string.Empty;
    private bool _isEnabled = true;
    private readonly Action? _onExecute;

    public CrpgAnimationMenuItemVm(string text, Action? onExecute = null, bool isEnabled = true)
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
            if (_text == value) return;
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
            if (_isEnabled == value) return;
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
