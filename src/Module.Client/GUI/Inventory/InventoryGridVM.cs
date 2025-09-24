using Crpg.Module.Api.Models.Items;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;

namespace Crpg.Module.GUI.Inventory;

public class InventoryGridVM : ViewModel
{
    internal event Action<InventorySlotVM>? OnInventorySlotClicked;
    internal event Action<InventorySortTypeVM>? OnInventorySortTypeClicked;
    internal event Action<InventorySlotVM>? OnInventorySlotHoverEnd;
    private List<InventorySlotVM> _availableItems;
    private List<InventorySortTypeVM> _activeFilters = new();
    private MBBindingList<InventorySlotVM> _filteredItems;
    private MBBindingList<InventorySortTypeVM> _inventorySortTypesLeft = new();
    private MBBindingList<InventorySortTypeVM> _inventorySortTypesTop = new();

    public InventoryGridVM()
    {
        _availableItems = new List<InventorySlotVM>();
        _filteredItems = new MBBindingList<InventorySlotVM>();

        InitializeSortTypes();
        InitializeFilteredItemsList();
    }

    public void SetAvailableItems(IEnumerable<(ItemObject item, int quantity, CrpgUserItemExtended userItemExtended)> inventory)
    {
        _availableItems = new List<InventorySlotVM>();
        foreach (var (item, quantity, userItemExtended) in inventory)
        {
            _availableItems.Add(new InventorySlotVM(item, slot => OnItemClicked(slot), slot => OnItemHoverEnd(slot), quantity, userItemExtended));
        }
    }

    private void OnItemClicked(InventorySlotVM? slot)
    {
        if (slot?.ItemObj != null)
        {
            InformationManager.DisplayMessage(new InformationMessage($"Clicked: {slot.ItemName}"));
            OnInventorySlotClicked?.Invoke(slot);
        }
    }

    private void OnItemHoverEnd(InventorySlotVM? slot)
    {
        if (slot?.ItemObj != null)
        {
            OnInventorySlotHoverEnd?.Invoke(slot);
        }
    }

    [DataSourceProperty]
    public List<InventorySlotVM> AvailableItems
    {
        get => _availableItems;
        set => SetField(ref _availableItems, value, nameof(AvailableItems));
    }

    [DataSourceProperty]
    public MBBindingList<InventorySlotVM> FilteredItems
    {
        get => _filteredItems;
        set => SetField(ref _filteredItems, value, nameof(FilteredItems));
    }

    // Add support for filtering, sorting, pagination, etc.

    [DataSourceProperty]
    public MBBindingList<InventorySortTypeVM> InventorySortTypesLeft
    {
        get => _inventorySortTypesLeft;
        set => SetField(ref _inventorySortTypesLeft, value, nameof(InventorySortTypesLeft));
    }

    [DataSourceProperty]
    public MBBindingList<InventorySortTypeVM> InventorySortTypesTop
    {
        get => _inventorySortTypesTop;
        set => SetField(ref _inventorySortTypesTop, value, nameof(InventorySortTypesTop));
    }

    public void InitializeFilteredItemsList()
    {
        _filteredItems.Clear();

        var sortedItems = _availableItems
            .OrderBy(item => item.ItemObj?.ItemType ?? ItemObject.ItemTypeEnum.Invalid)
            .ThenBy(item => item.ItemName)
            .ToList();

        foreach (var slot in sortedItems)
        {
            _filteredItems.Add(slot);
        }
    }

