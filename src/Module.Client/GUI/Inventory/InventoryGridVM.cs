using Crpg.Module.Api.Models;
using Crpg.Module.Api.Models.Items;
using Crpg.Module.Common;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.GUI.Inventory;

public class InventoryGridVM : ViewModel
{
    public enum InventorySection
    {
        UserInventory,
        Armory,
        TeamInventory,
    }

    [DataSourceProperty]
    public bool IsVisible { get; set => SetField(ref field, value, nameof(IsVisible)); }

    internal event Action<InventorySlotVM>? OnInventorySlotClicked;
    internal event Action<InventorySortTypeVM>? OnInventorySortTypeClicked;
    internal event Action<InventorySlotVM>? OnInventorySlotHoverEnd;
    internal event Action<InventorySlotVM>? OnInventorySlotDragStart;
    internal event Action<InventorySlotVM>? OnInventorySlotDragEnd;
    internal event Action<InventorySlotVM>? OnInventorySlotAlternateClicked;
    internal event Action<InventorySection>? OnInventoryChangeType;

    // Three item sources
    private readonly MBBindingList<InventorySlotVM> _inventoryItems = [];
    private readonly MBBindingList<InventorySlotVM> _armoryItems = [];
    private readonly MBBindingList<InventorySlotVM> _teamItems = [];
    private readonly CrpgClanArmoryClient? _clanArmory;
    private readonly CrpgTeamInventoryClient? _teamInventory;
    private List<InventorySortTypeVM> _activeFilters = [];

    public InventoryGridVM()
    {
        _clanArmory = Mission.Current?.GetMissionBehavior<CrpgClanArmoryClient>();
        _teamInventory = Mission.Current?.GetMissionBehavior<CrpgTeamInventoryClient>();

        _armoryItems.ListChanged += (_, _) => OnPropertyChanged(nameof(IsArmoryButtonVisible));

        InitializeSortTypes();

        ActiveSection = _teamInventory?.IsEnabled == true
            ? InventorySection.TeamInventory
            : InventorySection.UserInventory;
    }

    internal void SetInventoryItems(IEnumerable<(ItemObject item, int quantity, CrpgUserItemExtended userItemExtended)> items)
    {
        _inventoryItems.Clear();
        foreach (var slot in BuildSlots(items))
        {
            _inventoryItems.Add(slot);
        }

        if (ActiveSection == InventorySection.UserInventory)
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

        if (ActiveSection == InventorySection.Armory || ActiveSection == InventorySection.UserInventory)
        {
            RefreshDisplayedItems();
        }
    }

    internal void SetTeamInventoryItems(IEnumerable<(ItemObject item, int quantity)> items)
    {
        _teamItems.Clear();

        var slots = BuildSlots(items
            .Where(t => t.item != null)
            .Select(t => (t.item!, t.quantity, new CrpgUserItemExtended
            {
                ItemId = t.item!.StringId,
                Id = CrpgTeamInventoryClient.TeamInventoryItemId,
            })));

        foreach (var slot in slots)
        {
            _teamItems.Add(slot);
        }

        if (ActiveSection == InventorySection.TeamInventory)
        {
            RefreshDisplayedItems();
        }
    }

    internal void UpdateTeamItemQuantity(string itemId, int quantity)
    {
        var slot = _teamItems.FirstOrDefault(s => s.Id == itemId);
        if (slot != null)
        {
            slot.ItemQuantity = quantity;
            slot.IsDraggable = quantity > 0;
        }
    }

    private List<InventorySlotVM> BuildSlots(
        IEnumerable<(ItemObject item, int quantity, CrpgUserItemExtended userItemExtended)> items)
    {
        return items
            .OrderBy(t => t.item?.ItemType ?? ItemObject.ItemTypeEnum.Invalid)
            .ThenBy(t => t.item?.Name?.ToString() ?? string.Empty)
            .Select(tuple => new InventorySlotVM(
                tuple.item,
                OnItemClicked,
                OnItemHoverEnd,
                OnItemDragStart,
                OnItemDragEnd,
                OnItemAlternateClicked,
                tuple.quantity,
                tuple.userItemExtended,
                () => ActiveSection))
            .ToList();
    }

