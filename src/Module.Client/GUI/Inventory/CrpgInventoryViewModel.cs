using Crpg.Module.Api.Models.Items;
using Crpg.Module.Common;
using Crpg.Module.Common.Network;
using Crpg.Module.Common.Network.Armory;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace Crpg.Module.GUI.Inventory;

public class CrpgInventoryViewModel : ViewModel
{
    public IGauntletMovie? Movie { get; set; }
    public GauntletLayer? Layer { get; set; }
    private UIContext? _context;
    public Widget? RootWidget { get; set; }
    internal CrpgCharacterLoadoutBehaviorClient? UserLoadoutBehavior { get; set; }
    private readonly CrpgClanArmoryClient? _clanArmory;
    private bool _isVisible;

    private string _characterName = string.Empty;
    private string _userName = string.Empty;
    private string _goldAmount = string.Empty;

    private EquipmentPanelVM _equipmentPanel = default!;
    private CharacteristicsEditorVM _characteristicsEditor;
    private CharacterInfoBuildEquipStatsVM _characterInfoBuildEquipStatsVm;
    private ItemInfoVM _itemInfo;
    private CharacterEquipNavBar _navBar;

    [DataSourceProperty]
    public CharacterEquipNavBar NavBar { get => _navBar; set => SetField(ref _navBar, value, nameof(NavBar)); }

    [DataSourceProperty]
    public InventoryGridVM InventoryGrid { get; set; }

    [DataSourceProperty]
    public ItemInfoVM ItemInfo { get => _itemInfo; set => SetField(ref _itemInfo, value, nameof(ItemInfo)); }

    [DataSourceProperty]
    public EquipmentPanelVM EquipmentPanel { get => _equipmentPanel; set => SetField(ref _equipmentPanel, value, nameof(EquipmentPanel)); }

    [DataSourceProperty]
    public CharacteristicsEditorVM CharacteristicsEditor { get => _characteristicsEditor; set => SetField(ref _characteristicsEditor, value, nameof(CharacteristicsEditor)); }

    [DataSourceProperty]
    public CharacterInfoBuildEquipStatsVM CharacterInfoBuildEquipStatsVm { get => _characterInfoBuildEquipStatsVm; set => SetField(ref _characterInfoBuildEquipStatsVm, value, nameof(CharacterInfoBuildEquipStatsVm)); }

    [DataSourceProperty]
    public bool IsVisible
    {
        get => _isVisible;
        set
        {
            if (_isVisible != value)
            {
                _isVisible = value;
                OnPropertyChangedWithValue(value, nameof(IsVisible));
                LogDebug($"IsVisible set to {_isVisible}");
            }
        }
    }

    [DataSourceProperty]
    public string CharacterName { get => _characterName; set => SetField(ref _characterName, value, nameof(CharacterName)); }

    [DataSourceProperty]
    public string UserName { get => _userName; set => SetField(ref _userName, value, nameof(UserName)); }

    [DataSourceProperty]
    public string GoldAmount { get => _goldAmount; set => SetField(ref _goldAmount, value, nameof(GoldAmount)); }

