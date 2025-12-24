using TaleWorlds.Library;

namespace Crpg.Module.GUI.Inventory;

public class InventorySortTypeVM : ViewModel
{
    private readonly Action<InventorySortTypeVM>? _onClick;
    private bool _showInventoryTypeIcon;
    internal Func<InventorySlotVM, bool> Predicate { get; }
    private bool _isSelected;
    private string _iconSprite;
    public string Id { get; }

    public InventorySortTypeVM(string id, string iconSprite, Func<InventorySlotVM, bool> predicate, bool isIconVisible = true, Action<InventorySortTypeVM>? handleClick = null)
    {
        Id = id;
        _showInventoryTypeIcon = isIconVisible;
        _iconSprite = iconSprite;
        _onClick = handleClick;
        Predicate = predicate;

        RefreshValues();
    }

    private void ExecuteClick()
    {
        IsSelected = !IsSelected;
        _onClick?.Invoke(this);
    }

    [DataSourceProperty]
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (value != _isSelected)
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }
    }

    [DataSourceProperty]
    public string IconSprite
    {
        get => _iconSprite;
        set
        {
            if (value != _iconSprite)
            {
                _iconSprite = value;
                OnPropertyChanged(nameof(IconSprite));
            }
        }
    }

    [DataSourceProperty]
    public bool ShowInventoryTypeIcon
    {
        get => _showInventoryTypeIcon;
        set
        {
            if (value != _showInventoryTypeIcon)
            {
                _showInventoryTypeIcon = value;
                OnPropertyChanged(nameof(ShowInventoryTypeIcon));
            }
        }
    }
}
