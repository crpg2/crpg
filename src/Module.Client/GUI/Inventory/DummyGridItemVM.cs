using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using System;
using System.Collections.Generic;
using System.Linq;

public class DummyGridItemVM : ViewModel
{
    private string _text;

    [DataSourceProperty]
    public string Text
    {
        get => _text;
        set => SetField(ref _text, value, nameof(Text));
    }

    public DummyGridItemVM(string text)
    {
        _text = text;
    }
}