    public CrpgInventoryViewModel()
    {
        _navBar = new CharacterEquipNavBar();
        _characteristicsEditor = new CharacteristicsEditorVM();
        _characterInfoBuildEquipStatsVm = new CharacterInfoBuildEquipStatsVM();
        _equipmentPanel = new EquipmentPanelVM();
        InventoryGrid = new InventoryGridVM();

        _itemInfo = new ItemInfoVM(null);
        _equipmentPanel.InitializeCharacterPreview();

        // defaults
        _navBar.EquipmentSelected = true;
        EquipmentPanel.IsVisible = true;
        InventoryGrid.IsVisible = true;
        CharacteristicsEditor.IsVisible = false;
        _navBar.EquipmentSelected = true;
        _navBar.CharacteristicsSelected = false;

        // Events
        _navBar.OnEquipNavButtonClicked += HandleEquipNavButtonClicked;

        _characteristicsEditor.OnEditCharacteristicsChanged += () =>
        {
            UpdateCharacterBuildEquipmentStatDisplayFromNavbarSelection();
        };

        // Subscribe to EquipmentPanel events
        _equipmentPanel.OnItemDropped += HandleItemDrop;
        _equipmentPanel.OnItemDragBegin += HandleItemDragBegin;
        _equipmentPanel.OnItemDragEnd += HandleItemDragEnd;
        _equipmentPanel.OnSlotAlternateClicked += HandleAlternateClick;
        _equipmentPanel.OnSlotClicked += HandleClick;

        UserLoadoutBehavior = Mission.Current?.GetMissionBehavior<CrpgCharacterLoadoutBehaviorClient>();
        if (UserLoadoutBehavior != null)
        {
            LogDebug("Subscribing to CrpgCharacterLoadoutBehaviorClient events");
            UserLoadoutBehavior.OnEquipmentSlotUpdated += HandleEquipmentSlotUpdated;
            UserLoadoutBehavior.OnUserInventoryUpdated += HandleInventoryUpdated;
            UserLoadoutBehavior.OnUserCharacterEquippedItemsUpdated += HandleEquippedItemsUpdated;
            UserLoadoutBehavior.OnUserCharacterBasicUpdated += HandleUserCharacterBasicUpdated;
            UserLoadoutBehavior.OnUserCharacteristicsUpdated += HandleUserCharacteristicsUpdated;
            UserLoadoutBehavior.OnUserCharacteristicsConverted += HandleUserCharacteristicsConverted;
            UserLoadoutBehavior.OnUserInfoUpdated += HandleUserInfoUpdated;

            _characterName = $"{UserLoadoutBehavior.UserCharacter?.Name ?? string.Empty} ({UserLoadoutBehavior.UserCharacter?.Level ?? 0})";

            MissionPeer? missionPeer = GameNetwork.MyPeer.GetComponent<MissionPeer>();
            CrpgPeer? crpgPeer = missionPeer?.GetComponent<CrpgPeer>();
            string clanTag = crpgPeer?.Clan?.Tag ?? string.Empty;

            if (clanTag.Length > 0)
            {
                clanTag = $"[{clanTag}]";
            }

            _userName = $"{clanTag} {UserLoadoutBehavior?.User?.Name ?? string.Empty}";
            _goldAmount = UserLoadoutBehavior?.User?.Gold.ToString("N0") ?? "0";
        }

        _clanArmory = Mission.Current?.GetMissionBehavior<CrpgClanArmoryClient>();
        if (_clanArmory is not null)
        {
            LogDebug("Subscribing to CrpgClanArmoryClient events");
            _clanArmory.OnArmoryActionUpdated += HandleArmoryUserItemUpdated;
            _clanArmory.OnClanArmoryUpdated += HandleClanArmoryUpdated;
        }

        InventoryGrid.OnInventorySlotClicked += HandleClick;
        InventoryGrid.OnInventorySlotHoverEnd += HandleInventorySlotHoverEnd;
        InventoryGrid.OnInventorySlotDragStart += HandleItemDragBegin;
        InventoryGrid.OnInventorySlotDragEnd += HandleItemDragEnd;
        InventoryGrid.OnInventoryChangeType += HandleInventoryChangeType;

        // HandleInventoryChangeType(0); // Initialize drag events
    }