    private void InitializeSortTypes()
    {
        InventorySortTypesLeft = new MBBindingList<InventorySortTypeVM>
        {
            new("onehanded", "ui_crpg_icon_white_onehanded", slot =>
                slot.ItemObj?.ItemType == ItemObject.ItemTypeEnum.OneHandedWeapon, true, OnSortTypeClicked),
            new("twohanded", "ui_crpg_icon_white_twohanded", slot =>
                slot.ItemObj?.ItemType == ItemObject.ItemTypeEnum.TwoHandedWeapon, true, OnSortTypeClicked),
            new("polearm", "ui_crpg_icon_white_polearm", slot =>
                slot.ItemObj?.ItemType == ItemObject.ItemTypeEnum.Polearm, true, OnSortTypeClicked),
            new("shield", "ui_crpg_icon_white_shield", slot =>
                slot.ItemObj?.ItemType == ItemObject.ItemTypeEnum.Shield, true, OnSortTypeClicked),
            new("thrown", "ui_crpg_icon_white_thrown", slot =>
                slot.ItemObj?.ItemType == ItemObject.ItemTypeEnum.Thrown, true, OnSortTypeClicked),
            new("bow", "ui_crpg_icon_white_bow", slot =>
                slot.ItemObj?.ItemType == ItemObject.ItemTypeEnum.Bow, true, OnSortTypeClicked),
            new("crossbow", "ui_crpg_icon_white_crossbow",
                slot => slot.ItemObj?.ItemType == ItemObject.ItemTypeEnum.Crossbow, true, OnSortTypeClicked),
            new("gun", "ui_crpg_icon_white_musket", slot =>
                slot.ItemObj?.ItemType == ItemObject.ItemTypeEnum.Musket ||
                slot.ItemObj?.ItemType == ItemObject.ItemTypeEnum.Pistol, true, OnSortTypeClicked),
            new("ammo", "ui_crpg_icon_white_arrow", slot =>
                slot.ItemObj?.ItemType == ItemObject.ItemTypeEnum.Arrows ||
                slot.ItemObj?.ItemType == ItemObject.ItemTypeEnum.Bolts ||
                slot.ItemObj?.ItemType == ItemObject.ItemTypeEnum.Bullets, true, OnSortTypeClicked),
        };

        InventorySortTypesTop = new MBBindingList<InventorySortTypeVM>
        {
            new("headarmor", "ui_crpg_icon_white_headarmor", slot =>
                slot.ItemObj?.ItemType == ItemObject.ItemTypeEnum.HeadArmor, true, OnSortTypeClicked),
            new("cape", "ui_crpg_icon_white_cape", slot =>
                slot.ItemObj?.ItemType == ItemObject.ItemTypeEnum.Cape, true, OnSortTypeClicked),
            new("chestarmor", "ui_crpg_icon_white_chestarmor", slot =>
                slot.ItemObj?.ItemType == ItemObject.ItemTypeEnum.BodyArmor, true, OnSortTypeClicked),
            new("handarmor", "ui_crpg_icon_white_handarmor", slot =>
                slot.ItemObj?.ItemType == ItemObject.ItemTypeEnum.HandArmor, true, OnSortTypeClicked),
            new("legarmor", "ui_crpg_icon_white_legarmor", slot =>
                slot.ItemObj?.ItemType == ItemObject.ItemTypeEnum.LegArmor, true, OnSortTypeClicked),
            new("mount", "ui_crpg_icon_white_mount", slot =>
                slot.ItemObj?.ItemType == ItemObject.ItemTypeEnum.Horse, true, OnSortTypeClicked),
            new("mountarmor", "ui_crpg_icon_white_mountharness", slot =>
                slot.ItemObj?.ItemType == ItemObject.ItemTypeEnum.HorseHarness, true, OnSortTypeClicked),
            new("banner", "ui_crpg_icon_white_flag", slot =>
                slot.ItemObj?.ItemType == ItemObject.ItemTypeEnum.Banner, true, OnSortTypeClicked),
        };
    }

    private bool PassesFilters(InventorySlotVM slot)
    {
        return _activeFilters.Count == 0
            || _activeFilters.Any(f => f.Predicate?.Invoke(slot) ?? false);
    }

    private void RefreshFilteredItemsFast()
    {
        FilteredItems.Clear();
        foreach (var item in AvailableItems.Where(PassesFilters))
        {
            FilteredItems.Add(item);
        }
    }

