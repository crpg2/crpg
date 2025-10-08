using Crpg.Module.Api.Models.Items;
using Crpg.Module.Common;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.GUI.Inventory;

public class InventoryGridVM : ViewModel
{
    public enum InventorySection
    {
        Inventory,
        Armory,
    }

    private bool _isVisible;
    [DataSourceProperty]
    public bool IsVisible { get => _isVisible; set => SetField(ref _isVisible, value, nameof(IsVisible)); }

    internal event Action<InventorySlotVM>? OnInventorySlotClicked;
    internal event Action<InventorySortTypeVM>? OnInventorySortTypeClicked;
    internal event Action<InventorySlotVM>? OnInventorySlotHoverEnd;
    internal event Action<InventorySlotVM>? OnInventorySlotDragStart;
    internal event Action<InventorySlotVM>? OnInventorySlotDragEnd;
    internal event Action<int>? OnInventoryChangeType;

    // Two item sources
    private readonly MBBindingList<InventorySlotVM> _inventoryItems = new();
    private readonly MBBindingList<InventorySlotVM> _armoryItems = new();
    private readonly CrpgClanArmoryClient? _clanArmory;

    private List<InventorySortTypeVM> _activeFilters = new();
    private MBBindingList<InventorySlotVM> _filteredItems;
    private MBBindingList<InventorySlotVM> _availableItems = new();
    private MBBindingList<InventorySortTypeVM> _inventorySortTypesLeft = new();
    private MBBindingList<InventorySortTypeVM> _inventorySortTypesTop = new();

    private InventorySection _activeSection;

    private bool _userInventorySelected;
    private bool _armorySelected;

    private bool _isUserInventoryButtonVisible;
    private bool _isArmoryButtonVisible;

    public InventoryGridVM()
    {
        _filteredItems = new MBBindingList<InventorySlotVM>();
        _clanArmory = Mission.Current?.GetMissionBehavior<CrpgClanArmoryClient>();

        // Default to user inventory
        _isUserInventoryButtonVisible = true; // TODO: make configurable (ie strategus or events)
        _activeSection = InventorySection.Inventory;
        UserInventorySelected = true;

        InitializeSortTypes();
        InitializeFilteredItemsList();
    }

    [DataSourceProperty]
    public InventorySection ActiveSection
    {
        get => _activeSection;
        set
        {
            if (_activeSection != value)
            {
                _activeSection = value;
                RefreshDisplayedItems();
                OnPropertyChanged(nameof(ActiveSection));

                if (value == InventorySection.Armory)
                {
                    UserInventorySelected = false;
                    ArmorySelected = true;
                }
                else if (value == InventorySection.Inventory)
                {
                    UserInventorySelected = true;
                    ArmorySelected = false;
                }
            }
        }
    }

    internal void SetInventoryItems(IEnumerable<(ItemObject item, int quantity, CrpgUserItemExtended userItemExtended)> items)
    {
        _inventoryItems.Clear();
        foreach (var slot in BuildSlots(items))
        {
            _inventoryItems.Add(slot);
        }

        if (ActiveSection == InventorySection.Inventory)
        {
            RefreshDisplayedItems();
        }
    }

    internal void SetArmoryItems(IEnumerable<(ItemObject item, int quantity, CrpgUserItemExtended userItemExtended)> items)
    {
        _armoryItems.Clear();
        foreach (var slot in BuildSlots(items))
        {
            _armoryItems.Add(slot);
        }

        if (ActiveSection == InventorySection.Armory)
        {
            RefreshDisplayedItems();
        }

        // Show armory button if there are any armory items
        IsArmoryButtonVisible = _armoryItems.Count > 0;
    }

    internal void SetAvailableItems(IEnumerable<(ItemObject itemObj, int count, CrpgUserItemExtended userItem)> items)
    {
        AvailableItems.Clear();

        foreach (var (itemObj, count, userItem) in items)
        {
            var slotVm = new InventorySlotVM(
                itemObj,
                slot => OnInventorySlotClicked?.Invoke(slot),   // forward click
                slot => OnInventorySlotHoverEnd?.Invoke(slot), // forward hover end
                slot => OnInventorySlotDragStart?.Invoke(slot), // forward drag start
                slot => OnInventorySlotDragEnd?.Invoke(slot),   // forward drag end
                count,
                userItem);

            AvailableItems.Add(slotVm);
        }
    }

    internal void InitializeFilteredItemsList()
    {
        _filteredItems.Clear();

        var sortedItems = AvailableItems
            .OrderBy(item => item.ItemObj?.ItemType ?? ItemObject.ItemTypeEnum.Invalid)
            .ThenBy(item => item.ItemName)
            .ToList();

        foreach (var slot in sortedItems)
        {
            _filteredItems.Add(slot);
        }
    }

    private void ExecuteClickArmory()
    {
        LogDebug($"Clicked Armory: (2)");
        OnInventoryChangeType?.Invoke(2);
        ActiveSection = InventorySection.Armory;
    }

