using System.Runtime.InteropServices;
using Crpg.Module.Api.Models.Items;
using Crpg.Module.Common;
using Crpg.Module.Common.Network;
using Crpg.Module.Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Engine;
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

    private bool _isVisible;

    private EquipmentPanelVM _equipmentPanel = default!;
    private CharacteristicsEditorVM _characteristicsEditor;
    private CharacterInfoBuildEquipStatsVM _characterInfoBuildEquipStatsVm;
    private ItemInfoVM _itemInfo;


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
                LogDebug($"[CrpgInventoryVM] IsVisible set to {_isVisible}");
            }
        }
    }

    public CrpgInventoryViewModel()
    {
        _characteristicsEditor = new CharacteristicsEditorVM();
        _characterInfoBuildEquipStatsVm = new CharacterInfoBuildEquipStatsVM();
        _equipmentPanel = new EquipmentPanelVM();
        InventoryGrid = new InventoryGridVM();

        _itemInfo = new ItemInfoVM(null);
        _equipmentPanel.InitializeCharacterPreview();

        // Subscribe to EquipmentPanel events
        _equipmentPanel.OnItemDropped += HandleItemDrop;
        _equipmentPanel.OnItemDragBegin += slot =>
        {
            if (slot.ItemObj != null)
                HandleItemDragBegin(slot.ItemObj);
        };
        _equipmentPanel.OnItemDragEnd += slot =>
        {
            if (slot.ItemObj != null)
                HandleItemDragEnd(slot.ItemObj);
        };
        EquipmentPanel.OnSlotAlternateClicked += HandleAlternateClick;
        _equipmentPanel.OnSlotClicked += HandleItemClick;

        LogDebug("[CrpgInventoryVM] Subscribing to CrpgCharacterLoadoutBehaviorClient events");
        UserLoadoutBehavior = Mission.Current?.GetMissionBehavior<CrpgCharacterLoadoutBehaviorClient>();
        if (UserLoadoutBehavior != null)
        {
            UserLoadoutBehavior.OnEquipmentSlotUpdated += HandleEquipmentSlotUpdated;
            UserLoadoutBehavior.OnUserInventoryUpdated += HandleInventoryUpdated;
            UserLoadoutBehavior.OnClanArmoryUpdated += HandleClanArmoryUpdated;
            UserLoadoutBehavior.OnUserCharacterEquippedItemsUpdated += HandleEquippedItemsUpdated;
            UserLoadoutBehavior.OnUserCharacterBasicUpdated += HandleUserCharacterBasicUpdated;
            UserLoadoutBehavior.OnUserCharacteristicsUpdated += HandleUserCharacteristicsUpdated;
            UserLoadoutBehavior.OnUserCharacteristicsConverted += HandleUserCharacteristicsConverted;
            UserLoadoutBehavior.OnArmoryActionUpdated += HandleArmoryUserItemUpdated;
        }

        InventoryGrid.OnInventorySlotClicked += HandleInventorySlotClicked;
        InventoryGrid.OnInventorySortTypeClicked += HandleInventorySortTypeClicked;
        InventoryGrid.OnInventorySlotHoverEnd += HandleInventorySlotHoverEnd;
    }

    public override void OnFinalize()
    {
        LogDebug("[CrpgInventoryVM] Finalizing CrpgInventoryViewModel");

        _equipmentPanel.OnItemDropped -= HandleItemDrop;
        EquipmentPanel.OnSlotAlternateClicked -= HandleAlternateClick;
        _equipmentPanel.OnSlotClicked -= HandleItemClick;

        UserLoadoutBehavior = Mission.Current?.GetMissionBehavior<CrpgCharacterLoadoutBehaviorClient>();
        if (UserLoadoutBehavior != null)
        {
            UserLoadoutBehavior.OnEquipmentSlotUpdated -= HandleEquipmentSlotUpdated;
            UserLoadoutBehavior.OnUserInventoryUpdated -= HandleInventoryUpdated;
            UserLoadoutBehavior.OnClanArmoryUpdated -= HandleClanArmoryUpdated;
            UserLoadoutBehavior.OnUserCharacterEquippedItemsUpdated -= HandleEquippedItemsUpdated;
            UserLoadoutBehavior.OnUserCharacterBasicUpdated -= HandleUserCharacterBasicUpdated;
            UserLoadoutBehavior.OnUserCharacteristicsUpdated -= HandleUserCharacteristicsUpdated;
            UserLoadoutBehavior.OnUserCharacteristicsConverted -= HandleUserCharacteristicsConverted;
            UserLoadoutBehavior.OnArmoryActionUpdated -= HandleArmoryUserItemUpdated;
        }

        InventoryGrid.OnInventorySlotClicked -= HandleInventorySlotClicked;
        InventoryGrid.OnInventorySortTypeClicked -= HandleInventorySortTypeClicked;
        InventoryGrid.OnInventorySlotHoverEnd -= HandleInventorySlotHoverEnd;
    }

    public void SetRootWidget(Widget rootWidget)
    {
        RootWidget = rootWidget;
        LogDebug("[CrpgInventoryVM] RootWidget set");

    }

    public void SetContext(UIContext context)
    {
        _context = context;
        MakeItemInfo();
    }

    private void ExecuteClickOpenCharacteristicsEditor()
    {
        LogDebug($"[CrpgInventoryVM] ExecuteClickOpenCharacteristicsEditor");
    }

    private void ShowCharacteristicsEditor(bool show)
    {
        if (!show)
        {

        }
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
                LogDebugError($"[CrpgInventoryVM] ItemObj is null");
                return;
            }

            ItemInfo = new ItemInfoVM(itemObj, userItemId);
            ItemInfo.GenerateItemInfo(itemObj, userItemId);

            var mousePos = Input.MousePositionPixel; // vec2

            ItemInfo.PositionX = mousePos.X + 25;
            ItemInfo.PositionY = mousePos.Y - 100;
            ItemInfo.IsVisible = true;

            LogDebug($"[CrpgInventoryVM] ShowItemInfoPopup at ({mousePos.X}, {mousePos.Y}) for {itemObj?.Name}");
        }
        else
        {
            LogDebugError($"[CrpgInventoryVM] userItemExtended is null");
        }
    }

    public void ExecuteDropDiscard(ViewModel draggedItem, int index)
    {
        LogDebug("[CrpgInventoryVM] ExecuteDropDiscard");
        if (draggedItem is EquipmentSlotVM eqSlot)
        {
            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(new UserRequestEquipCharacterItem { Slot = eqSlot.CrpgItemSlotIndex, UserItemId = -1 });
            GameNetwork.EndModuleEventAsClient();
        }
    }

    private void HandleArmoryUserItemUpdated(ClanArmoryActionType action, int uItemId)
    {
        InformationManager.DisplayMessage(new InformationMessage($"[CrpgInventoryViewModel] HandleArmoryUserItemUpdated() mode: {action} userItemId:{uItemId} "));

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

    private void HandleInventorySortTypeClicked(InventorySortTypeVM sortType)
    {
        LogDebug($"[CrpgInventoryVM] HandleInventorySortTypeClicked");
        if (ItemInfo.IsVisible)
        {
            ItemInfo.IsVisible = false;
        }
    }

    private void HandleInventorySlotClicked(InventorySlotVM slot)
    {
        InformationManager.DisplayMessage(new InformationMessage($"[CrpgInventoryViewModel] HandleInventorySlotClicked() {slot.ItemName} "));
        ShowItemInfoPopup(true, slot.ItemObj, slot.UserItemId);

        /*

                if (RootWidget != null)
                {
                    var itemInfoWidget = FindChildById(RootWidget, "ItemInfoId");
                    if (itemInfoWidget != null)
                    {
                        InformationManager.DisplayMessage(new InformationMessage($"[CrpgInventoryViewModel] ITEMINFOWIDGET FOUND!!!"));
                    }
                }


                ItemInfo.PositionX = mousePos.X + 25;
                ItemInfo.PositionY = mousePos.Y - 100;
                ItemInfo.IsVisible = true;

                LogDebug($"[CrpgInventoryVM] Showing ItemInfo at ({mousePos.X}, {mousePos.Y}) for {slot.ItemName}");
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
            LogDebugError($"[CrpgInventoryVM] Inventory item dropped: {inv.ItemName} (UserItemId: {userItemId}) into slot {targetSlot.CrpgItemSlotIndex}");
        }
        else if (draggedItem is EquipmentSlotVM eq)
        {
            // 🔹 Prevent dropping onto the same slot or empty item
            if (eq.CrpgItemSlotIndex == targetSlot.CrpgItemSlotIndex || eq.ItemObj == null)
            {
                LogDebug("[CrpgInventoryVM] Ignored drop: same slot or empty item dropped");
                return;
            }

            userItemId = eq.UserItemId;
            draggedItemObject = eq.ItemObj;
            sourceEquipmentSlot = eq;

            LogDebug($"[CrpgInventoryVM] Equipment item dropped: {eq.ItemObj?.Name} (UserItemId: {userItemId}) into slot {targetSlot.CrpgItemSlotIndex}");
        }
        else
        {
            LogDebugError("[CrpgInventoryVM] Dropped unknown item type");
            return;
        }

        if (draggedItemObject == null)
        {
            LogDebugError("[CrpgInventoryVM] No Object to drag");
            return;
        }

        // Prevent equipping an item that does not fit the target slot
        if (!Equipment.IsItemFitsToSlot(EquipmentSlotVM.ConvertToEquipmentIndex(targetSlot.CrpgItemSlotIndex), draggedItemObject))
        {
            LogDebug($"[CrpgInventoryVM] {draggedItemObject.Name} cannot be equipped in slot {targetSlot.CrpgItemSlotIndex}");
            return;
        }

        // Prevent equipping the same item into the same slot
        if (draggedItemObject == targetSlot.ItemObj && userItemId == targetSlot.UserItemId)
        {
            LogDebug("[CrpgInventoryVM] Ignored drop: item already equipped in target slot");
            return;
        }

        // ✅ Send equip request
        LogDebug("[CrpgInventoryVM] Sending equip request to server");
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
            LogDebug($"[CrpgInventoryVM] HandleItemDragBegin for item: {itemObj?.Name}");

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
        LogDebug("[CrpgInventoryVM] HandleItemDragEnd - resetting equipment slot states");

        foreach (var slotVm in EquipmentPanel.EquipmentSlots)
        {
            slotVm.IsButtonEnabled = true;
        }
    }

    private void HandleAlternateClick(EquipmentSlotVM slot)
    {
        LogDebug($"[CrpgInventoryVM] Alternate click on slot: {slot.CrpgItemSlotIndex}");
        GameNetwork.BeginModuleEventAsClient();
        GameNetwork.WriteMessage(new UserRequestEquipCharacterItem { Slot = slot.CrpgItemSlotIndex, UserItemId = -1 });
        GameNetwork.EndModuleEventAsClient();
    }

    private void HandleItemClick(EquipmentSlotVM slot)
    {
        InformationManager.DisplayMessage(new InformationMessage($"[CrpgInventoryViewModel] HandleItemClick() "));
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
        LogDebug("[CrpgInventoryVM] HandleInventoryUpdated called");

        var behavior = Mission.Current?.GetMissionBehavior<CrpgCharacterLoadoutBehaviorClient>();
        if (behavior == null)
        {
            LogDebug("[CrpgInventoryVM] No CrpgCharacterLoadoutBehaviorClient found", Colors.Red);
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
        LogDebug("[CrpgInventoryVM] HandleClanArmoryUpdated called");

        var behavior = Mission.Current?.GetMissionBehavior<CrpgCharacterLoadoutBehaviorClient>();
        if (behavior == null)
        {
            LogDebug("[CrpgInventoryVM] No CrpgCharacterLoadoutBehaviorClient found", Colors.Red);
            return;
        }

        if (behavior.ClanArmoryItems == null || behavior.ClanArmoryItems.Count == 0)
        {
            LogDebug("[CrpgInventoryVM] No clan armory items available.");
            InventoryGrid.SetArmoryItems(Array.Empty<(ItemObject, int, CrpgUserItemExtended)>());
            return;
        }

        var items = behavior.ClanArmoryItems
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
        LogDebug("[CrpgInventoryVM] HandleEquippedItemsUpdated called");

        var behavior = Mission.Current?.GetMissionBehavior<CrpgCharacterLoadoutBehaviorClient>();
        if (behavior == null)
        {
            LogDebug("[CrpgInventoryVM] No CrpgCharacterLoadoutBehaviorClient found", Colors.Red);
            return;
        }

        foreach (var slotVm in EquipmentPanel.EquipmentSlots)
        {
            var equippedItem = behavior.EquippedItems.FirstOrDefault(e => e.Slot == slotVm.CrpgItemSlotIndex);
            if (equippedItem != null)
            {
                var itemObj = MBObjectManager.Instance.GetObject<ItemObject>(equippedItem.UserItem.ItemId);
                slotVm.SetItem(new ImageIdentifierVM(itemObj), itemObj, equippedItem.UserItem);
                // LogDebug($"[CrpgInventoryVM] Updated slot {slotVm.CrpgItemSlotIndex} with item {itemObj?.Name}");
                //InformationManager.DisplayMessage(new InformationMessage($"{equippedItem.UserItem.ItemId} - IsArmoryItem:{equippedItem.UserItem.IsArmoryItem}", Colors.Cyan));
            }
            else
            {
                slotVm.ClearItem();
                // LogDebug($"[CrpgInventoryVM] Cleared slot {slotVm.CrpgItemSlotIndex}");
            }
        }

        var equipment = behavior.GetCrpgUserCharacterEquipment();
        EquipmentPanel.RefreshCharacterPreview(equipment);

        var characteristics = behavior.UserCharacter.Characteristics;
        CharacterInfoBuildEquipStatsVm.UpdateCharacterBuildEquipmentStatDisplay(equipment, characteristics);
    }

    private void HandleUserCharacterBasicUpdated()
    {
        // LogDebug("[CprgInventoryVM] HandleUserCharacterBasicUpdated called");

        var behavior = Mission.Current?.GetMissionBehavior<CrpgCharacterLoadoutBehaviorClient>();
        if (behavior == null)
        {
            LogDebug("[CrpgInventoryVM] No CrpgCharacterLoadoutBehaviorClient found", Colors.Red);
            return;
        }

        CharacteristicsEditor.SetCrpgCharacterBasic(behavior.UserCharacter);
    }

    private void HandleUserCharacteristicsUpdated()
    {
        LogDebug("[CprgInventoryVM] HandleUserCharacteristicsUpdated called");

        var behavior = Mission.Current?.GetMissionBehavior<CrpgCharacterLoadoutBehaviorClient>();
        if (behavior == null)
        {
            LogDebug("[CrpgInventoryVM] No CrpgCharacterLoadoutBehaviorClient found", Colors.Red);
            return;
        }

        CharacteristicsEditor.SetInitialCharacteristics(behavior.UserCharacter.Characteristics);
        LogDebug("[CrpgInventoryVm] Updated CHaracterInfoVM with new characteristics");
    }

    private void HandleUserCharacteristicsConverted()
    {
        LogDebug("[CrpgInventoryVM] HandleUserCharacteristicsConverted called");

        var behavior = Mission.Current?.GetMissionBehavior<CrpgCharacterLoadoutBehaviorClient>();
        if (behavior == null)
        {
            LogDebugError("[CrpgInventoryVM] No CrpgCharacterLoadoutBehaviorClient found");
            return;
        }

        CharacteristicsEditor.UpdateCharacteristicsPointsAfterConversion(
            behavior.UserCharacter.Characteristics.Attributes.Points,
            behavior.UserCharacter.Characteristics.Skills.Points);

        LogDebug("[CrpgInventoryVM] HandleUserCharacteristicsConverted finished.");
    }

    private void MakeItemInfo()
    {
        if (RootWidget == null)
        {
            LogDebug("[CrpgInventoryVM] Widget is NULL");
            return;
        }

        Widget userInventoryGridWidget = RootWidget.FindChild("UserInventoryGrid");

        if (userInventoryGridWidget != null)
        {
            LogDebug("[CrpgInventoryVM] userInventoryGridWidget found");
        }
        else
        {
            LogDebug("[CrpgInventoryVM] userInventoryGridWidget NOT FOUND", Colors.Red);
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
        LogDebug($"{GetType().Name} {message}", Colors.Red);
    }

    private void LogDebug(string message, Color color)
    {
        Debug.Print(message);
        InformationManager.DisplayMessage(new InformationMessage(message, color));
    }
}