    public override void OnFinalize()
    {
        base.OnFinalize();
        LogDebug("OnFinalize()");

        _equipmentPanel.OnItemDropped -= HandleItemDrop;
        _equipmentPanel.OnItemDragBegin -= HandleItemDragBegin;
        _equipmentPanel.OnItemDragEnd -= HandleItemDragEnd;
        EquipmentPanel.OnSlotAlternateClicked -= HandleAlternateClick;
        _equipmentPanel.OnSlotClicked -= HandleClick;

        UserLoadoutBehavior = Mission.Current?.GetMissionBehavior<CrpgCharacterLoadoutBehaviorClient>();
        if (UserLoadoutBehavior != null)
        {
            UserLoadoutBehavior.OnEquipmentSlotUpdated -= HandleEquipmentSlotUpdated;
            UserLoadoutBehavior.OnUserInventoryUpdated -= HandleInventoryUpdated;
            UserLoadoutBehavior.OnUserCharacterEquippedItemsUpdated -= HandleEquippedItemsUpdated;
            UserLoadoutBehavior.OnUserCharacterBasicUpdated -= HandleUserCharacterBasicUpdated;
            UserLoadoutBehavior.OnUserCharacteristicsUpdated -= HandleUserCharacteristicsUpdated;
            UserLoadoutBehavior.OnUserCharacteristicsConverted -= HandleUserCharacteristicsConverted;
            UserLoadoutBehavior.OnUserInfoUpdated -= HandleUserInfoUpdated;
        }

        if (_clanArmory is not null)
        {
            _clanArmory.OnClanArmoryUpdated -= HandleClanArmoryUpdated;
            _clanArmory.OnArmoryActionUpdated -= HandleArmoryUserItemUpdated;
        }

        /*
                foreach (var slot in InventoryGrid.AvailableItems)
                {
                    slot.OnItemDragBegin -= HandleItemDragBegin;
                    slot.OnItemDragEnd -= HandleItemDragEnd;
                }
        */
        InventoryGrid.OnInventorySlotClicked -= HandleClick;
        InventoryGrid.OnInventorySlotHoverEnd -= HandleInventorySlotHoverEnd;
        InventoryGrid.OnInventorySlotDragStart += HandleItemDragBegin;
        InventoryGrid.OnInventorySlotDragEnd += HandleItemDragEnd;
        InventoryGrid.OnInventoryChangeType -= HandleInventoryChangeType;
    }

    public void SetRootWidget(Widget rootWidget)
    {
        RootWidget = rootWidget;
        LogDebug("RootWidget set");
    }

    public void SetContext(UIContext context)
    {
        _context = context;

        _context.Root.FindChild("InventoryGridPrefab");
        if (_context.Root != null)
        {
            LogDebug("UIContext Root found");
        }
        else
        {
            LogDebugError("UIContext Root NOT found");
        }
        MakeItemInfo();
    }

    private void ShowItemInfoPopup(bool show, ItemObject? itemObj, int userItemId = -1)
    {
        if (!show)
        {
            ItemInfo = new ItemInfoVM(null)
            {
                IsVisible = false,
            };
            return;
        }

        // Check for userItem
        var userItemExtended = UserLoadoutBehavior?.GetCrpgUserItem(userItemId);

        if (userItemExtended != null)
        {
            if (itemObj == null)
            {
                LogDebugError($" ItemObj is null");
                return;
            }

            ItemInfo = new ItemInfoVM(itemObj, userItemId);
            ItemInfo.GenerateItemInfo(itemObj, userItemId);

            var mousePos = Input.MousePositionPixel; // vec2

            ItemInfo.PositionX = mousePos.X + 25;
            ItemInfo.PositionY = mousePos.Y - 100;
            ItemInfo.IsVisible = true;

            LogDebug($"ShowItemInfoPopup at ({mousePos.X}, {mousePos.Y}) for {itemObj?.Name}");
        }
        else
        {
            LogDebugError($"userItemExtended is null");
        }
    }

    public void ExecuteDropDiscard(ViewModel draggedItem, int index)
    {
        LogDebug("ExecuteDropDiscard");
        if (draggedItem is EquipmentSlotVM eqSlot)
        {
            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(new UserRequestEquipCharacterItem { Slot = eqSlot.CrpgItemSlotIndex, UserItemId = -1 });
            GameNetwork.EndModuleEventAsClient();
        }
    }

