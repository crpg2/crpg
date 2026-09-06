using TaleWorlds.Library;

namespace Crpg.Module.GUI.VoiceCommands;

internal class CrpgVoiceCommandsVm : ViewModel
{
    private bool _isMenuVisible;
    private string _title = string.Empty;
    private MBBindingList<CrpgVoiceCommandItemVm> _items = new();

    public CrpgVoiceCommandsVm()
    {
        RefreshValues();
    }

    public sealed override void RefreshValues()
    {
        base.RefreshValues();
    }

    public void ShowMenu(string title, IReadOnlyList<VoiceCommandMenuItem> items)
    {
        Title = title;
        var list = new MBBindingList<CrpgVoiceCommandItemVm>();
        foreach (var item in items)
        {
            list.Add(new CrpgVoiceCommandItemVm(item));
        }

        Items = list;
        IsMenuVisible = true;
    }

    public void HideMenu()
    {
        IsMenuVisible = false;
    }

    [DataSourceProperty]
    public bool IsMenuVisible
    {
        get => _isMenuVisible;
        set
        {
            if (value != _isMenuVisible)
            {
                _isMenuVisible = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public string Title
    {
        get => _title;
        set
        {
            if (value != _title)
            {
                _title = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public MBBindingList<CrpgVoiceCommandItemVm> Items
    {
        get => _items;
        set
        {
            if (value != _items)
            {
                _items = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }
}

internal class CrpgVoiceCommandItemVm : ViewModel
{
    private string _beforePart;
    private string _keyPart;
    private string _afterPart;

    public CrpgVoiceCommandItemVm(VoiceCommandMenuItem item)
    {
        char keyLetter = item.Key.ToString()[0];
        string suffix = item.IsCategory ? "..." : string.Empty;
        string label = item.Label;

        int idx = label.IndexOf(keyLetter.ToString(), StringComparison.OrdinalIgnoreCase);
        if (idx >= 0)
        {
            _beforePart = label.Substring(0, idx);
            _keyPart = $"[{label[idx]}]";
            _afterPart = label.Substring(idx + 1) + suffix;
        }
        else
        {
            _beforePart = string.Empty;
            _keyPart = $"[{keyLetter}]";
            _afterPart = " " + label + suffix;
        }
    }

    [DataSourceProperty]
    public string BeforePart
    {
        get => _beforePart;
        set
        {
            if (value != _beforePart)
            {
                _beforePart = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public string KeyPart
    {
        get => _keyPart;
        set
        {
            if (value != _keyPart)
            {
                _keyPart = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public string AfterPart
    {
        get => _afterPart;
        set
        {
            if (value != _afterPart)
            {
                _afterPart = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }
}
