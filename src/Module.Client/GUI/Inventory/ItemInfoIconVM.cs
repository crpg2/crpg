using TaleWorlds.Library;

namespace Crpg.Module.GUI.Inventory;

public class ItemInfoIconVM : ViewModel
{
    private string _iconSprite = string.Empty;

    [DataSourceProperty]
    public string IconSprite
    {
        get => _iconSprite;
        set => SetField(ref _iconSprite, value, nameof(IconSprite));
    }

    [DataSourceProperty]
    public bool IsIconVisible => !string.IsNullOrEmpty(IconSprite);
}