    private void HandleInventoryChangeType(int type)
    {
        LogDebug($"HandleInventoryChangeType: {type}");
        /*
                foreach (var slot in InventoryGrid.AvailableItems)
                {
                    slot.OnItemDragBegin -= HandleItemDragBegin;
                    slot.OnItemDragEnd -= HandleItemDragEnd;

                    slot.OnItemDragBegin += HandleItemDragBegin;
                    slot.OnItemDragEnd += HandleItemDragEnd;
                }
                */
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
            LogDebug($"Inventory item dropped: {inv.ItemName} (UserItemId: {userItemId}) into slot {targetSlot.CrpgItemSlotIndex}");
        }
        else if (draggedItem is EquipmentSlotVM eq)
        {
            // 🔹 Prevent dropping onto the same slot or empty item
            if (eq.CrpgItemSlotIndex == targetSlot.CrpgItemSlotIndex || eq.ItemObj == null)
            {
                LogDebug("Ignored drop: same slot or empty item dropped");
                return;
            }

            userItemId = eq.UserItemId;
            draggedItemObject = eq.ItemObj;
            sourceEquipmentSlot = eq;

            LogDebug($"Equipment item dropped: {eq.ItemObj?.Name} (UserItemId: {userItemId}) into slot {targetSlot.CrpgItemSlotIndex}");
        }
        else
        {
            LogDebugError("Dropped unknown item type");
            return;
        }

        if (draggedItemObject == null)
        {
            LogDebugError("No Object to drag");
            return;
        }

        // Prevent equipping an item that does not fit the target slot
        if (!Equipment.IsItemFitsToSlot(EquipmentSlotVM.ConvertToEquipmentIndex(targetSlot.CrpgItemSlotIndex), draggedItemObject))
        {
            LogDebug($"{draggedItemObject.Name} cannot be equipped in slot {targetSlot.CrpgItemSlotIndex}");
            return;
        }

        // Prevent equipping the same item into the same slot
        if (draggedItemObject == targetSlot.ItemObj && userItemId == targetSlot.UserItemId)
        {
            LogDebug("Ignored drop: item already equipped in target slot");
            return;
        }

        // ✅ Send equip request
        LogDebug("Sending equip request to server");
        GameNetwork.BeginModuleEventAsClient();
        GameNetwork.WriteMessage(new UserRequestEquipCharacterItem
        {
            Slot = targetSlot.CrpgItemSlotIndex,
            UserItemId = userItemId,
        });
        GameNetwork.EndModuleEventAsClient();

