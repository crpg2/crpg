using Crpg.Module.Api.Models.Items;
using Crpg.Module.Common.Network;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace Crpg.Module.GUI.Inventory;

internal class CrpgInventoryVm : ViewModel
{
    private static readonly EquipmentIndex[] AllSlots =
    [
        EquipmentIndex.Head, EquipmentIndex.Cape, EquipmentIndex.Body, EquipmentIndex.Gloves,
        EquipmentIndex.Leg, EquipmentIndex.Horse, EquipmentIndex.HorseHarness,
        EquipmentIndex.Weapon0, EquipmentIndex.Weapon1, EquipmentIndex.Weapon2, EquipmentIndex.Weapon3,
        EquipmentIndex.ExtraWeaponSlot,
    ];

    /// <summary>Maps <see cref="ItemObject.ItemTypeEnum"/> to the unique <see cref="EquipmentIndex"/> for non-weapon items.</summary>
    private static readonly Dictionary<ItemObject.ItemTypeEnum, EquipmentIndex> ItemTypeToSlot = new()
    {
        [ItemObject.ItemTypeEnum.HeadArmor] = EquipmentIndex.Head,
        [ItemObject.ItemTypeEnum.Cape] = EquipmentIndex.Cape,
        [ItemObject.ItemTypeEnum.BodyArmor] = EquipmentIndex.Body,
        [ItemObject.ItemTypeEnum.HandArmor] = EquipmentIndex.Gloves,
        [ItemObject.ItemTypeEnum.LegArmor] = EquipmentIndex.Leg,
        [ItemObject.ItemTypeEnum.Horse] = EquipmentIndex.Horse,
        [ItemObject.ItemTypeEnum.HorseHarness] = EquipmentIndex.HorseHarness,
    };

    private readonly List<CrpgInventoryItemVm> _allItems = new();
    private readonly Dictionary<EquipmentIndex, CrpgInventorySlotVm> _slotsByType = new();

    private static readonly EquipmentIndex[] ArmorSlotOrder =
    [
        EquipmentIndex.Head, EquipmentIndex.Cape, EquipmentIndex.Body, EquipmentIndex.Gloves, EquipmentIndex.Leg,
        EquipmentIndex.Horse, EquipmentIndex.HorseHarness,
    ];

    private static readonly EquipmentIndex[] WeaponSlotOrder =
    [
        EquipmentIndex.Weapon0, EquipmentIndex.Weapon1, EquipmentIndex.Weapon2, EquipmentIndex.Weapon3,
        EquipmentIndex.ExtraWeaponSlot,
    ];

    private MBBindingList<CrpgInventoryItemVm> _filteredItems = new();
    private MBBindingList<CrpgInventorySlotVm> _equipmentSlots = new();
    private MBBindingList<CrpgInventorySlotVm> _armorSlots = new();
    private MBBindingList<CrpgInventorySlotVm> _weaponSlots = new();
    private string _searchText = string.Empty;
    private int _selectedTypeFilter; // 0 = All
    private bool _isVisible;
    private bool _isAlive;
    private string _itemCountText = string.Empty;

    public CrpgInventoryVm()
    {
        foreach (var slot in AllSlots)
        {
            var slotVm = new CrpgInventorySlotVm(slot) { OnUnequipRequested = OnUnequipSlot };
            _slotsByType[slot] = slotVm;
            _equipmentSlots.Add(slotVm);
        }

        foreach (var slot in ArmorSlotOrder)
        {
            _armorSlots.Add(_slotsByType[slot]);
        }

        foreach (var slot in WeaponSlotOrder)
        {
            _weaponSlots.Add(_slotsByType[slot]);
        }
    }

    [DataSourceProperty]
    public MBBindingList<CrpgInventoryItemVm> FilteredItems
    {
        get => _filteredItems;
        set
        {
            _filteredItems = value;
            OnPropertyChangedWithValue(value);
        }
    }

    [DataSourceProperty]
    public MBBindingList<CrpgInventorySlotVm> EquipmentSlots
    {
        get => _equipmentSlots;
        set
        {
            _equipmentSlots = value;
            OnPropertyChangedWithValue(value);
        }
    }

    [DataSourceProperty]
    public MBBindingList<CrpgInventorySlotVm> ArmorSlots
    {
        get => _armorSlots;
        set
        {
            _armorSlots = value;
            OnPropertyChangedWithValue(value);
        }
    }

    [DataSourceProperty]
    public MBBindingList<CrpgInventorySlotVm> WeaponSlots
    {
        get => _weaponSlots;
        set
        {
            _weaponSlots = value;
            OnPropertyChangedWithValue(value);
        }
    }

