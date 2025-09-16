using System.Runtime.InteropServices;
using Crpg.Module.Api.Models.Items;
using Crpg.Module.Common;
using Crpg.Module.Common.Network;
using Crpg.Module.Helpers;
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
    private readonly List<EquipmentSlotVM> _equipmentSlots;
    private bool _isVisible;
    private CharacterViewModel _characterPreview;
    private ItemInfoVM _itemInfo;


    [DataSourceProperty]
    public InventoryGridVM InventoryGrid { get; set; }

    public EquipmentSlotVM HeadArmor { get; private set; }
    public EquipmentSlotVM CapeArmor { get; private set; }
    public EquipmentSlotVM BodyArmor { get; private set; }
    public EquipmentSlotVM HandArmor { get; private set; }
    public EquipmentSlotVM LegArmor { get; private set; }
    public EquipmentSlotVM Weapon0 { get; private set; }
    public EquipmentSlotVM Weapon1 { get; private set; }
    public EquipmentSlotVM Weapon2 { get; private set; }
    public EquipmentSlotVM Weapon3 { get; private set; }
    public EquipmentSlotVM ExtraWeaponSlot { get; private set; }
    public EquipmentSlotVM Horse { get; private set; }
    public EquipmentSlotVM HorseArmor { get; private set; }

    [DataSourceProperty]
    public ItemInfoVM ItemInfo
    {
        get => _itemInfo;
        set => SetField(ref _itemInfo, value, nameof(ItemInfo));
    }

    [DataSourceProperty]
    public ArmorAmountVM AmountHeadArmor { get; set; }

    [DataSourceProperty]
    public ArmorAmountVM AmountBodyArmor { get; set; }

    [DataSourceProperty]
    public ArmorAmountVM AmountLegArmor { get; set; }

    [DataSourceProperty]
    public ArmorAmountVM AmountHandArmor { get; set; }

    [DataSourceProperty]
    public ArmorAmountVM AmountHorseArmor { get; set; }

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

    private CharacterInfoVM _characterInfo;
    [DataSourceProperty]
    public CharacterInfoVM CharacterInfo
    {
        get => _characterInfo;
        set => SetField(ref _characterInfo, value, nameof(CharacterInfo));
    }

    private CharacterInfoBuildEquipStatsVM _characterInfoBuildEquipStatsVm;
    [DataSourceProperty]
    public CharacterInfoBuildEquipStatsVM CharacterInfoBuildEquipStatsVm { get => _characterInfoBuildEquipStatsVm; set => SetField(ref _characterInfoBuildEquipStatsVm, value, nameof(CharacterInfoBuildEquipStatsVm)); }

    [DataSourceProperty]
    public CharacterViewModel CharacterPreview
    {
        get => _characterPreview;
        set
        {
            if (_characterPreview != value)
            {
                _characterPreview = value;
                OnPropertyChangedWithValue(value, nameof(CharacterPreview));
                LogDebug("[CrpgInventoryVM] CharacterPreview updated");
            }
        }
    }

    public CrpgInventoryViewModel()
    {
        _characterInfo = new CharacterInfoVM();
        _characterInfoBuildEquipStatsVm = new CharacterInfoBuildEquipStatsVM();

        _characterPreview = new CharacterViewModel();
        InventoryGrid = new InventoryGridVM();

        _equipmentSlots = new List<EquipmentSlotVM>();

        HeadArmor = CreateSlot(CrpgItemSlot.Head);
        CapeArmor = CreateSlot(CrpgItemSlot.Shoulder);
        BodyArmor = CreateSlot(CrpgItemSlot.Body);
        HandArmor = CreateSlot(CrpgItemSlot.Hand);
        LegArmor = CreateSlot(CrpgItemSlot.Leg);

        Weapon0 = CreateSlot(CrpgItemSlot.Weapon0);
        Weapon1 = CreateSlot(CrpgItemSlot.Weapon1);
        Weapon2 = CreateSlot(CrpgItemSlot.Weapon2);
        Weapon3 = CreateSlot(CrpgItemSlot.Weapon3);
        ExtraWeaponSlot = CreateSlot(CrpgItemSlot.WeaponExtra);

        Horse = CreateSlot(CrpgItemSlot.Mount);
        HorseArmor = CreateSlot(CrpgItemSlot.MountHarness);

        AmountHeadArmor = new ArmorAmountVM(0);
        AmountBodyArmor = new ArmorAmountVM(0);
        AmountLegArmor = new ArmorAmountVM(0);
        AmountHandArmor = new ArmorAmountVM(0);
        AmountHorseArmor = new ArmorAmountVM(0);

        _itemInfo = new ItemInfoVM(null);
        InitializeCharacterPreview();

        LogDebug("[CrpgInventoryVM] Subscribing to CrpgCharacterLoadoutBehaviorClient events");
        var behavior = Mission.Current?.GetMissionBehavior<CrpgCharacterLoadoutBehaviorClient>();
        if (behavior != null)
        {
            behavior.OnSlotUpdated += HandleSlotUpdated;
            behavior.OnUserInventoryUpdated += HandleInventoryUpdated;
            behavior.OnUserCharacterEquippedItemsUpdated += HandleEquippedItemsUpdated;
            behavior.OnUserCharacterBasicUpdated += HandleUserCharacterBasicUpdated;
            behavior.OnUserCharacteristicsUpdated += HandleUserCharacteristicsUpdated;
            behavior.OnUserCharacteristicsConverted += HandleUserCharacteristicsConverted;
        }

        InventoryGrid.OnInventorySlotClicked += HandleInventorySlotClicked;
        InventoryGrid.OnInventorySortTypeClicked += HandleInventorySortTypeClicked;
        InventoryGrid.OnInventorySlotHoverEnd += HandleInventorySlotHoverEnd;
    }

    public override void OnFinalize()
    {
        LogDebug("[CrpgInventoryVM] Finalizing CrpgInventoryViewModel");
        var behavior = Mission.Current?.GetMissionBehavior<CrpgCharacterLoadoutBehaviorClient>();
        if (behavior != null)
        {
            behavior.OnSlotUpdated -= HandleSlotUpdated;
            behavior.OnUserInventoryUpdated -= HandleInventoryUpdated;
            behavior.OnUserCharacterEquippedItemsUpdated -= HandleEquippedItemsUpdated;
            behavior.OnUserCharacterBasicUpdated -= HandleUserCharacterBasicUpdated;
            behavior.OnUserCharacteristicsUpdated -= HandleUserCharacteristicsUpdated;
            behavior.OnUserCharacteristicsConverted -= HandleUserCharacteristicsConverted;
        }

        InventoryGrid.OnInventorySlotClicked -= HandleInventorySlotClicked;
        InventoryGrid.OnInventorySortTypeClicked -= HandleInventorySortTypeClicked;
        InventoryGrid.OnInventorySlotHoverEnd -= HandleInventorySlotHoverEnd;
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

    internal void RefreshCharacterPreview(Equipment? useEquipment = null)
    {
        var behavior = Mission.Current?.GetMissionBehavior<CrpgCharacterLoadoutBehaviorClient>();
        var characteristics = behavior?.UserCharacter.Characteristics;
        if (behavior != null && useEquipment == null)
        {
            useEquipment = behavior.GetCrpgUserCharacterEquipment();
        }
        if (useEquipment != null && characteristics != null)
        {
            CharacterPreview.EquipmentCode = useEquipment.CalculateEquipmentCode();
            CalculateArmorFromEquipment(useEquipment);
            CharacterInfoBuildEquipStatsVm.UpdateCharacterBuildEquipmentStatDisplay(useEquipment, characteristics);

            LogDebug("[CrpgInventoryVM] Refreshed character preview and recalculated armor");

            LogDebug("[CrpgInventoryVM] UpdateCharacterInfo() in refreshcharacterpreview");
        }
        else
        {
            LogDebug("[CrpgInventoryVM] No equipment available to refresh character preview");
        }
    }

    private EquipmentSlotVM CreateSlot(CrpgItemSlot slot)
    {
        var vm = new EquipmentSlotVM(slot);
        _equipmentSlots.Add(vm);
        vm.OnItemDropped += HandleItemDrop;
        vm.OnSlotAlternateClicked += HandleAlternateClick;
        vm.OnItemDragBegin += HandleItemDragBegin;
        vm.OnItemDragEnd += HandleItemDragEnd;
        vm.OnSlotClicked += HandleItemClick;
        LogDebug($"[CrpgInventoryVM] Created equipment slot: {slot}");
        return vm;
    }

    private void LogDebug(string message)
    {
        Debug.Print(message);
        InformationManager.DisplayMessage(new InformationMessage(message));
    }

    private void HandleInventorySortTypeClicked(InventorySortTypeVM sortType)
    {
        LogDebug($"[CrpgInventoryVM] HandleInventorySortTypeClicked");
        if (ItemInfo.IsVisible)
        {
            ItemInfo.IsVisible = false;
        }
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

    private void ShowItemInfoPopup(bool show, ItemObject? itemObj, int? userItemId = null)
    {
        if (!show)
        {
            ItemInfo = new ItemInfoVM(null)
            {
                IsVisible = false,
            };
            return;
        }

        if (userItemId != null)
        {
            var behavior = Mission.Current?.GetMissionBehavior<CrpgCharacterLoadoutBehaviorClient>();

            if (behavior == null)
                return;

            var item = behavior.UserInventoryItems
                .FirstOrDefault(x => x.Id == userItemId.Value);

            if (!userItemId.HasValue || item == null)
            {
                LogDebug($"[CrpgInventoryVM] No item found for userItemId={userItemId.Value}");
                return;
            }

            if (itemObj == null)
            {
                LogDebug($"[CrpgInventoryVM] ItemObj is null");
                return;
            }

            ItemInfo = new ItemInfoVM(itemObj, (int)userItemId);
            ItemInfo.GenerateItemInfo(itemObj);

            var mousePos = Input.MousePositionPixel; // vec2

            ItemInfo.PositionX = mousePos.X + 25;
            ItemInfo.PositionY = mousePos.Y - 100;
            ItemInfo.IsVisible = true;

            LogDebug($"[CrpgInventoryVM] ShowItemInfoPopup at ({mousePos.X}, {mousePos.Y}) for {itemObj?.Name}");
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
            LogDebug($"[CrpgInventoryVM] Inventory item dropped: {inv.ItemName} (UserItemId: {userItemId}) into slot {targetSlot.CrpgItemSlotIndex}");
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
            LogDebug("[CrpgInventoryVM] Dropped unknown item type");
            return;
        }

        if (draggedItemObject == null)
        {
            LogDebug("[CrpgInventoryVM] No Object to drag");
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

            foreach (var slotVm in _equipmentSlots)
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

        foreach (var slotVm in _equipmentSlots)
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
    private void HandleSlotUpdated(CrpgItemSlot updatedSlot)
    {
        LogDebug($"[CrpgInventoryVM] HandleSlotUpdated called for slot {updatedSlot}");

        var behavior = Mission.Current?.GetMissionBehavior<CrpgCharacterLoadoutBehaviorClient>();
        if (behavior == null)
        {
            LogDebug("[CrpgInventoryVM] No CrpgCharacterLoadoutBehaviorClient found");
            return;
        }

        var slotVm = _equipmentSlots.FirstOrDefault(s => s.CrpgItemSlotIndex == updatedSlot);
        if (slotVm == null)
        {
            LogDebug("[CrpgInventoryVM] No matching EquipmentSlotVM found for updated slot");
            return;
        }

        var equippedItem = behavior.EquippedItems.FirstOrDefault(e => e.Slot == updatedSlot);
        if (equippedItem != null)
        {
            var itemObj = MBObjectManager.Instance.GetObject<ItemObject>(equippedItem.UserItem.ItemId);
            slotVm.SetItem(new ImageIdentifierVM(itemObj), itemObj, equippedItem.UserItem.Id);
            LogDebug($"[CrpgInventoryVM] Slot {updatedSlot} set to item {itemObj?.Name} (UserItemId: {equippedItem.UserItem.Id})");
        }
        else
        {
            slotVm.ClearItem();
            LogDebug($"[CrpgInventoryVM] Slot {updatedSlot} cleared");
        }

        RefreshCharacterPreview(behavior.GetCrpgUserCharacterEquipment());
    }

    private void HandleInventoryUpdated()
    {
        LogDebug("[CrpgInventoryVM] HandleInventoryUpdated called");

        var behavior = Mission.Current?.GetMissionBehavior<CrpgCharacterLoadoutBehaviorClient>();
        if (behavior == null)
        {
            LogDebug("[CrpgInventoryVM] No CrpgCharacterLoadoutBehaviorClient found");
            return;
        }

        var items = behavior.UserInventoryItems
            .Where(ui => !string.IsNullOrEmpty(ui.ItemId))
            .Select(ui =>
            {
                var obj = MBObjectManager.Instance.GetObject<ItemObject>(ui.ItemId);
                return obj != null ? (obj, 1, ui.Id) : default;
            })
            .Where(t => t.obj != null)
            .ToList();

        InventoryGrid.SetAvailableItems(items);
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

    private void HandleEquippedItemsUpdated()
    {
        LogDebug("[CrpgInventoryVM] HandleEquippedItemsUpdated called");

        var behavior = Mission.Current?.GetMissionBehavior<CrpgCharacterLoadoutBehaviorClient>();
        if (behavior == null)
        {
            LogDebug("[CrpgInventoryVM] No CrpgCharacterLoadoutBehaviorClient found");
            return;
        }

        foreach (var slotVm in _equipmentSlots)
        {
            var equippedItem = behavior.EquippedItems.FirstOrDefault(e => e.Slot == slotVm.CrpgItemSlotIndex);
            if (equippedItem != null)
            {
                var itemObj = MBObjectManager.Instance.GetObject<ItemObject>(equippedItem.UserItem.ItemId);
                slotVm.SetItem(new ImageIdentifierVM(itemObj), itemObj, equippedItem.UserItem.Id);
                LogDebug($"[CrpgInventoryVM] Updated slot {slotVm.CrpgItemSlotIndex} with item {itemObj?.Name}");
            }
            else
            {
                slotVm.ClearItem();
                LogDebug($"[CrpgInventoryVM] Cleared slot {slotVm.CrpgItemSlotIndex}");
            }
        }

        RefreshCharacterPreview(behavior.GetCrpgUserCharacterEquipment());
    }

    private void HandleUserCharacterBasicUpdated()
    {
        LogDebug("[CprgInventoryVM] HandleUserCharacterBasicUpdated called");

        var behavior = Mission.Current?.GetMissionBehavior<CrpgCharacterLoadoutBehaviorClient>();
        if (behavior == null)
        {
            LogDebug("[CrpgInventoryVM] No CrpgCharacterLoadoutBehaviorClient found");
            return;
        }

        CharacterInfo.SetCrpgCharacterBasic(behavior.UserCharacter);
    }

    private void HandleUserCharacteristicsUpdated()
    {
        LogDebug("[CprgInventoryVM] HandleUserCharacteristicsUpdated called");

        var behavior = Mission.Current?.GetMissionBehavior<CrpgCharacterLoadoutBehaviorClient>();
        if (behavior == null)
        {
            LogDebug("[CrpgInventoryVM] No CrpgCharacterLoadoutBehaviorClient found");
            return;
        }

        CharacterInfo.SetInitialCharacteristics(behavior.UserCharacter.Characteristics);
        LogDebug("[CrpgInventoryVm] Updated CHaracterInfoVM with new characteristics");
    }

    private void HandleUserCharacteristicsConverted()
    {
        LogDebug("[CrpgInventoryVM] HandleUserCharacteristicsConverted called");

        var behavior = Mission.Current?.GetMissionBehavior<CrpgCharacterLoadoutBehaviorClient>();
        if (behavior == null)
        {
            LogDebug("[CrpgInventoryVM] No CrpgCharacterLoadoutBehaviorClient found");
            return;
        }

        CharacterInfo.UpdateCharacteristicsPointsAfterConversion(
            behavior.UserCharacter.Characteristics.Attributes.Points,
            behavior.UserCharacter.Characteristics.Skills.Points);

        LogDebug("[CrpgInventoryVM] HandleUserCharacteristicsConverted finished.");
    }

    private void InitializeCharacterPreview()
    {
        var basicCharacter = MBObjectManager.Instance.GetObject<BasicCharacterObject>("mp_character");
        if (basicCharacter != null)
        {
            CharacterPreview.FillFrom(basicCharacter);
            LogDebug("[CrpgInventoryVM] Initialized character preview with mp_character");
        }
        else
        {
            LogDebug("[CrpgInventoryVM] Failed to find mp_character for preview");
        }
    }

    private void CalculateArmorFromEquipment(Equipment equipment)
    {
        AmountHeadArmor.ArmorAmount = (int)equipment.GetHeadArmorSum();
        AmountBodyArmor.ArmorAmount = (int)equipment.GetHumanBodyArmorSum();
        AmountHandArmor.ArmorAmount = (int)equipment.GetArmArmorSum();
        AmountLegArmor.ArmorAmount = (int)equipment.GetLegArmorSum();
        AmountHorseArmor.ArmorAmount = (int)equipment.GetHorseArmorSum();

        LogDebug("[CrpgInventoryVM] Calculated armor values from equipment");
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
            LogDebug("[CrpgInventoryVM] userInventoryGridWidget NOT FOUND");
        }


    }
}
