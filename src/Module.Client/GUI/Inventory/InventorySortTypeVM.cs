using System;
using JetBrains.Annotations;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.GauntletUI.Widgets;

namespace Crpg.Module.GUI.Inventory;

public class InventorySortTypeVM : ViewModel
{
    private readonly Action<InventorySortTypeVM>? _onClick;
    private bool _showInventoryTypeIcon;
    public Func<ItemObject, bool> Predicate { get; }
    private bool _isSelected;
    private string _iconSprite;

    public InventorySortTypeVM(string iconSprite, Func<ItemObject, bool> predicate, bool isIconVisible = true, Action<InventorySortTypeVM>? handleClick = null)
    {
        _showInventoryTypeIcon = isIconVisible;
        _iconSprite = iconSprite;
        _onClick = handleClick;
        Predicate = predicate;

        RefreshValues();
    }

    public void ExecuteClick()
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