    [DataSourceProperty]
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText != value)
            {
                _searchText = value;
                OnPropertyChangedWithValue(value);
                RefreshFilteredItems();
            }
        }
    }

    [DataSourceProperty]
    public int SelectedTypeFilter
    {
        get => _selectedTypeFilter;
        set
        {
            if (_selectedTypeFilter != value)
            {
                _selectedTypeFilter = value;
                OnPropertyChangedWithValue(value);
                RefreshFilteredItems();
            }
        }
    }

    [DataSourceProperty]
    public bool IsVisible
    {
        get => _isVisible;
        set
        {
            _isVisible = value;
            OnPropertyChangedWithValue(value);
        }
    }

    [DataSourceProperty]
    public bool IsAlive
    {
        get => _isAlive;
        set
        {
            _isAlive = value;
            OnPropertyChangedWithValue(value);
        }
    }

    [DataSourceProperty]
    public string ItemCountText
    {
        get => _itemCountText;
        set
        {
            _itemCountText = value;
            OnPropertyChangedWithValue(value);
        }
    }

    public void PopulateItems(IList<CrpgOwnedItem> items)
    {
        _allItems.Clear();
        foreach (var ownedItem in items)
        {
            var itemVm = new CrpgInventoryItemVm(ownedItem.Item, ownedItem.Rank, ownedItem.IsBroken)
            {
                AutoEquipAction = ExecuteAutoEquip,
            };
            _allItems.Add(itemVm);
        }

        RefreshFilteredItems();
    }

    public void RefreshEquippedState(IList<CrpgEquippedItem> equippedItems)
    {
        // Clear all slots.
        foreach (var slotVm in _slotsByType.Values)
        {
            slotVm.Item = null;
        }

        // Resolve equipped item IDs to ItemObjects and mark state.
        HashSet<ItemObject> equippedObjects = new();
        foreach (var equipped in equippedItems)
        {
            var itemObject = MBObjectManager.Instance.GetObject<ItemObject>(equipped.UserItem.ItemId);
            if (itemObject == null)
            {
                continue;
            }

            equippedObjects.Add(itemObject);
            if (_slotsByType.TryGetValue(equipped.Slot.ToIndex(), out var slotVm))
            {
                // Item not in the inventory list (edge case). Create a display-only VM.
                var itemVm = _allItems.Find(i => i.Item == itemObject)
                             ?? new CrpgInventoryItemVm(itemObject, equipped.UserItem.Rank, equipped.UserItem.IsBroken)
                             {
                                 IsEquipped = true,
                             };

                slotVm.Item = itemVm;
            }
        }

        foreach (var itemVm in _allItems)
        {
            itemVm.IsEquipped = equippedObjects.Contains(itemVm.Item);
        }
    }

    /// <summary>Finds an item by MBGUID and auto-equips it. Called via static event from the widget layer.</summary>
    public void HandleAutoEquip(ItemObject item)
    {
        var itemVm = _allItems.Find(i => i.Item == item);
        if (itemVm != null)
        {
            ExecuteAutoEquip(itemVm);
        }
    }

    /// <summary>Auto-equips armor/mount to the unique slot. No-op for weapons (slot is ambiguous).</summary>
    public void ExecuteAutoEquip(CrpgInventoryItemVm itemVm)
    {
        if (!ItemTypeToSlot.TryGetValue(itemVm.Item.ItemType, out var slot))
        {
            return; // Weapons: no auto-equip, must drag.
        }

        SendEquipRequest(itemVm.Item, slot);
    }

    /// <summary>Called when an item is dropped on a slot widget.</summary>
    public void EquipToSlot(ItemObject item, EquipmentIndex slot)
    {
        SendEquipRequest(item, slot);
    }

    public void ExecuteClose()
    {
        IsVisible = false;
    }

    public void ExecuteFilterAll()
    {
        SelectedTypeFilter = 0;
    }

    public void ExecuteFilterArmor()
    {
        SelectedTypeFilter = 1;
    }

    public void ExecuteFilterWeapons()
    {
        SelectedTypeFilter = 2;
    }

    public void ExecuteFilterMount()
    {
        SelectedTypeFilter = 3;
    }

    private void OnUnequipSlot(CrpgInventorySlotVm slotVm)
    {
        SendEquipRequest(null, slotVm.Slot);
    }

    private void SendEquipRequest(ItemObject? item, EquipmentIndex slot)
    {
        if (!GameNetwork.IsClient)
        {
            return;
        }

        GameNetwork.BeginModuleEventAsClient();
        GameNetwork.WriteMessage(new RequestEquipItem { Item = item, Slot = slot });
        GameNetwork.EndModuleEventAsClient();
    }

    private void RefreshFilteredItems()
    {
        _filteredItems.Clear();
        foreach (var item in _allItems)
        {
            if (!MatchesFilter(item))
            {
                continue;
            }

            _filteredItems.Add(item);
        }

        ItemCountText = $"{_filteredItems.Count} / {_allItems.Count} items";
    }

    private bool MatchesFilter(CrpgInventoryItemVm item)
    {
        // Text search.
        if (!string.IsNullOrEmpty(_searchText)
            && item.Name.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) < 0)
        {
            return false;
        }

        // Type filter.
        if (_selectedTypeFilter == 0)
        {
            return true;
        }

        return _selectedTypeFilter switch
        {
            1 => IsArmorType(item.Item.ItemType),
            2 => IsWeaponType(item.Item.ItemType),
            3 => item.Item.ItemType is ItemObject.ItemTypeEnum.Horse or ItemObject.ItemTypeEnum.HorseHarness,
            _ => true,
        };
    }

    private static bool IsArmorType(ItemObject.ItemTypeEnum type) =>
        type is ItemObject.ItemTypeEnum.HeadArmor
            or ItemObject.ItemTypeEnum.BodyArmor
            or ItemObject.ItemTypeEnum.LegArmor
            or ItemObject.ItemTypeEnum.HandArmor
            or ItemObject.ItemTypeEnum.Cape;

    private static bool IsWeaponType(ItemObject.ItemTypeEnum type) =>
        type is ItemObject.ItemTypeEnum.OneHandedWeapon
            or ItemObject.ItemTypeEnum.TwoHandedWeapon
            or ItemObject.ItemTypeEnum.Polearm
            or ItemObject.ItemTypeEnum.Bow
            or ItemObject.ItemTypeEnum.Crossbow
            or ItemObject.ItemTypeEnum.Thrown
            or ItemObject.ItemTypeEnum.Arrows
            or ItemObject.ItemTypeEnum.Bolts
            or ItemObject.ItemTypeEnum.Shield;
}
