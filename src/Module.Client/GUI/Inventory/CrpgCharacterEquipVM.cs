using Crpg.Module.Api.Models;
using Crpg.Module.Api.Models.Characters;
using Crpg.Module.Api.Models.Items;
using Crpg.Module.Common;
using Crpg.Module.Common.Network.Armory;
using Crpg.Module.GUI.Notifications;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using static Crpg.Module.GUI.Inventory.InventoryGridVM;

namespace Crpg.Module.GUI.Inventory;

public class CrpgCharacterEquipVM : ViewModel
{
    internal event Action<ViewModel>? OnCloseButtonClicked;
    internal event Action<ViewModel>? OnCloseButtonAlternateClicked;
    internal event Action<ViewModel>? OnSelectedReadyClicked;
    private readonly CrpgCharacterLoadoutBehaviorClient? _userLoadout;
    private readonly CrpgClanArmoryClient? _clanArmory;
    private readonly CrpgTeamInventoryClient? _teamInventory;
    private readonly bool _isTeamInventoryEnabled;

    [DataSourceProperty]
    public InventoryGridVM InventoryGrid { get; set => SetField(ref field, value, nameof(InventoryGrid)); }

    [DataSourceProperty]
    public CharacterEquipNavBar NavBar { get; set => SetField(ref field, value, nameof(NavBar)); }

    [DataSourceProperty]
    public ItemInfoVM ItemInfo { get; set => SetField(ref field, value, nameof(ItemInfo)); }

    [DataSourceProperty]
    public EquipmentPanelVM EquipmentPanel { get; set => SetField(ref field, value, nameof(EquipmentPanel)); }

    [DataSourceProperty]
    public CharacteristicsEditorVM CharacteristicsEditor { get; set => SetField(ref field, value, nameof(CharacteristicsEditor)); }

    [DataSourceProperty]
    public CharacterInfoBuildEquipStatsVM CharacterInfoBuildEquipStatsVm { get; set => SetField(ref field, value, nameof(CharacterInfoBuildEquipStatsVm)); }

    [DataSourceProperty]
    public bool IsSpawnButtonVisible { get; set => SetField(ref field, value, nameof(IsSpawnButtonVisible)); }

