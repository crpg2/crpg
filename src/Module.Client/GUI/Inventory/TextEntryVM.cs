using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Crpg.Module.GUI.Inventory;

public class TextEntryVM : ViewModel
{
    [DataSourceProperty]
    public string Value { get; set; }

    public TextEntryVM(string value)
    {
        Value = value;
    }
}