    private void RefreshDisplayedItems()
    {
        var source = ActiveSection switch
        {
            InventorySection.Armory => _armoryItems,
            InventorySection.TeamInventory => _teamItems,
            _ => _inventoryItems,
        };

        AvailableItems.Clear();
        foreach (var item in source)
        {
            if (ShouldShowItem(item, source))
            {
                AvailableItems.Add(item);
            }
        }

        RefreshFilteredItemsFast();
    }

    private bool ShouldShowItem(InventorySlotVM item, MBBindingList<InventorySlotVM> source)
    {
        if (source != _inventoryItems || !item.IsArmoryItem)
        {
            return true;
        }

        return item.UserItemEx?.Id is not null
            && _clanArmory?.GetCrpgUserItemArmoryStatus(item.UserItemEx.Id, out var status) == true
            && status == CrpgGameArmoryItemStatus.BorrowedByYou;
    }

    // ===== handlers for UI actions =====
    // from this VM
    private void ExecuteClickArmory() => ActiveSection = InventorySection.Armory;
    private void ExecuteClickUserInventory() => ActiveSection = InventorySection.UserInventory;
    private void ExecuteClickTeamInventory() => ActiveSection = InventorySection.TeamInventory;

    // Events from other VMs
    private void OnItemClicked(InventorySlotVM? slot)
    {
        if (slot?.ItemObj != null)
        {
            OnInventorySlotClicked?.Invoke(slot);
        }
    }

    private void OnItemAlternateClicked(InventorySlotVM? slot)
    {
        if (slot?.ItemObj != null)
        {
            OnInventorySlotAlternateClicked?.Invoke(slot);
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

    private void OnSortTypeClicked(InventorySortTypeVM clickedSort)
    {
        // rebuild active filters
        _activeFilters = InventorySortTypesLeft
            .Concat(InventorySortTypesTop)
            .Where(f => f.IsSelected)
            .ToList();

        RefreshFilteredItemsFast();
        OnInventorySortTypeClicked?.Invoke(clickedSort);
    }

    [DataSourceProperty]
    public MBBindingList<InventorySlotVM> AvailableItems { get; set => SetField(ref field, value, nameof(AvailableItems)); } = [];
    [DataSourceProperty]
    public MBBindingList<InventorySlotVM> FilteredItems { get; set => SetField(ref field, value, nameof(FilteredItems)); } = [];
    [DataSourceProperty]
    public bool UserInventorySelected => ActiveSection == InventorySection.UserInventory;
    [DataSourceProperty]
    public bool ArmorySelected => ActiveSection == InventorySection.Armory;
    [DataSourceProperty]
    public bool TeamInventorySelected => ActiveSection == InventorySection.TeamInventory;
    [DataSourceProperty]
    public MBBindingList<InventorySortTypeVM> InventorySortTypesLeft { get; set => SetField(ref field, value, nameof(InventorySortTypesLeft)); } = [];
    [DataSourceProperty]
    public MBBindingList<InventorySortTypeVM> InventorySortTypesTop { get; set => SetField(ref field, value, nameof(InventorySortTypesTop)); } = [];
    [DataSourceProperty]
    public bool IsTeamInventoryButtonVisible => _teamInventory?.IsEnabled ?? false;

    [DataSourceProperty]
    public bool IsUserInventoryButtonVisible => !(_teamInventory?.IsEnabled ?? false);
    [DataSourceProperty]
    public bool IsArmoryButtonVisible => _armoryItems.Count > 0;
    [DataSourceProperty]
    public string UserInventoryButtonText { get; set => SetField(ref field, value, nameof(UserInventoryButtonText)); }
        = new TextObject("{=KC9dx136}User Inventory").ToString();
    [DataSourceProperty]
    public string ArmoryInventoryButtonText { get; set => SetField(ref field, value, nameof(ArmoryInventoryButtonText)); }
        = new TextObject("{=KC9dx137}Armory").ToString();
    [DataSourceProperty]
    public string TeamInventoryButtonText { get; set => SetField(ref field, value, nameof(TeamInventoryButtonText)); }
        = new TextObject("{=KC9dx138}Team Inventory").ToString();
    [DataSourceProperty]
    public InventorySection ActiveSection
    {
        get;
        set
        {
            if (SetField(ref field, value, nameof(ActiveSection)))
            {
                OnPropertyChanged(nameof(UserInventorySelected));
                OnPropertyChanged(nameof(ArmorySelected));
                OnPropertyChanged(nameof(TeamInventorySelected));

                RefreshDisplayedItems();
                OnInventoryChangeType?.Invoke(value);
            }
        }
    }
}
