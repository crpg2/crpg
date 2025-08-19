using System.Runtime.InteropServices;
using Crpg.Module.Api.Models.Items;
using Crpg.Module.Common;
using Crpg.Module.Helpers;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace Crpg.Module.GUI.Inventory;

public class CrpgInventoryViewModel : ViewModel
{
    private bool _isVisible;
    private CharacterViewModel _characterPreview;
    private readonly List<EquipmentSlotVM> _equipmentSlots;

    [DataSourceProperty] public InventoryGridVM InventoryGrid { get; set; }

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
    public bool IsVisible
    {
        get => _isVisible;
        set
        {
            if (_isVisible != value)
            {
                _isVisible = value;
                OnPropertyChangedWithValue(value, nameof(IsVisible));
            }
        }
    }

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
            }
        }
    }

    [DataSourceProperty] public ArmorAmountVM AmountHeadArmor { get; set; }
    [DataSourceProperty] public ArmorAmountVM AmountBodyArmor { get; set; }
    [DataSourceProperty] public ArmorAmountVM AmountLegArmor { get; set; }
    [DataSourceProperty] public ArmorAmountVM AmountHandArmor { get; set; }
    [DataSourceProperty] public ArmorAmountVM AmountHorseArmor { get; set; }

    public CrpgInventoryViewModel()
    {
        _characterPreview = new CharacterViewModel();
        InventoryGrid = new InventoryGridVM();

        // Initialize EquipmentSlots
        HeadArmor = GetSlotVM(CrpgItemSlot.Head);
        CapeArmor = GetSlotVM(CrpgItemSlot.Shoulder);
        BodyArmor = GetSlotVM(CrpgItemSlot.Body);
        HandArmor = GetSlotVM(CrpgItemSlot.Hand);
        LegArmor = GetSlotVM(CrpgItemSlot.Leg);

        Weapon0 = GetSlotVM(CrpgItemSlot.Weapon0);
        Weapon1 = GetSlotVM(CrpgItemSlot.Weapon1);
        Weapon2 = GetSlotVM(CrpgItemSlot.Weapon2);
        Weapon3 = GetSlotVM(CrpgItemSlot.Weapon3);
        ExtraWeaponSlot = GetSlotVM(CrpgItemSlot.WeaponExtra);

        Horse = GetSlotVM(CrpgItemSlot.Mount);
        HorseArmor = GetSlotVM(CrpgItemSlot.MountHarness);

        _equipmentSlots = new List<EquipmentSlotVM>
        {
            HeadArmor, CapeArmor, BodyArmor, HandArmor, LegArmor,
            Weapon0, Weapon1, Weapon2, Weapon3, ExtraWeaponSlot,
            Horse, HorseArmor
        };

        AmountHeadArmor = new ArmorAmountVM(0);
        AmountBodyArmor = new ArmorAmountVM(0);
        AmountLegArmor = new ArmorAmountVM(0);
        AmountHandArmor = new ArmorAmountVM(0);
        AmountHorseArmor = new ArmorAmountVM(0);

        InitializeCharacterPreview();

        // Subscribe to behavior events
        CrpgInventoryBehaviorClient.OnSlotUpdated += HandleSlotUpdated;
        CrpgInventoryBehaviorClient.OnUserInventoryUpdated += HandleInventoryUpdated;
        CrpgInventoryBehaviorClient.OnUserCharacterEquippedItemsUpdated += HandleEquippedItemsUpdated;
    }

    private EquipmentSlotVM GetSlotVM(CrpgItemSlot slot)
    {
        return new EquipmentSlotVM(new ImageIdentifierVM(ImageIdentifierType.Item), slot);
    }

    private void HandleSlotUpdated(CrpgItemSlot updatedSlot)
    {
        var behavior = Mission.Current?.GetMissionBehavior<CrpgInventoryBehaviorClient>();
        if (behavior == null)
        {
            return;
        }

        // Only touch the VM that corresponds to the updated slot
        var slotVm = _equipmentSlots.FirstOrDefault(s => s.CrpgItemSlotIndex == updatedSlot);
        if (slotVm == null)
        {
            return;
        }

        var equippedItem = behavior.EquippedItems.FirstOrDefault(e => e.Slot == updatedSlot);
        if (equippedItem != null)
        {
            var itemObj = MBObjectManager.Instance.GetObject<ItemObject>(equippedItem.UserItem.ItemId);
            slotVm.SetItem(new ImageIdentifierVM(itemObj), equippedItem.UserItem.Id);
        }
        else
        {
            slotVm.ClearItem();
        }

        // Optionally only refresh if the slot is relevant for the preview
        RefreshCharacterPreview(behavior.GetCrpgUserCharacterEquipment());
    }

    private void HandleInventoryUpdated()
    {
        var behavior = Mission.Current?.GetMissionBehavior<CrpgInventoryBehaviorClient>();
        if (behavior == null)
            return;

        var items = behavior.UserInventoryItems
            .Where(ui => !string.IsNullOrEmpty(ui.ItemId))
            .Select(ui =>
            {
                var obj = MBObjectManager.Instance.GetObject<ItemObject>(ui.ItemId);
                return obj != null ? (obj, 1, ui.Id) : default;
            })
            .Where(t => t.Item1 != null)
            .ToList();

        InventoryGrid.SetAvailableItems(items);
        InventoryGrid.InitializeFilteredItemsList();
        OnPropertyChanged(nameof(InventoryGrid));
    }

    private void HandleEquippedItemsUpdated()
    {
        var behavior = Mission.Current?.GetMissionBehavior<CrpgInventoryBehaviorClient>();
        if (behavior == null)
            return;

        foreach (var slotVm in _equipmentSlots)
        {
            var equippedItem = behavior.EquippedItems.FirstOrDefault(e => e.Slot == slotVm.CrpgItemSlotIndex);
            if (equippedItem != null)
            {
                var itemObj = MBObjectManager.Instance.GetObject<ItemObject>(equippedItem.UserItem.ItemId);
                slotVm.SetItem(new ImageIdentifierVM(itemObj), equippedItem.UserItem.Id);
            }
            else
            {
                slotVm.ClearItem();
            }
        }

        RefreshCharacterPreview(behavior.GetCrpgUserCharacterEquipment());
    }

    private void InitializeCharacterPreview()
    {
        var basicCharacter = MBObjectManager.Instance.GetObject<BasicCharacterObject>("mp_character");
        if (basicCharacter == null)
            return;

        CharacterPreview.FillFrom(basicCharacter);
    }

    public void RefreshCharacterPreview(Equipment? useEquipment = null)
    {
        var behavior = Mission.Current?.GetMissionBehavior<CrpgInventoryBehaviorClient>();
        if (behavior != null && useEquipment == null)
            useEquipment = behavior.GetCrpgUserCharacterEquipment();

        if (useEquipment != null)
        {
            CharacterPreview.EquipmentCode = useEquipment.CalculateEquipmentCode();
            CalculateArmorFromEquipment(useEquipment);
        }
    }

    private void CalculateArmorFromEquipment(Equipment equipment)
    {
        AmountHeadArmor.ArmorAmount = (int)equipment.GetHeadArmorSum();
        AmountBodyArmor.ArmorAmount = (int)equipment.GetHumanBodyArmorSum();
        AmountHandArmor.ArmorAmount = (int)equipment.GetArmArmorSum();
        AmountLegArmor.ArmorAmount = (int)equipment.GetLegArmorSum();
        AmountHorseArmor.ArmorAmount = (int)equipment.GetHorseArmorSum();
    }

    public override void OnFinalize()
    {
        CrpgInventoryBehaviorClient.OnSlotUpdated -= HandleSlotUpdated;
        CrpgInventoryBehaviorClient.OnUserInventoryUpdated -= HandleInventoryUpdated;
        CrpgInventoryBehaviorClient.OnUserCharacterEquippedItemsUpdated -= HandleEquippedItemsUpdated;
    }
}