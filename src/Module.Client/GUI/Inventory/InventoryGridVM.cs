using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;

namespace Crpg.Module.GUI.Inventory;

public class InventoryGridVM : ViewModel
{
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

    public void SetAvailableItems(IEnumerable<(ItemObject item, int quantity)> inventory)
    {
        _availableItems = new List<InventorySlotVM>();
        foreach (var (item, quantity) in inventory)
        {
            _availableItems.Add(new InventorySlotVM(item, quantity));
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

        foreach (var slot in _availableItems)
        {
            _filteredItems.Add(slot);
        }
    }

    private void InitializeSortTypes()
    {
        InventorySortTypesLeft = new MBBindingList<InventorySortTypeVM>
        {
            new("General\\EquipmentIcons\\equipment_type_one_handed", item => item.ItemType == ItemObject.ItemTypeEnum.OneHandedWeapon, true, OnSortTypeClicked),
            new("General\\EquipmentIcons\\equipment_type_two_handed", item => item.ItemType == ItemObject.ItemTypeEnum.TwoHandedWeapon, true, OnSortTypeClicked),
            new("General\\EquipmentIcons\\equipment_type_polearm", item => item.ItemType == ItemObject.ItemTypeEnum.Polearm, true, OnSortTypeClicked),
            new("General\\EquipmentIcons\\equipment_type_shield", item => item.ItemType == ItemObject.ItemTypeEnum.Shield, true, OnSortTypeClicked),
            new("General\\EquipmentIcons\\equipment_type_throwing", item => item.ItemType == ItemObject.ItemTypeEnum.Thrown, true, OnSortTypeClicked),
            new("General\\EquipmentIcons\\equipment_type_bow", item => item.ItemType == ItemObject.ItemTypeEnum.Bow, true, OnSortTypeClicked),
            new("General\\EquipmentIcons\\equipment_type_crossbow", item =>
                item.ItemType == ItemObject.ItemTypeEnum.Crossbow ||
                item.ItemType == ItemObject.ItemTypeEnum.Musket ||
                item.ItemType == ItemObject.ItemTypeEnum.Pistol,
                true, OnSortTypeClicked),
            new("General\\EquipmentIcons\\equipment_type_quiver", item =>
                item.ItemType == ItemObject.ItemTypeEnum.Arrows ||
                item.ItemType == ItemObject.ItemTypeEnum.Bolts ||
                item.ItemType == ItemObject.ItemTypeEnum.Bullets,
                true, OnSortTypeClicked),
        };

        InventorySortTypesTop = new MBBindingList<InventorySortTypeVM>
        {
            new("General\\EquipmentIcons\\equipment_type_head_armor", item => item.ItemType == ItemObject.ItemTypeEnum.HeadArmor, true, OnSortTypeClicked),
            new("General\\EquipmentIcons\\equipment_type_cape", item => item.ItemType == ItemObject.ItemTypeEnum.Cape, true, OnSortTypeClicked),
            new("General\\EquipmentIcons\\equipment_type_body_armor", item => item.ItemType == ItemObject.ItemTypeEnum.BodyArmor, true, OnSortTypeClicked),
            new("General\\EquipmentIcons\\equipment_type_hand_armor", item => item.ItemType == ItemObject.ItemTypeEnum.HandArmor, true, OnSortTypeClicked),
            new("General\\EquipmentIcons\\equipment_type_leg_armor", item => item.ItemType == ItemObject.ItemTypeEnum.LegArmor, true, OnSortTypeClicked),
            new("General\\EquipmentIcons\\equipment_type_mount", item =>
                item.ItemType == ItemObject.ItemTypeEnum.Horse ||
                item.ItemType == ItemObject.ItemTypeEnum.HorseHarness,
                true, OnSortTypeClicked),
            new("General\\EquipmentIcons\\equipment_type_banner", item => item.ItemType == ItemObject.ItemTypeEnum.Banner, true, OnSortTypeClicked),
        };
    }

    private void OnSortTypeClicked(InventorySortTypeVM clickedSort)
    {
        InformationManager.DisplayMessage(new InformationMessage($"Clicked sort icon: {clickedSort.IconSprite}"));

        // TODO: Apply filtering logic here and refresh visible items.
        RefreshFilteredItems();
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

    private void RefreshFilteredItems()
    {
        FilteredItems.Clear();

        _activeFilters = InventorySortTypesLeft
            .Concat(InventorySortTypesTop)
            .Where(f => f.IsSelected)
            .ToList();

        foreach (var slot in AvailableItems)
        {
            if (PassesFilters(slot))
            {
                FilteredItems.Add(slot);
            }
        }
    }
}
