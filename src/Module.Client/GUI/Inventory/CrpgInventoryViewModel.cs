using Crpg.Module.Api.Models.Items;
using Crpg.Module.Common;
using Crpg.Module.Common.Network;
using Crpg.Module.Common.Network.Armory;
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
        _equipmentPanel.OnItemDragBegin += slot =>
        {
            if (slot.ItemObj != null)
            {
                HandleItemDragBegin(slot.ItemObj);
            }
        };

        _equipmentPanel.OnItemDragEnd += slot =>
        {
            if (slot.ItemObj != null)
            {
                HandleItemDragEnd(slot.ItemObj);
            }
        };
        EquipmentPanel.OnSlotAlternateClicked += HandleAlternateClick;
        _equipmentPanel.OnSlotClicked += HandleItemClick;

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

            _characterName = $"{UserLoadoutBehavior.UserCharacter?.Name ?? string.Empty} ({UserLoadoutBehavior.UserCharacter?.Level ?? 0})";

            MissionPeer? missionPeer = GameNetwork.MyPeer.GetComponent<MissionPeer>();
            CrpgPeer? crpgPeer = missionPeer?.GetComponent<CrpgPeer>();
            string clanTag = crpgPeer?.Clan?.Tag ?? string.Empty;
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

        InventoryGrid.OnInventorySlotClicked += HandleInventorySlotClicked;
        InventoryGrid.OnInventorySlotHoverEnd += HandleInventorySlotHoverEnd;
    }

    public override void OnFinalize()
    {
        LogDebug("CrpgInventoryViewModel-- OnFinalize()");

        _equipmentPanel.OnItemDropped -= HandleItemDrop;
        EquipmentPanel.OnSlotAlternateClicked -= HandleAlternateClick;
        _equipmentPanel.OnSlotClicked -= HandleItemClick;

        UserLoadoutBehavior = Mission.Current?.GetMissionBehavior<CrpgCharacterLoadoutBehaviorClient>();
        if (UserLoadoutBehavior != null)
        {
            UserLoadoutBehavior.OnEquipmentSlotUpdated -= HandleEquipmentSlotUpdated;
            UserLoadoutBehavior.OnUserInventoryUpdated -= HandleInventoryUpdated;
            UserLoadoutBehavior.OnUserCharacterEquippedItemsUpdated -= HandleEquippedItemsUpdated;
            UserLoadoutBehavior.OnUserCharacterBasicUpdated -= HandleUserCharacterBasicUpdated;
            UserLoadoutBehavior.OnUserCharacteristicsUpdated -= HandleUserCharacteristicsUpdated;
            UserLoadoutBehavior.OnUserCharacteristicsConverted -= HandleUserCharacteristicsConverted;
        }

        if (_clanArmory is not null)
        {
            _clanArmory.OnClanArmoryUpdated -= HandleClanArmoryUpdated;
            _clanArmory.OnArmoryActionUpdated -= HandleArmoryUserItemUpdated;
        }

        InventoryGrid.OnInventorySlotClicked -= HandleInventorySlotClicked;
        InventoryGrid.OnInventorySlotHoverEnd -= HandleInventorySlotHoverEnd;
    }

    public void SetRootWidget(Widget rootWidget)
    {
        RootWidget = rootWidget;
        LogDebug("RootWidget set");
    }

    public void SetContext(UIContext context)
    {
        _context = context;
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

    private void HandleArmoryUserItemUpdated(ClanArmoryActionType action, int uItemId)
    {
        LogDebugError($"HandleArmoryUserItemUpdated() mode: {action} userItemId:{uItemId} ");

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

    private void HandleInventorySlotClicked(InventorySlotVM slot)
    {
        LogDebug($"HandleInventorySlotClicked() {slot.ItemName} ");
        ShowItemInfoPopup(true, slot.ItemObj, slot.UserItemId);
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

    private void HandleItemDragBegin(ItemObject itemObj)
    {
        {
            LogDebug($"HandleItemDragBegin for item: {itemObj?.Name}");

            if (itemObj == null)
            {
                return;
            }

            foreach (var slotVm in EquipmentPanel.EquipmentSlots)
            {
                // Disable equipment slots where the item **cannot** be equipped
                if (!Equipment.IsItemFitsToSlot(EquipmentSlotVM.ConvertToEquipmentIndex(slotVm.CrpgItemSlotIndex), itemObj))
                {
                    slotVm.IsButtonEnabled = false;
                }
            }
        }
    }

    private void HandleItemDragEnd(ItemObject itemObj)
    {
        LogDebug("HandleItemDragEnd - resetting equipment slot states");

        foreach (var slotVm in EquipmentPanel.EquipmentSlots)
        {
            slotVm.IsButtonEnabled = true;
        }
    }

    private void HandleAlternateClick(EquipmentSlotVM slot)
    {
        LogDebug($"Alternate click on slot: {slot.CrpgItemSlotIndex}");
        GameNetwork.BeginModuleEventAsClient();
        GameNetwork.WriteMessage(new UserRequestEquipCharacterItem { Slot = slot.CrpgItemSlotIndex, UserItemId = -1 });
        GameNetwork.EndModuleEventAsClient();
    }

    private void HandleItemClick(EquipmentSlotVM slot)
    {
        LogDebug($"HandleItemClick() ");
        ShowItemInfoPopup(true, slot.ItemObj, slot.UserItemId);
    }

    private void HandleClick(ViewModel viewModel)
    {
        if (viewModel is EquipmentSlotVM)
        {

        }
        else if (viewModel is InventorySlotVM)
        {

        }
        else if (viewModel is InventorySortTypeVM)
        {

        }
    }

    // Gui Updates because of behavior (API usage)
    private void HandleEquipmentSlotUpdated(CrpgItemSlot updatedSlot)
    {
        LogDebug($"[CrpgInventoryVM] HandleSlotUpdated called for slot {updatedSlot}");

        var behavior = Mission.Current?.GetMissionBehavior<CrpgCharacterLoadoutBehaviorClient>();
        if (behavior == null)
        {
            LogDebug("[CrpgInventoryVM] No CrpgCharacterLoadoutBehaviorClient found", Colors.Red);
            return;
        }

        var slotVm = EquipmentPanel.EquipmentSlots.FirstOrDefault(s => s.CrpgItemSlotIndex == updatedSlot);
        if (slotVm == null)
        {
            LogDebug("[CrpgInventoryVM] No matching EquipmentSlotVM found for updated slot", Colors.Red);
            return;
        }

        var equippedItem = behavior.EquippedItems.FirstOrDefault(e => e.Slot == updatedSlot);
        if (equippedItem != null)
        {
            var itemObj = MBObjectManager.Instance.GetObject<ItemObject>(equippedItem.UserItem.ItemId);
            slotVm.SetItem(new ImageIdentifierVM(itemObj), itemObj, equippedItem.UserItem);
            LogDebug($"[CrpgInventoryVM] Slot {updatedSlot} set to item {itemObj?.Name} (UserItemId: {equippedItem.UserItem.Id})");
        }
        else
        {
            slotVm.ClearItem();
            LogDebug($"[CrpgInventoryVM] Slot {updatedSlot} cleared");
        }

        var equipment = behavior.GetCrpgUserCharacterEquipment();
        EquipmentPanel.RefreshCharacterPreview(equipment);

        var characteristics = behavior.UserCharacter.Characteristics;
        CharacterInfoBuildEquipStatsVm.UpdateCharacterBuildEquipmentStatDisplay(equipment, characteristics);
        // RefreshCharacterPreview(behavior.GetCrpgUserCharacterEquipment());
    }

    private void HandleInventoryUpdated()
    {
        LogDebug("HandleInventoryUpdated called");

        var behavior = Mission.Current?.GetMissionBehavior<CrpgCharacterLoadoutBehaviorClient>();
        if (behavior == null)
        {
            LogDebugError("No CrpgCharacterLoadoutBehaviorClient found");
            return;
        }

        var items = behavior.UserInventoryItems
            .Where(ui => !string.IsNullOrEmpty(ui.ItemId))
            .Select(ui => (MBObjectManager.Instance.GetObject<ItemObject>(ui.ItemId), 1, ui))
            .Where(t => t.Item1 != null)
            .ToList();

        InventoryGrid.SetInventoryItems(items);
        /*
                var items = behavior.UserInventoryItems
                    .Where(ui => !string.IsNullOrEmpty(ui.ItemId))
                    .Select(ui =>
                    {
                        var obj = MBObjectManager.Instance.GetObject<ItemObject>(ui.ItemId);
                        return obj != null ? (obj, 1, ui) : default;
                    })
                    .Where(t => t.obj != null)
                    .ToList();
        */
        // InventoryGrid.SetAvailableItems(items);
        InventoryGrid.InitializeFilteredItemsList();

        foreach (var slot in InventoryGrid.AvailableItems)
        {
            slot.OnItemDragBegin -= HandleItemDragBegin;
            slot.OnItemDragEnd -= HandleItemDragEnd;

            slot.OnItemDragBegin += HandleItemDragBegin;
            slot.OnItemDragEnd += HandleItemDragEnd;
        }

        OnPropertyChanged(nameof(InventoryGrid));
        LogDebug($"[CrpgInventoryVM] Inventory updated with {items.Count} items");
    }

    private void HandleClanArmoryUpdated()
    {
        LogDebug("HandleClanArmoryUpdated()");

        var behavior = Mission.Current?.GetMissionBehavior<CrpgCharacterLoadoutBehaviorClient>();
        if (behavior == null)
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

        foreach (var slot in InventoryGrid.AvailableItems)
        {
            slot.OnItemDragBegin -= HandleItemDragBegin;
            slot.OnItemDragEnd -= HandleItemDragEnd;

            slot.OnItemDragBegin += HandleItemDragBegin;
            slot.OnItemDragEnd += HandleItemDragEnd;
        }
    }

    private void HandleEquippedItemsUpdated()
    {
        LogDebug("HandleEquippedItemsUpdated()");

        var behavior = Mission.Current?.GetMissionBehavior<CrpgCharacterLoadoutBehaviorClient>();
        if (behavior == null)
        {
            LogDebugError("No CrpgCharacterLoadoutBehaviorClient found");
            return;
        }

        foreach (var slotVm in EquipmentPanel.EquipmentSlots)
        {
            var equippedItem = behavior.EquippedItems.FirstOrDefault(e => e.Slot == slotVm.CrpgItemSlotIndex);
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

        var equipment = behavior.GetCrpgUserCharacterEquipment();
        EquipmentPanel.RefreshCharacterPreview(equipment);

        var characteristics = behavior.UserCharacter.Characteristics;
        CharacterInfoBuildEquipStatsVm.UpdateCharacterBuildEquipmentStatDisplay(equipment, characteristics);
    }

    private void HandleUserCharacterBasicUpdated()
    {
        LogDebug("HandleUserCharacterBasicUpdated called");

        var behavior = Mission.Current?.GetMissionBehavior<CrpgCharacterLoadoutBehaviorClient>();
        if (behavior == null)
        {
            LogDebug("No CrpgCharacterLoadoutBehaviorClient found", Colors.Red);
            return;
        }

        CharacteristicsEditor.SetCrpgCharacterBasic(behavior.UserCharacter);
    }

    private void HandleUserCharacteristicsUpdated()
    {
        LogDebug("HandleUserCharacteristicsUpdated()");

        var behavior = Mission.Current?.GetMissionBehavior<CrpgCharacterLoadoutBehaviorClient>();
        if (behavior == null)
        {
            LogDebugError("No CrpgCharacterLoadoutBehaviorClient found");
            return;
        }

        CharacteristicsEditor.SetInitialCharacteristics(behavior.UserCharacter.Characteristics);
        LogDebug("Updated CHaracterInfoVM with new characteristics");
    }

    private void HandleUserCharacteristicsConverted()
    {
        LogDebug("HandleUserCharacteristicsConverted called");

        var behavior = Mission.Current?.GetMissionBehavior<CrpgCharacterLoadoutBehaviorClient>();
        if (behavior == null)
        {
            LogDebugError("No CrpgCharacterLoadoutBehaviorClient found");
            return;
        }

        CharacteristicsEditor.UpdateCharacteristicsPointsAfterConversion(
            behavior.UserCharacter.Characteristics.Attributes.Points,
            behavior.UserCharacter.Characteristics.Skills.Points);

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

        Widget userInventoryGridWidget = RootWidget.FindChild("UserInventoryGrid");

        if (userInventoryGridWidget != null)
        {
            LogDebug("userInventoryGridWidget found");
        }
        else
        {
            LogDebug("userInventoryGridWidget NOT FOUND", Colors.Red);
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

    private Vec2 ConvertScreenToWidgetCoordinates(Vec2 screenPos)
    {
        if (Movie == null || Layer == null)
            return screenPos; // fallback

        float layerWidth = Layer.UsableArea.X;
        float layerHeight = Layer.UsableArea.Y;

        // Convert pixel coordinates to relative (0..1)
        float x = screenPos.X; // / layerWidth;
        float y = screenPos.Y; // / layerHeight;

        return new Vec2(x, y);
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
