using TaleWorlds.Library;

namespace Crpg.Module.GUI.Inventory;

public class TextGridVM : ViewModel
{
    [DataSourceProperty]
    public MBBindingList<TextEntryVM> TextEntries { get; set; }

    public TextGridVM()
    {
        TextEntries = new MBBindingList<TextEntryVM>();

        // Populate dummy data
        for (int i = 0; i < 24; i++)
        {
            TextEntries.Add(new TextEntryVM($"Slot {i + 1}"));
        }
    }
}