    private void InitializeSortTypes()
    {
        InventorySortTypesLeft = new MBBindingList<InventorySortTypeVM>
        {
            new("ui_crpg_icon_white_onehanded", item => item.ItemType == ItemObject.ItemTypeEnum.OneHandedWeapon, true, OnSortTypeClicked),
            new("ui_crpg_icon_white_twohanded", item => item.ItemType == ItemObject.ItemTypeEnum.TwoHandedWeapon, true, OnSortTypeClicked),
            new("ui_crpg_icon_white_polearm", item => item.ItemType == ItemObject.ItemTypeEnum.Polearm, true, OnSortTypeClicked),
            new("ui_crpg_icon_white_shield", item => item.ItemType == ItemObject.ItemTypeEnum.Shield, true, OnSortTypeClicked),
            new("ui_crpg_icon_white_thrown", item => item.ItemType == ItemObject.ItemTypeEnum.Thrown, true, OnSortTypeClicked),
            new("ui_crpg_icon_white_bow", item => item.ItemType == ItemObject.ItemTypeEnum.Bow, true, OnSortTypeClicked),
            new("ui_crpg_icon_white_crossbow", item => item.ItemType == ItemObject.ItemTypeEnum.Crossbow, true, OnSortTypeClicked),
            new("ui_crpg_icon_white_musket", item =>
                item.ItemType == ItemObject.ItemTypeEnum.Musket ||
                item.ItemType == ItemObject.ItemTypeEnum.Pistol,
                true, OnSortTypeClicked),
            new("ui_crpg_icon_white_arrow", item =>
                item.ItemType == ItemObject.ItemTypeEnum.Arrows ||
                item.ItemType == ItemObject.ItemTypeEnum.Bolts ||
                item.ItemType == ItemObject.ItemTypeEnum.Bullets,
                true, OnSortTypeClicked),
        };

        InventorySortTypesTop = new MBBindingList<InventorySortTypeVM>
        {
            new("ui_crpg_icon_white_headarmor", item => item.ItemType == ItemObject.ItemTypeEnum.HeadArmor, true, OnSortTypeClicked),
            new("ui_crpg_icon_white_cape", item => item.ItemType == ItemObject.ItemTypeEnum.Cape, true, OnSortTypeClicked),
            new("ui_crpg_icon_white_chestarmor", item => item.ItemType == ItemObject.ItemTypeEnum.BodyArmor, true, OnSortTypeClicked),
            new("ui_crpg_icon_white_handarmor", item => item.ItemType == ItemObject.ItemTypeEnum.HandArmor, true, OnSortTypeClicked),
            new("ui_crpg_icon_white_legarmor", item => item.ItemType == ItemObject.ItemTypeEnum.LegArmor, true, OnSortTypeClicked),
            new("ui_crpg_icon_white_mount", item => item.ItemType == ItemObject.ItemTypeEnum.Horse, true, OnSortTypeClicked),
            new("ui_crpg_icon_white_mountharness", item => item.ItemType == ItemObject.ItemTypeEnum.HorseHarness, true, OnSortTypeClicked),
            new("ui_crpg_icon_white_flag", item => item.ItemType == ItemObject.ItemTypeEnum.Banner, true, OnSortTypeClicked),
        };
    }

    private void OnSortTypeClicked(InventorySortTypeVM clickedSort)
    {
        InformationManager.DisplayMessage(new InformationMessage($"Clicked sort icon: {clickedSort.IconSprite}"));

        // TODO: Apply filtering logic here and refresh visible items.
        RefreshFilteredItemsFast();

        if (clickedSort != null)
        {
            OnInventorySortTypeClicked?.Invoke(clickedSort);
        }
    }

    private bool PassesFilters(InventorySlotVM slot)
    {
        if (_activeFilters.Count == 0)
        {
            return true;
        }

        var item = slot.ItemObj;
        return item != null && item.ItemType != ItemObject.ItemTypeEnum.Invalid
            && _activeFilters.Any(f => MatchesFilter(item, f));
    }

    private bool MatchesFilter(ItemObject item, InventorySortTypeVM filter)
    {
        return filter?.Predicate?.Invoke(item) ?? false;
    }

    private void RefreshFilteredItemsFast()
    {
        // Update active filters from selected filters
        _activeFilters = InventorySortTypesLeft
            .Concat(InventorySortTypesTop)
            .Where(f => f.IsSelected)
            .ToList();

        // Filter and sort all items
        var filteredSortedItems = AvailableItems
            .Where(slot => PassesFilters(slot))
            .OrderBy(item => item.ItemObj?.ItemType ?? ItemObject.ItemTypeEnum.Invalid)
            .ThenBy(item => item.ItemName)
            .ToList();

        // Clear and bulk add (fewer UI notifications)
        FilteredItems.Clear();
        foreach (var item in filteredSortedItems)
        {
            FilteredItems.Add(item);
        }
    }
}
