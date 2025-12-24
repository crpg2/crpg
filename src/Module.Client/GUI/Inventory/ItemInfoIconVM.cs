using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;

namespace Crpg.Module.GUI.Inventory;

public class ItemInfoIconVM : ViewModel
{
    private string _iconSprite = string.Empty;
    private string _hintText = string.Empty;
    private BasicTooltipViewModel? _itemHint;
    public ItemInfoIconVM()
    {
        _itemHint = new BasicTooltipViewModel(() => $"{HintText}");
    }

    [DataSourceProperty]
    public string IconSprite
    {
        get => _iconSprite;
        set => SetField(ref _iconSprite, value, nameof(IconSprite));
    }

    [DataSourceProperty]
    public bool IsIconVisible => !string.IsNullOrEmpty(IconSprite);

    [DataSourceProperty]
    public string HintText
    {
        get => _hintText;
        set => SetField(ref _hintText, value, nameof(HintText));
    }

    [DataSourceProperty]
    public BasicTooltipViewModel? ItemHint
    {
        get
        {
            return _itemHint;
        }
        set
        {
            if (value != _itemHint)
            {
                _itemHint = value;
                OnPropertyChangedWithValue<BasicTooltipViewModel?>(value, "ItemHint");
            }
        }
    }
}