    private void ExecuteClickUserInventory()
    {
        LogDebug($"Clicked User Inventory: (1)");
        OnInventoryChangeType?.Invoke(1);
        ActiveSection = InventorySection.Inventory;
    }

    private void OnItemClicked(InventorySlotVM? slot)
    {
        if (slot?.ItemObj != null)
        {
            LogDebug($"Clicked: {slot.ItemName}");
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

    private void OnItemDragStart(InventorySlotVM? slot)
    {
        if (slot?.ItemObj != null)
        {
            OnInventorySlotDragStart?.Invoke(slot);
        }
    }

    private void OnItemDragEnd(InventorySlotVM? slot)
    {
        if (slot?.ItemObj != null)
        {
            OnInventorySlotDragEnd?.Invoke(slot);
        }
    }

    private void RefreshDisplayedItems()
    {
        // Determine source list based on active section
        var source = ActiveSection == InventorySection.Inventory
            ? _inventoryItems
            : _armoryItems;

        // Use HashSet for quick lookups
        var currentSet = new HashSet<InventorySlotVM>(AvailableItems);
        var sourceSet = new HashSet<InventorySlotVM>(source);

        // Remove items that are no longer in the source
        for (int i = AvailableItems.Count - 1; i >= 0; i--)
        {
            var item = AvailableItems[i];
            if (!sourceSet.Contains(item))
            {
                AvailableItems.RemoveAt(i);
            }
        }

        // Add new items from source that are missing
        foreach (var item in source)
        {
            if (!currentSet.Contains(item))
            {
                // check if source is inventory and omit armory items not borrowed by you. TODO: add a button to toggle this later
                if (source == _inventoryItems && item.IsArmoryItem)
                {
                    if (item?.UserItemEx?.Id is not null && _clanArmory?.GetCrpgUserItemArmoryStatus(item.UserItemEx.Id, out var armoryStatus) == true)
                    {
                        if (armoryStatus == Api.Models.CrpgGameArmoryItemStatus.BorrowedByYou)
                        {
                            AvailableItems.Add(item);
                        }
                    }
                }
                else
                {
                    AvailableItems.Add(item);
                }
            }
        }

        // Optionally: sort after syncing
        var sortedItems = AvailableItems
            .OrderBy(item => item.ItemObj?.ItemType ?? ItemObject.ItemTypeEnum.Invalid)
            .ThenBy(item => item.ItemName)
            .ToList();

        AvailableItems.Clear();
        foreach (var item in sortedItems)
        {
            AvailableItems.Add(item);
        }

        // Refresh filters after updating available items
        RefreshFilteredItemsFast();
    }

    private void InitializeSortTypes()
    {
        InventorySortTypesLeft = new MBBindingList<InventorySortTypeVM>
        {
            new("onehanded", "ui_crpg_icon_white_onehanded", slot => slot.ItemObj?.ItemType == ItemObject.ItemTypeEnum.OneHandedWeapon, true, OnSortTypeClicked),
            new("twohanded", "ui_crpg_icon_white_twohanded", slot => slot.ItemObj?.ItemType == ItemObject.ItemTypeEnum.TwoHandedWeapon, true, OnSortTypeClicked),
            new("polearm", "ui_crpg_icon_white_polearm", slot => slot.ItemObj?.ItemType == ItemObject.ItemTypeEnum.Polearm, true, OnSortTypeClicked),
            new("shield", "ui_crpg_icon_white_shield", slot => slot.ItemObj?.ItemType == ItemObject.ItemTypeEnum.Shield, true, OnSortTypeClicked),
            new("thrown", "ui_crpg_icon_white_thrown", slot => slot.ItemObj?.ItemType == ItemObject.ItemTypeEnum.Thrown, true, OnSortTypeClicked),
            new("bow", "ui_crpg_icon_white_bow", slot => slot.ItemObj?.ItemType == ItemObject.ItemTypeEnum.Bow, true, OnSortTypeClicked),
            new("crossbow", "ui_crpg_icon_white_crossbow", slot => slot.ItemObj?.ItemType == ItemObject.ItemTypeEnum.Crossbow, true, OnSortTypeClicked),
            new("gun", "ui_crpg_icon_white_musket", slot =>
                slot.ItemObj?.ItemType == ItemObject.ItemTypeEnum.Musket ||
                slot.ItemObj?.ItemType == ItemObject.ItemTypeEnum.Pistol,
                true, OnSortTypeClicked),
            new("ammo", "ui_crpg_icon_white_arrow", slot =>
                slot.ItemObj?.ItemType == ItemObject.ItemTypeEnum.Arrows ||
                slot.ItemObj?.ItemType == ItemObject.ItemTypeEnum.Bolts ||
                slot.ItemObj?.ItemType == ItemObject.ItemTypeEnum.Bullets,
                true, OnSortTypeClicked),
        };

        InventorySortTypesTop = new MBBindingList<InventorySortTypeVM>
        {
            new("headarmor", "ui_crpg_icon_white_headarmor", slot => slot.ItemObj?.ItemType == ItemObject.ItemTypeEnum.HeadArmor, true, OnSortTypeClicked),
            new("cape", "ui_crpg_icon_white_cape", slot => slot.ItemObj?.ItemType == ItemObject.ItemTypeEnum.Cape, true, OnSortTypeClicked),
            new("chestarmor", "ui_crpg_icon_white_chestarmor", slot => slot.ItemObj?.ItemType == ItemObject.ItemTypeEnum.BodyArmor, true, OnSortTypeClicked),
            new("handarmor", "ui_crpg_icon_white_handarmor", slot => slot.ItemObj?.ItemType == ItemObject.ItemTypeEnum.HandArmor, true, OnSortTypeClicked),
            new("legarmor", "ui_crpg_icon_white_legarmor", slot => slot.ItemObj?.ItemType == ItemObject.ItemTypeEnum.LegArmor, true, OnSortTypeClicked),
            new("mount", "ui_crpg_icon_white_mount", slot => slot.ItemObj?.ItemType == ItemObject.ItemTypeEnum.Horse, true, OnSortTypeClicked),
            new("mountarmor", "ui_crpg_icon_white_mountharness", slot => slot.ItemObj?.ItemType == ItemObject.ItemTypeEnum.HorseHarness, true, OnSortTypeClicked),
            new("banner", "ui_crpg_icon_white_flag", slot => slot.ItemObj?.ItemType == ItemObject.ItemTypeEnum.Banner, true, OnSortTypeClicked),
        };
    }

    private void OnSortTypeClicked(InventorySortTypeVM clickedSort)
    {
        LogDebug($"Clicked sort icon: {clickedSort.IconSprite}");
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

        return _activeFilters.Any(f => MatchesFilter(slot, f));
    }

    private bool MatchesFilter(InventorySlotVM slot, InventorySortTypeVM filter)
    {
        return filter?.Predicate?.Invoke(slot) ?? false;
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

    private List<InventorySlotVM> BuildSlots(IEnumerable<(ItemObject item, int quantity, CrpgUserItemExtended userItemExtended)> items)
    {
        return items
            .Select(tuple => new InventorySlotVM(
                tuple.item,
                slot => OnItemClicked(slot),
                slot => OnItemHoverEnd(slot),
                slot => OnItemDragStart(slot), // forward drag start
                slot => OnItemDragEnd(slot),   // forward drag end
                tuple.quantity,
                tuple.userItemExtended))
            .ToList();
    }

    [DataSourceProperty]
    public MBBindingList<InventorySlotVM> AvailableItems
    {
        get => _availableItems;
        set
        {
            if (value != _availableItems)
            {
                _availableItems = value;
                OnPropertyChangedWithValue(value, nameof(AvailableItems));
            }
        }
    }

    [DataSourceProperty]
    public MBBindingList<InventorySlotVM> FilteredItems { get => _filteredItems; set => SetField(ref _filteredItems, value, nameof(FilteredItems)); }

    [DataSourceProperty]
    public bool UserInventorySelected { get => _userInventorySelected; set => SetField(ref _userInventorySelected, value, nameof(UserInventorySelected)); }

    [DataSourceProperty]
    public bool ArmorySelected { get => _armorySelected; set => SetField(ref _armorySelected, value, nameof(ArmorySelected)); }

    [DataSourceProperty]
    public MBBindingList<InventorySortTypeVM> InventorySortTypesLeft { get => _inventorySortTypesLeft; set => SetField(ref _inventorySortTypesLeft, value, nameof(InventorySortTypesLeft)); }

    [DataSourceProperty]
    public MBBindingList<InventorySortTypeVM> InventorySortTypesTop { get => _inventorySortTypesTop; set => SetField(ref _inventorySortTypesTop, value, nameof(InventorySortTypesTop)); }

    [DataSourceProperty]
    public bool IsUserInventoryButtonVisible { get => _isUserInventoryButtonVisible; set => SetField(ref _isUserInventoryButtonVisible, value, nameof(IsUserInventoryButtonVisible)); }
    [DataSourceProperty]
    public bool IsArmoryButtonVisible { get => _isArmoryButtonVisible; set => SetField(ref _isArmoryButtonVisible, value, nameof(IsArmoryButtonVisible)); }

    private readonly bool _debugOn = false;
    private void LogDebug(string message)
    {
        if (_debugOn)
        {
            LogDebug(message, Color.White);
        }
    }

    private void LogDebugError(string message)
    {
        LogDebug(message, Colors.Red);
    }

    private void LogDebug(string message, Color color)
    {
        message = $"{GetType().Name} {message}";
        Debug.Print(message);
        InformationManager.DisplayMessage(new InformationMessage(message, color));
    }
}