        // ✅ Clear the source slot only if different from target
        if (sourceEquipmentSlot != null && sourceEquipmentSlot.CrpgItemSlotIndex != targetSlot.CrpgItemSlotIndex)
        {
            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(new UserRequestEquipCharacterItem
            {
                Slot = sourceEquipmentSlot.CrpgItemSlotIndex,
                UserItemId = -1,
            });
            GameNetwork.EndModuleEventAsClient();
        }
    }

    private void HandleItemDragBegin(ViewModel viewModel)
    {
        LogDebugError("HandleItemDragBegin - updating equipment slot states");
        if (viewModel is InventorySlotVM inv)
        {
            LogDebugError($"HandleItemDragBegin for item: {inv.ItemObj?.Name}");

            if (inv.ItemObj == null)
            {
                return;
            }

            foreach (var slotVm in EquipmentPanel.EquipmentSlots)
            {
                // Disable equipment slots where the item **cannot** be equipped
                if (!Equipment.IsItemFitsToSlot(EquipmentSlotVM.ConvertToEquipmentIndex(slotVm.CrpgItemSlotIndex), inv.ItemObj))
                {
                    slotVm.IsButtonEnabled = false;
                }
            }
        }
        else if (viewModel is EquipmentSlotVM eq)
        {
            LogDebugError($"HandleItemDragBegin for item: {eq.ItemObj?.Name}");

            if (eq.ItemObj == null)
            {
                return;
            }

            foreach (var slotVm in EquipmentPanel.EquipmentSlots)
            {
                // Disable equipment slots where the item **cannot** be equipped
                if (!Equipment.IsItemFitsToSlot(EquipmentSlotVM.ConvertToEquipmentIndex(slotVm.CrpgItemSlotIndex), eq.ItemObj))
                {
                    slotVm.IsButtonEnabled = false;
                }
            }
        }
    }

    private void HandleItemDragEnd(ViewModel viewModel)
    {
        LogDebug("HandleItemDragEnd - resetting equipment slot states");

        if (viewModel is InventorySlotVM inv)
        {
            foreach (var slotVm in EquipmentPanel.EquipmentSlots)
            {
                slotVm.IsButtonEnabled = true;
            }
        }
        else if (viewModel is EquipmentSlotVM eq)
        {
            foreach (var slotVm in EquipmentPanel.EquipmentSlots)
            {
                slotVm.IsButtonEnabled = true;
            }
        }
    }

    private void HandleAlternateClick(EquipmentSlotVM slot)
    {
        LogDebug($"Alternate click on slot: {slot.CrpgItemSlotIndex}");
        GameNetwork.BeginModuleEventAsClient();
        GameNetwork.WriteMessage(new UserRequestEquipCharacterItem { Slot = slot.CrpgItemSlotIndex, UserItemId = -1 });
        GameNetwork.EndModuleEventAsClient();
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
                LogDebugError("Clicked unknown ViewModel type");
                break;
        }
    }

    // Gui Updates because of UserLoadoutBehavior (API usage)
    private void HandleEquipmentSlotUpdated(CrpgItemSlot updatedSlot)
    {
        LogDebug($"HandleSlotUpdated called for slot {updatedSlot}");

        if (UserLoadoutBehavior == null)
        {
            LogDebug("No CrpgCharacterLoadoutBehaviorClient found", Colors.Red);
            return;
        }

        var slotVm = EquipmentPanel.EquipmentSlots.FirstOrDefault(s => s.CrpgItemSlotIndex == updatedSlot);
        if (slotVm == null)
        {
            LogDebug("No matching EquipmentSlotVM found for updated slot", Colors.Red);
            return;
        }

        var equippedItem = UserLoadoutBehavior.EquippedItems.FirstOrDefault(e => e.Slot == updatedSlot);
        if (equippedItem != null)
        {
            var itemObj = MBObjectManager.Instance.GetObject<ItemObject>(equippedItem.UserItem.ItemId);
            slotVm.SetItem(new ImageIdentifierVM(itemObj), itemObj, equippedItem.UserItem);
            LogDebug($"Slot {updatedSlot} set to item {itemObj?.Name} (UserItemId: {equippedItem.UserItem.Id})");
        }
        else
        {
            slotVm.ClearItem();
            LogDebug($"Slot {updatedSlot} cleared");
        }

        var equipment = UserLoadoutBehavior.GetCrpgUserCharacterEquipment();
        EquipmentPanel.RefreshCharacterPreview(equipment);

        var characteristics = UserLoadoutBehavior.UserCharacter.Characteristics;
        CharacterInfoBuildEquipStatsVm.UpdateCharacterBuildEquipmentStatDisplay(equipment, characteristics);
        // RefreshCharacterPreview(UserLoadoutBehavior.GetCrpgUserCharacterEquipment());
    }

    private void HandleInventoryUpdated()
    {
        LogDebug("HandleInventoryUpdated called");
        if (UserLoadoutBehavior == null)
        {
            LogDebugError("No CrpgCharacterLoadoutBehaviorClient found");
            return;
        }

        var items = UserLoadoutBehavior.UserInventoryItems
            .Where(ui => !string.IsNullOrEmpty(ui.ItemId))
            .Select(ui => (MBObjectManager.Instance.GetObject<ItemObject>(ui.ItemId), 1, ui))
            .Where(t => t.Item1 != null)
            .ToList();

        InventoryGrid.SetInventoryItems(items);
        InventoryGrid.InitializeFilteredItemsList();

        OnPropertyChanged(nameof(InventoryGrid));
        LogDebug($"Inventory updated with {items.Count} items");
    }

    private void HandleClanArmoryUpdated()
    {
        LogDebug("HandleClanArmoryUpdated()");

        if (UserLoadoutBehavior == null)
        {
            LogDebugError("No CrpgCharacterLoadoutBehaviorClient found");
            return;
        }

        if (_clanArmory?.ClanArmoryItems == null || _clanArmory.ClanArmoryItems?.Count == 0)
        {
            LogDebug("No clan armory items available.");
            InventoryGrid.SetArmoryItems(Array.Empty<(ItemObject, int, CrpgUserItemExtended)>());
            return;
        }

        var items = _clanArmory.ClanArmoryItems
            // keep only entries with a valid UserItem and ItemId
            .Where(ai => ai.UserItem is { ItemId: { Length: > 0 } })
            // now we can safely use ai.UserItem without null-forgiving
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
        InventoryGrid.InitializeFilteredItemsList();
    }

    private void HandleArmoryUserItemUpdated(ClanArmoryActionType action, int uItemId)
    {
        LogDebug($"HandleArmoryUserItemUpdated() mode: {action} userItemId:{uItemId} ");

        if (ItemInfo?.ItemObj is not null) // update the popup ItemInfoVM
        {
            if (ItemInfo?.UserItemExtended?.Id == uItemId)
            {
                ItemInfo.GenerateItemInfo(ItemInfo.ItemObj, uItemId);
            }
        }

        // update equipmentpanel/slots
        if (UserLoadoutBehavior != null && UserLoadoutBehavior.IsItemEquipped(uItemId))
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
        if (UserLoadoutBehavior != null)
        {
            HandleClanArmoryUpdated();
            HandleInventoryUpdated();
        }

        // update inventorygrid/slots
        // if (UserLoadoutBehavior != null &)
    }

    private void HandleEquippedItemsUpdated()
    {
        LogDebug("HandleEquippedItemsUpdated()");

        if (UserLoadoutBehavior == null)
        {
            LogDebugError("No CrpgCharacterLoadoutBehaviorClient found");
            return;
        }

        foreach (var slotVm in EquipmentPanel.EquipmentSlots)
        {
            var equippedItem = UserLoadoutBehavior.EquippedItems.FirstOrDefault(e => e.Slot == slotVm.CrpgItemSlotIndex);
            if (equippedItem != null)
            {
                var itemObj = MBObjectManager.Instance.GetObject<ItemObject>(equippedItem.UserItem.ItemId);
                slotVm.SetItem(new ImageIdentifierVM(itemObj), itemObj, equippedItem.UserItem);
            }
            else
            {
                slotVm.ClearItem();
            }
        }

        var equipment = UserLoadoutBehavior.GetCrpgUserCharacterEquipment();
        EquipmentPanel.RefreshCharacterPreview(equipment);

        var characteristics = UserLoadoutBehavior.UserCharacter.Characteristics;
        CharacterInfoBuildEquipStatsVm.UpdateCharacterBuildEquipmentStatDisplay(equipment, characteristics);
    }

    private void HandleUserCharacterBasicUpdated()
    {
        LogDebug("HandleUserCharacterBasicUpdated called");

        if (UserLoadoutBehavior == null)
        {
            LogDebug("No CrpgCharacterLoadoutBehaviorClient found", Colors.Red);
            return;
        }

        CharacteristicsEditor.SetCrpgCharacterBasic(UserLoadoutBehavior.UserCharacter);
    }

    private void HandleUserCharacteristicsUpdated()
    {
        LogDebug("HandleUserCharacteristicsUpdated()");

        if (UserLoadoutBehavior == null)
        {
            LogDebugError("No CrpgCharacterLoadoutBehaviorClient found");
            return;
        }

        CharacteristicsEditor.SetInitialCharacteristics(UserLoadoutBehavior.UserCharacter.Characteristics);
        LogDebug("Updated CHaracterInfoVM with new characteristics");
    }

    private void HandleUserCharacteristicsConverted()
    {
        LogDebug("HandleUserCharacteristicsConverted called");

        if (UserLoadoutBehavior == null)
        {
            LogDebugError("No CrpgCharacterLoadoutBehaviorClient found");
            return;
        }

        CharacteristicsEditor.UpdateCharacteristicsPointsAfterConversion(
            UserLoadoutBehavior.UserCharacter.Characteristics.Attributes.Points,
            UserLoadoutBehavior.UserCharacter.Characteristics.Skills.Points);

        LogDebug("HandleUserCharacteristicsConverted finished.");
    }

    private void HandleEquipNavButtonClicked(string button)
    {
        LogDebug($"HandleEquipNavButtonClicked() {button} ");

        if (button == "Equipment")
        {
            EquipmentPanel.IsVisible = true;
            InventoryGrid.IsVisible = true;
            CharacteristicsEditor.IsVisible = false;
            _navBar.EquipmentSelected = true;
            _navBar.CharacteristicsSelected = false;
        }
        else if (button == "Characteristics")
        {
            EquipmentPanel.IsVisible = false;
            InventoryGrid.IsVisible = false;
            CharacteristicsEditor.IsVisible = true;
            _navBar.EquipmentSelected = false;
            _navBar.CharacteristicsSelected = true;
        }
        else
        {
            LogDebugError($"Unknown button: {button}");
        }

        UpdateCharacterBuildEquipmentStatDisplayFromNavbarSelection();
    }

    private void HandleUserInfoUpdated()
    {
        LogDebug("HandleUserInfoUpdated()");

        if (UserLoadoutBehavior == null)
        {
            LogDebugError("No CrpgCharacterLoadoutBehaviorClient found");
            return;
        }

        CharacterName = $"{UserLoadoutBehavior.UserCharacter?.Name ?? string.Empty} ({UserLoadoutBehavior.UserCharacter?.Level ?? 0})";

        MissionPeer? missionPeer = GameNetwork.MyPeer.GetComponent<MissionPeer>();
        CrpgPeer? crpgPeer = missionPeer?.GetComponent<CrpgPeer>();
        string clanTag = crpgPeer?.Clan?.Tag ?? string.Empty;

        if (clanTag.Length > 0)
        {
            clanTag = $"[{clanTag}]";
        }

        UserName = $"{clanTag} {UserLoadoutBehavior?.User?.Name ?? string.Empty}";
        GoldAmount = UserLoadoutBehavior?.User?.Gold.ToString("N0") ?? "0";
    }

    private void UpdateCharacterBuildEquipmentStatDisplayFromNavbarSelection()
    {
        if (_navBar.EquipmentSelected)
        {
            // Update CharacterInfoBuildEquipStatsVM to use userLoadoutBehavior characteristics data
            var characteristics = UserLoadoutBehavior?.UserCharacter.Characteristics;
            var equipment = UserLoadoutBehavior?.GetCrpgUserCharacterEquipment();
            if (characteristics != null && equipment != null)
            {
                CharacterInfoBuildEquipStatsVm.UpdateCharacterBuildEquipmentStatDisplay(equipment, characteristics);
            }
            else
            {
                LogDebugError("Failed to update CharacterInfoBuildEquipStatsVm: characteristics or equipment is null");
            }
        }
        else if (_navBar.CharacteristicsSelected)
        {
            // Update CharacterInfoBuildEquipStatsVM to use CharacteristicsEditorVM data
            var characteristics = CharacteristicsEditor.GetCrpgCharacteristicsFromVM();
            var equipment = UserLoadoutBehavior?.GetCrpgUserCharacterEquipment();

            if (characteristics != null && equipment != null)
            {
                CharacterInfoBuildEquipStatsVm.UpdateCharacterBuildEquipmentStatDisplay(equipment, characteristics);
            }
            else
            {
                LogDebugError("Failed to update CharacterInfoBuildEquipStatsVm: characteristics or equipment is null");
            }
        }
    }

    private void MakeItemInfo()
    {
        if (RootWidget == null)
        {
            LogDebug("Widget is NULL");
            return;
        }

        Widget? userInventoryGridWidget = FindChildById(RootWidget, "InventoryGridRoot");

        if (userInventoryGridWidget != null)
        {
            LogDebug("InventoryGridPrefab found");
        }
        else
        {
            LogDebug("InventoryGridPrefab NOT FOUND", Colors.Red);
        }
    }

    public static Widget? FindChildById(Widget parent, string id)
    {
        if (parent == null)
        {
            return null;
        }

        if (parent.Id == id)
        {
            return parent;
        }

        foreach (var child in parent.Children)
        {
            var result = FindChildById(child, id);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

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