    [DataSourceProperty]
    public bool IsVisible
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                OnPropertyChangedWithValue(value, nameof(IsVisible));
            }
        }
    }

    [DataSourceProperty]
    public string CharacterName { get; set => SetField(ref field, value, nameof(CharacterName)); } = string.Empty;

    [DataSourceProperty]
    public string UserName { get; set => SetField(ref field, value, nameof(UserName)); } = string.Empty;

    [DataSourceProperty]
    public string GoldAmount { get; set => SetField(ref field, value, nameof(GoldAmount)); } = string.Empty;

    [DataSourceProperty]
    public string SelectEquipAndSpawnButton { get; set => SetField(ref field, value, nameof(SelectEquipAndSpawnButton)); }
        = new TextObject("{=KC9dx130}Select Equipment and Spawn").ToString();

    public CrpgCharacterEquipVM()
    {
        _userLoadout = Mission.Current?.GetMissionBehavior<CrpgCharacterLoadoutBehaviorClient>();
        _clanArmory = Mission.Current?.GetMissionBehavior<CrpgClanArmoryClient>();
        _teamInventory = Mission.Current?.GetMissionBehavior<CrpgTeamInventoryClient>();
        _isTeamInventoryEnabled = _teamInventory?.IsEnabled ?? false;
        NavBar = new CharacterEquipNavBar();
        CharacteristicsEditor = new CharacteristicsEditorVM();
        CharacterInfoBuildEquipStatsVm = new CharacterInfoBuildEquipStatsVM();
        EquipmentPanel = new EquipmentPanelVM();
        InventoryGrid = new InventoryGridVM();
        ItemInfo = new ItemInfoVM(null);
        EquipmentPanel.InitializeCharacterPreview();

        SelectNavBarTab(NavBarTab.Equipment);

        NavBar.OnEquipNavButtonClicked += HandleEquipNavButtonClicked;
        CharacteristicsEditor.OnEditCharacteristicsChanged += UpdateCharacterBuildEquipmentStatDisplayFromNavbarSelection;
        EquipmentPanel.OnItemDropped += HandleItemDrop;
        EquipmentPanel.OnItemDragBegin += HandleItemDragBegin;
        EquipmentPanel.OnItemDragEnd += HandleItemDragEnd;
        EquipmentPanel.OnSlotAlternateClicked += HandleAlternateClick;
        EquipmentPanel.OnSlotClicked += HandleClick;
        InventoryGrid.OnInventorySlotClicked += HandleClick;
        InventoryGrid.OnInventorySlotHoverEnd += HandleInventorySlotHoverEnd;
        InventoryGrid.OnInventorySlotDragStart += HandleItemDragBegin;
        InventoryGrid.OnInventorySlotDragEnd += HandleItemDragEnd;
        InventoryGrid.OnInventoryChangeType += HandleInventoryChangeType;
        InventoryGrid.OnInventorySlotAlternateClicked += HandleAlternateClick;

        if (_userLoadout != null)
        {
            // Always subscribe regardless of mode
            _userLoadout.OnUserCharacterBasicUpdated += HandleUserCharacterBasicUpdated;
            _userLoadout.OnUserCharacteristicsUpdated += HandleUserCharacteristicsUpdated;
            _userLoadout.OnUserCharacteristicsConverted += HandleUserCharacteristicsConverted;
            _userLoadout.OnUserInfoUpdated += HandleUserInfoUpdated;

            if (_userLoadout.UserCharacter != null)
            {
                HandleUserCharacterBasicUpdated();
            }

            if (_userLoadout.User != null)
            {
                HandleUserInfoUpdated();
            }

            MissionPeer? missionPeer = GameNetwork.MyPeer.GetComponent<MissionPeer>();
            CrpgPeer? crpgPeer = missionPeer?.GetComponent<CrpgPeer>();
            string clanTag = crpgPeer?.Clan?.Tag ?? string.Empty;
            if (clanTag.Length > 0)
            {
                clanTag = $"[{clanTag}]";
            }

            if (!_isTeamInventoryEnabled)
            {
                _userLoadout?.OnEquipmentSlotUpdated += HandleEquipmentSlotUpdated;
                _userLoadout?.OnUserInventoryUpdated += HandleInventoryUpdated;
                _userLoadout?.OnUserCharacterEquippedItemsUpdated += HandleEquippedItemsUpdated;
            }
        }

        if (!_isTeamInventoryEnabled && _clanArmory is not null)
        {
            _clanArmory.OnArmoryActionUpdated += HandleArmoryUserItemUpdated;
            _clanArmory.OnClanArmoryUpdated += HandleClanArmoryUpdated;
        }

        if (_isTeamInventoryEnabled && _teamInventory is not null)
        {
            _teamInventory.OnTeamItemsUpdated += HandleTeamInventoryUpdated;
            _teamInventory.OnSingleTeamItemQuantityUpdated += HandleTeamItemQuantityChanged;
            _teamInventory.OnEquippedTeamItemsUpdated += HandleEquippedTeamItemsUpdated;

            HandleTeamInventoryUpdated();
        }

        RefreshSpawnButtonVisibility();
    }

    public override void OnFinalize()
    {
        base.OnFinalize();

        NavBar.OnEquipNavButtonClicked -= HandleEquipNavButtonClicked;
        CharacteristicsEditor.OnEditCharacteristicsChanged -= UpdateCharacterBuildEquipmentStatDisplayFromNavbarSelection;
        EquipmentPanel.OnItemDropped -= HandleItemDrop;
        EquipmentPanel.OnItemDragBegin -= HandleItemDragBegin;
        EquipmentPanel.OnItemDragEnd -= HandleItemDragEnd;
        EquipmentPanel.OnSlotAlternateClicked -= HandleAlternateClick;
        EquipmentPanel.OnSlotClicked -= HandleClick;
        InventoryGrid.OnInventorySlotClicked -= HandleClick;
        InventoryGrid.OnInventorySlotHoverEnd -= HandleInventorySlotHoverEnd;
        InventoryGrid.OnInventorySlotDragStart -= HandleItemDragBegin;
        InventoryGrid.OnInventorySlotDragEnd -= HandleItemDragEnd;
        InventoryGrid.OnInventoryChangeType -= HandleInventoryChangeType;
        InventoryGrid.OnInventorySlotAlternateClicked -= HandleAlternateClick;

        if (_userLoadout != null)
        {
            _userLoadout.OnUserCharacterBasicUpdated -= HandleUserCharacterBasicUpdated;
            _userLoadout.OnUserCharacteristicsUpdated -= HandleUserCharacteristicsUpdated;
            _userLoadout.OnUserCharacteristicsConverted -= HandleUserCharacteristicsConverted;
            _userLoadout.OnUserInfoUpdated -= HandleUserInfoUpdated;

            if (!_isTeamInventoryEnabled)
            {
                _userLoadout.OnEquipmentSlotUpdated -= HandleEquipmentSlotUpdated;
                _userLoadout.OnUserInventoryUpdated -= HandleInventoryUpdated;
                _userLoadout.OnUserCharacterEquippedItemsUpdated -= HandleEquippedItemsUpdated;
            }
        }

        if (!_isTeamInventoryEnabled && _clanArmory is not null)
        {
            _clanArmory.OnClanArmoryUpdated -= HandleClanArmoryUpdated;
            _clanArmory.OnArmoryActionUpdated -= HandleArmoryUserItemUpdated;
        }

        if (_isTeamInventoryEnabled && _teamInventory is not null)
        {
            _teamInventory.OnTeamItemsUpdated -= HandleTeamInventoryUpdated;
            _teamInventory.OnSingleTeamItemQuantityUpdated -= HandleTeamItemQuantityChanged;
            _teamInventory.OnEquippedTeamItemsUpdated -= HandleEquippedTeamItemsUpdated;
        }
    }

    internal void SelectNavBarTab(NavBarTab tab)
    {
        NavBar.ActiveTab = tab;
        bool isEquipment = tab == NavBarTab.Equipment;
        EquipmentPanel.IsVisible = isEquipment;
        InventoryGrid.IsVisible = isEquipment;
        CharacteristicsEditor.IsVisible = !isEquipment;
        UpdateCharacterBuildEquipmentStatDisplayFromNavbarSelection();
        RefreshSpawnButtonVisibility();
    }

    /// <summary>
    /// Updates IsSpawnButtonVisible based on the active mode (team inventory vs user
    /// loadout), whether the player is currently alive, and which nav bar tab is shown.
    /// Call this whenever any of those states change.
    /// </summary>
    internal void RefreshSpawnButtonVisibility()
    {
        var missionPeer = GameNetwork.MyPeer?.GetComponent<MissionPeer>();
        bool isAlive = missionPeer?.ControlledAgent != null && missionPeer.ControlledAgent.IsActive();
        bool isEquipmentTabSelected = NavBar.ActiveTab == NavBarTab.Equipment;

        if (_teamInventory?.IsEnabled == true)
        {
            IsSpawnButtonVisible = !isAlive && isEquipmentTabSelected;
            return;
        }

        if (_userLoadout?.IsEnabled == true && _userLoadout?.IsReadyToSpawn != true)
        {
            IsSpawnButtonVisible = isEquipmentTabSelected;
            return;
        }

        IsSpawnButtonVisible = false;
        return;
    }

    /// <summary>
    /// Fires a status message for each item in LastEquippedItems that is no longer
    /// available in the team pool. Only meaningful in team inventory mode; no-ops otherwise.
    /// </summary>
    internal void NotifyUnavailableLastEquippedItems()
    {
        if (_teamInventory == null || !_isTeamInventoryEnabled)
        {
            return;
        }

        var unavailable = _teamInventory.GetUnavailableLastEquippedItems();
        foreach (string itemId in unavailable)
        {
            var itemObj = MBObjectManager.Instance.GetObject<ItemObject>(itemId);
            string itemName = itemObj?.Name?.ToString() ?? itemId;
            CrpgHudNotificationUiHandler.AddToLog("statuslog", new CrpgHudNotificationOptions
            {
                Text = new TextObject("{=KC9dx129}{ITEM_NAME} is no longer available.").SetTextVariable("ITEM_NAME", itemName).ToString(),
                Color = Colors.Red,
                Duration = 8f,
            });
        }

        if (unavailable.Count > 0)
        {
            SoundEvent.PlaySound2D("event:/ui/notification/alert");
        }
    }

    private void ApplyEquipmentState(
        Equipment equipment,
        IReadOnlyList<CrpgEquippedItemExtended> items,
        CrpgCharacterCharacteristics characteristics)
    {
        EquipmentPanel.SetState(equipment, items);
        CharacterInfoBuildEquipStatsVm.UpdateCharacterBuildEquipmentStatDisplay(equipment, characteristics);
    }

    private void ApplyEquipmentSlotUpdate(
        CrpgItemSlot slot,
        Equipment equipment,
        IReadOnlyList<CrpgEquippedItemExtended> items,
        CrpgCharacterCharacteristics characteristics)
    {
        var equippedItem = items.FirstOrDefault(e => e.Slot == slot);
        EquipmentPanel.UpdateSlotAndVisuals(slot, equippedItem, equipment);
        CharacterInfoBuildEquipStatsVm.UpdateCharacterBuildEquipmentStatDisplay(equipment, characteristics);
    }

    private void ShowItemInfoPopup(bool show, ItemObject itemObj, int userItemId = -1)
    {
        if (!show)
        {
            ItemInfo.IsVisible = false;
            return;
        }
        else
        {
            var mousePos = Input.MousePositionPixel;
            ItemInfo.PositionX = mousePos.X + 25;
            ItemInfo.PositionY = mousePos.Y - 100;
            ItemInfo.GenerateItemInfo(itemObj, userItemId);
            ItemInfo.IsVisible = true;
        }
    }

    // ===== handlers for UI actions =====
    // from this VM
    private void ExecuteDropDiscard(ViewModel draggedItem, int index)
    {
        if (draggedItem is EquipmentSlotVM slot)
        {
            // Team inventory Item only request to equip from teamInventory Server, not API, so we handle it differently here
            if (_teamInventory is not null && slot.IsTeamInventoryItem)
            {
                _teamInventory.RequestEquipTeamItem(slot.CrpgItemSlotIndex, slot.UserItemEx?.ItemId ?? string.Empty, false);
                return;
            }

            if (_userLoadout is not null)
            {
                _userLoadout.RequestEquipCharacterItem(slot.CrpgItemSlotIndex, -1); // -1 to unequip
            }
        }
    }

    private void CloseButtonClicked()
    {
        OnCloseButtonClicked?.Invoke(this);
    }

    private void CloseButtonAlternateClicked()
    {
        OnCloseButtonAlternateClicked?.Invoke(this);
    }

    private void SelectedReadyClicked()
    {
        OnSelectedReadyClicked?.Invoke(this);
    }

    // Events from other VMs
    private void HandleInventoryChangeType(InventorySection type)
    {
        if (type == InventorySection.TeamInventory)
        {
            _teamInventory?.RequestGetTeamItemsList();
            HandleEquippedTeamItemsUpdated();
        }
        else if (_teamInventory?.IsEnabled != true)
        {
            HandleEquippedItemsUpdated();
        }

        RefreshSpawnButtonVisibility();
    }

    private void HandleInventorySlotHoverEnd(InventorySlotVM slot)
    {
        if (ItemInfo.IsVisible)
        {
            // ItemInfo.IsVisible = false;
        }
    }

    private void HandleItemDrop(EquipmentSlotVM targetSlot, ViewModel draggedItem)
    {
        int userItemId = 0;
        ItemObject draggedItemObject;
        EquipmentSlotVM? sourceEquipmentSlot = null;

        // Determine source item and type
        if (draggedItem is InventorySlotVM inv)
        {
            userItemId = inv.UserItemId;
            draggedItemObject = inv.ItemObj;
        }
        else if (draggedItem is EquipmentSlotVM eq)
        {
            // Prevent dropping onto the same slot or empty item
            if (eq.CrpgItemSlotIndex == targetSlot.CrpgItemSlotIndex || eq.ItemObj == null)
            {
                return;
            }

            userItemId = eq.UserItemId;
            draggedItemObject = eq.ItemObj;
            sourceEquipmentSlot = eq;
        }
        else
        {
            Debug.Print("Dropped unknown item type");
            return;
        }

        if (draggedItemObject == null)
        {
            Debug.Print("No Object to drag");
            return;
        }

        // Prevent equipping an item that does not fit the target slot
        if (!Equipment.IsItemFitsToSlot(EquipmentSlotVM.ConvertToEquipmentIndex(targetSlot.CrpgItemSlotIndex), draggedItemObject))
        {
            return;
        }

        // Prevent equipping the same item into the same slot
        if (draggedItemObject == targetSlot.ItemObj && userItemId == targetSlot.UserItemId)
        {
            return;
        }

        // Team inventory Item only request to equip from teamInventory Server, not API, so we handle it differently here
        if (_teamInventory is not null && CrpgTeamInventoryClient.IsTeamInventoryItem(userItemId))
        {
            _teamInventory.RequestEquipTeamItem(targetSlot.CrpgItemSlotIndex, draggedItemObject.StringId, true);
            return;
        }

        // Send equip request to Server/API
        if (_userLoadout is not null)
        {
            _userLoadout.RequestEquipCharacterItem(targetSlot.CrpgItemSlotIndex, userItemId);

            // Clear the source slot only if different from target
            if (sourceEquipmentSlot != null && sourceEquipmentSlot.CrpgItemSlotIndex != targetSlot.CrpgItemSlotIndex)
            {
                _userLoadout.RequestEquipCharacterItem(sourceEquipmentSlot.CrpgItemSlotIndex, -1); // -1 to unequip
            }
        }
    }

    private void HandleItemDragBegin(ViewModel viewModel)
    {
        ItemObject? itemObj = viewModel switch
        {
            InventorySlotVM inv => inv.ItemObj,
            EquipmentSlotVM eq => eq.ItemObj,
            _ => null,
        };

        if (itemObj == null)
        {
            if (viewModel is not InventorySlotVM && viewModel is not EquipmentSlotVM)
            {
                Debug.Print($"HandleItemDragBegin: unexpected ViewModel type {viewModel?.GetType().Name}");
            }

            return;
        }

        foreach (var slotVm in EquipmentPanel.EquipmentSlots)
        {
            if (!Equipment.IsItemFitsToSlot(EquipmentSlotVM.ConvertToEquipmentIndex(slotVm.CrpgItemSlotIndex), itemObj))
            {
                slotVm.IsButtonEnabled = false;
            }
        }
    }

    private void HandleItemDragEnd(ViewModel viewModel)
    {
        foreach (var slotVm in EquipmentPanel.EquipmentSlots)
        {
            slotVm.IsButtonEnabled = true;
        }
    }

    private void HandleAlternateClick(EquipmentSlotVM slot)
    {
        // Team inventory Item only request to equip from teamInventory Server, not API, so we handle it differently here
        if (_teamInventory is not null && slot.IsTeamInventoryItem)
        {
            _teamInventory.RequestEquipTeamItem(slot.CrpgItemSlotIndex, slot.UserItemEx?.ItemId ?? string.Empty, false);
            return;
        }

        if (_userLoadout is not null)
        {
            _userLoadout.RequestEquipCharacterItem(slot.CrpgItemSlotIndex, -1);
        }
    }

    private void HandleAlternateClick(InventorySlotVM slot)
    {
        if (slot.ItemObj == null)
        {
            return;
        }

        // prevent equipping armory items that are not borrowed by you
        if (slot.IsArmoryItem && slot.UserItemEx is not null && _clanArmory is not null)
        {
            if (_clanArmory.GetCrpgUserItemArmoryStatus(slot.UserItemEx.Id, out var status))
            {
                if (status != CrpgGameArmoryItemStatus.BorrowedByYou)
                {
                    return;
                }
            }
        }

        if (_teamInventory is not null && _teamInventory.IsEnabled && CrpgTeamInventoryClient.IsTeamInventoryItem(slot.UserItemId))
        {
            var targetSlot = FindFirstValidAvailableSlot(slot.ItemObj, slot.UserItemId);
            if (targetSlot != null)
            {
                _teamInventory.RequestEquipTeamItem(targetSlot.CrpgItemSlotIndex, slot.ItemObj.StringId, true);
            }

            return;
        }

        var firstValidSlot = FindFirstValidAvailableSlot(slot.ItemObj, slot.UserItemId);

        if (firstValidSlot != null)
        {
            if (_userLoadout is not null)
            {
                _userLoadout.RequestEquipCharacterItem(firstValidSlot.CrpgItemSlotIndex, slot.UserItemId);
            }
        }
    }

    private void HandleClick(ViewModel viewModel)
    {
        switch (viewModel)
        {
            case EquipmentSlotVM eq when eq.ItemObj != null:
                ShowItemInfoPopup(true, eq.ItemObj, eq.UserItemId);
                break;

            case InventorySlotVM inv when inv.ItemObj != null:
                ShowItemInfoPopup(true, inv.ItemObj, inv.UserItemId);
                break;

            case InventorySortTypeVM sortVm:
                break;
            default:
                // Debug.Print("Clicked unknown ViewModel type");
                break;
        }
    }

    private (Equipment equipment, IReadOnlyList<CrpgEquippedItemExtended> items) GetCurrentEquipmentState()
    {
        if (_isTeamInventoryEnabled && _teamInventory != null)
        {
            return (
                _teamInventory.GetSelectedTeamInventoryEquipment(),
                _teamInventory.EquippedItems);
        }

        if (_userLoadout != null)
        {
            return (
                _userLoadout.GetCrpgUserCharacterEquipment(),
                _userLoadout.EquippedItems);
        }

        return (new Equipment(), []);
    }

    private CrpgCharacterCharacteristics GetCurrentCharacteristics() =>
    _userLoadout?.UserCharacter.Characteristics ?? new CrpgCharacterCharacteristics();

    private EquipmentSlotVM? FindFirstValidAvailableSlot(ItemObject itemObj, int excludeUserItemId)
    {
        return EquipmentPanel.EquipmentSlots
            .Where(s => Equipment.IsItemFitsToSlot(
                EquipmentSlotVM.ConvertToEquipmentIndex(s.CrpgItemSlotIndex), itemObj)
                && s.UserItemId != excludeUserItemId)
            .FirstOrDefault(s => IsWeaponSlot(s.CrpgItemSlotIndex)
                ? s.ItemObj == null
                : true);
    }

    private static bool IsWeaponSlot(CrpgItemSlot slot) =>
        slot is CrpgItemSlot.Weapon0
            or CrpgItemSlot.Weapon1
            or CrpgItemSlot.Weapon2
            or CrpgItemSlot.Weapon3;

    // Gui Updates because of _userLoadout (API usage) or _teamInventory
    private void HandleEquipmentSlotUpdated(CrpgItemSlot updatedSlot)
    {
        if (_userLoadout == null)
        {
            return;
        }

        var (equipment, items) = GetCurrentEquipmentState();
        ApplyEquipmentSlotUpdate(updatedSlot, equipment, items, GetCurrentCharacteristics());
    }

    private void HandleInventoryUpdated()
    {
        if (_userLoadout == null)
        {
            Debug.Print("No CrpgCharacterLoadoutBehaviorClient found");
            return;
        }

        var items = _userLoadout.UserInventoryItems
            .Where(ui => !string.IsNullOrEmpty(ui.ItemId))
            .Select(ui => (MBObjectManager.Instance.GetObject<ItemObject>(ui.ItemId), 1, ui))
            .Where(t => t.Item1 != null)
            .ToList();

        InventoryGrid.SetInventoryItems(items);

        OnPropertyChanged(nameof(InventoryGrid));
    }

    private void HandleClanArmoryUpdated()
    {
        if (_userLoadout == null)
        {
            Debug.Print("HandleClanArmoryUpdated: No CrpgCharacterLoadoutBehaviorClient found");
            return;
        }

        if (_clanArmory?.ClanArmoryItems == null || _clanArmory.ClanArmoryItems?.Count == 0)
        {
            Debug.Print("No clan armory items available.");
            InventoryGrid.SetArmoryItems(Array.Empty<(ItemObject, int, CrpgUserItemExtended)>());
            return;
        }

        var items = _clanArmory.ClanArmoryItems
            // keep only entries with a valid UserItem and ItemId
            .Where(ai => ai.UserItem is { ItemId: { Length: > 0 } })
            .Select(ai =>
            {
                var userItem = ai.UserItem!; // safe because of pattern above
                var itemObj = MBObjectManager.Instance.GetObject<ItemObject>(userItem.ItemId);
                return (itemObj, 1, userItem);
            })
            // drop anything where the ItemObject lookup failed
            .Where(t => t.itemObj != null)
            .ToList();

        InventoryGrid.SetArmoryItems(items);

        // rebuild inventory slots so armory icons reflect current armory state
        HandleInventoryUpdated();
    }

    private void HandleArmoryUserItemUpdated(ClanArmoryActionType action, int uItemId)
    {
        if (ItemInfo?.ItemObj is not null) // update the popup ItemInfoVM
        {
            if (ItemInfo?.UserItemExtended?.Id == uItemId)
            {
                ItemInfo.GenerateItemInfo(ItemInfo.ItemObj, uItemId);
            }
        }

        // update equipmentpanel/slots
        if (_userLoadout != null && _userLoadout.IsItemEquipped(uItemId))
        {
            foreach (var eSlot in EquipmentPanel.EquipmentSlots)
            {
                if (eSlot.UserItemId == uItemId)
                {
                    // unequip the item?
                    // Do something special for this one
                    // eSlot.
                }
            }
        }

        // remove from armory or add?
        // update inventory slots
        if (_userLoadout != null)
        {
            HandleClanArmoryUpdated();
            HandleInventoryUpdated();
        }
    }

    private void HandleEquippedItemsUpdated()
    {
        if (_userLoadout == null)
        {
            Debug.Print("No CrpgCharacterLoadoutBehaviorClient found");
            return;
        }

        var items = _userLoadout.EquippedItems;
        var equipment = _userLoadout.GetCrpgUserCharacterEquipment();
        var characteristics = _userLoadout.UserCharacter.Characteristics;

        ApplyEquipmentState(equipment, items, characteristics);
    }

    private void HandleUserCharacterBasicUpdated()
    {
        if (_userLoadout == null)
        {
            Debug.Print("No CrpgCharacterLoadoutBehaviorClient found");
            return;
        }

        CharacteristicsEditor.SetCrpgCharacterBasic(_userLoadout.UserCharacter);
        CharacterName = $"{_userLoadout.UserCharacter?.Name ?? string.Empty} ({_userLoadout.UserCharacter?.Level ?? 0})";
    }

    private void HandleUserCharacteristicsUpdated()
    {
        if (_userLoadout == null)
        {
            Debug.Print("No CrpgCharacterLoadoutBehaviorClient found");
            return;
        }

        CharacteristicsEditor.SetInitialCharacteristics(_userLoadout.UserCharacter.Characteristics);
    }

    private void HandleUserCharacteristicsConverted()
    {
        if (_userLoadout == null)
        {
            Debug.Print("No CrpgCharacterLoadoutBehaviorClient found");
            return;
        }

        CharacteristicsEditor.UpdateCharacteristicsPointsAfterConversion(
            _userLoadout.UserCharacter.Characteristics.Attributes.Points,
            _userLoadout.UserCharacter.Characteristics.Skills.Points);
    }

    private void HandleEquipNavButtonClicked(NavBarTab tab)
    {
        SelectNavBarTab(tab);
    }

    private void HandleUserInfoUpdated()
    {
        if (_userLoadout == null)
        {
            Debug.Print("No CrpgCharacterLoadoutBehaviorClient found");
            return;
        }

        MissionPeer? missionPeer = GameNetwork.MyPeer.GetComponent<MissionPeer>();
        CrpgPeer? crpgPeer = missionPeer?.GetComponent<CrpgPeer>();
        string clanTag = crpgPeer?.Clan?.Tag ?? string.Empty;

        if (clanTag.Length > 0)
        {
            clanTag = $"[{clanTag}]";
        }

        CharacterName = $"{_userLoadout.UserCharacter?.Name ?? string.Empty} ({_userLoadout.UserCharacter?.Level ?? 0})";
        UserName = $"{clanTag} {_userLoadout.User?.Name ?? string.Empty}";
        GoldAmount = _userLoadout.User?.Gold.ToString("N0") ?? "0";
    }

    private void UpdateCharacterBuildEquipmentStatDisplayFromNavbarSelection()
    {
        var (equipment, _) = GetCurrentEquipmentState();
        var characteristics = NavBar.ActiveTab == NavBarTab.Characteristics
                ? CharacteristicsEditor.GetCrpgCharacteristicsFromVM() ?? new CrpgCharacterCharacteristics()
                : GetCurrentCharacteristics();

        CharacterInfoBuildEquipStatsVm.UpdateCharacterBuildEquipmentStatDisplay(equipment, characteristics);
    }

    private void HandleTeamInventoryUpdated()
    {
        if (_teamInventory == null)
        {
            Debug.Print("No CrpgTeamInventoryClient found");
            return;
        }

        var items = _teamInventory.TeamItems
            .Where(ai => !string.IsNullOrEmpty(ai.Id) && ai.Quantity > 0)
            .Select(ai =>
            {
                var itemObj = MBObjectManager.Instance.GetObject<ItemObject>(ai.Id);
                return (itemObj, ai.Quantity);
            })
            .Where(t => t.itemObj != null)
            .ToList();

        InventoryGrid.SetTeamInventoryItems(items);
    }

    private void HandleTeamItemQuantityChanged(CrpgTeamInventoryItem item)
    {
        if (item.Quantity <= 0)
        {
            HandleTeamInventoryUpdated();
        }
        else if (!InventoryGrid.UpdateTeamItemQuantity(item.Id, item.Quantity))
        {
            HandleTeamInventoryUpdated();
        }
    }

    private void HandleEquippedTeamItemsUpdated()
    {
        if (_teamInventory is null)
        {
            Debug.Print("No CrpgTeamInventoryClient found");
            return;
        }

        var equipment = _teamInventory.GetSelectedTeamInventoryEquipment();
        var items = _teamInventory.EquippedItems;
        var characteristics = _userLoadout?.UserCharacter.Characteristics ?? new CrpgCharacterCharacteristics();

        ApplyEquipmentState(equipment, items, characteristics);
    }
}
