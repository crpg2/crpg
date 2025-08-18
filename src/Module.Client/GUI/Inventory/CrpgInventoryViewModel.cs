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
    private List<EquipmentSlotVM> _equipmentSlots;

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

    // Armor amounts remain unchanged
    [DataSourceProperty] public ArmorAmountVM AmountHeadArmor { get; set; }
    [DataSourceProperty] public ArmorAmountVM AmountBodyArmor { get; set; }
    [DataSourceProperty] public ArmorAmountVM AmountLegArmor { get; set; }
    [DataSourceProperty] public ArmorAmountVM AmountHandArmor { get; set; }
    [DataSourceProperty] public ArmorAmountVM AmountHorseArmor { get; set; }

    public CrpgInventoryViewModel()
    {
        _characterPreview = new CharacterViewModel();
        CrpgInventoryBehaviorClient.OnUserInventoryUpdated += UpdateAvailableItems;

        InventoryGrid = new InventoryGridVM();
        UpdateAvailableItems();

        // Initialize slots with CrpgItemSlot
        HeadArmor = GetImageIdentifierFromCrpgSlot(CrpgItemSlot.Head);
        CapeArmor = GetImageIdentifierFromCrpgSlot(CrpgItemSlot.Shoulder);
        BodyArmor = GetImageIdentifierFromCrpgSlot(CrpgItemSlot.Body);
        HandArmor = GetImageIdentifierFromCrpgSlot(CrpgItemSlot.Hand);
        LegArmor = GetImageIdentifierFromCrpgSlot(CrpgItemSlot.Leg);

        Weapon0 = GetImageIdentifierFromCrpgSlot(CrpgItemSlot.Weapon0);
        Weapon1 = GetImageIdentifierFromCrpgSlot(CrpgItemSlot.Weapon1);
        Weapon2 = GetImageIdentifierFromCrpgSlot(CrpgItemSlot.Weapon2);
        Weapon3 = GetImageIdentifierFromCrpgSlot(CrpgItemSlot.Weapon3);
        ExtraWeaponSlot = GetImageIdentifierFromCrpgSlot(CrpgItemSlot.WeaponExtra);

        Horse = GetImageIdentifierFromCrpgSlot(CrpgItemSlot.Mount);
        HorseArmor = GetImageIdentifierFromCrpgSlot(CrpgItemSlot.MountHarness);

        _equipmentSlots = new List<EquipmentSlotVM>
            {
                HeadArmor, CapeArmor, BodyArmor, HandArmor, LegArmor,
                Weapon0, Weapon1, Weapon2, Weapon3, ExtraWeaponSlot,
                Horse, HorseArmor,
            };

        SetEquipmentSlotEventSubscriptions(true);

        AmountHeadArmor = new ArmorAmountVM(0);
        AmountBodyArmor = new ArmorAmountVM(0);
        AmountLegArmor = new ArmorAmountVM(0);
        AmountHandArmor = new ArmorAmountVM(0);
        AmountHorseArmor = new ArmorAmountVM(0);

        InitializeCharacterPreview();
        RefreshCharacterPreview();
    }

    private void InitializeCharacterPreview()
    {
        var basicCharacter = MBObjectManager.Instance.GetObject<BasicCharacterObject>("mp_character");
        if (basicCharacter == null)
        {
            InformationManager.DisplayMessage(new InformationMessage("Failed to find 'mp_character'."));
            return;
        }

        var virtualPlayer = GameNetwork.MyPeer?.VirtualPlayer;
        if (virtualPlayer != null)
        {
            basicCharacter.UpdatePlayerCharacterBodyProperties(
                virtualPlayer.BodyProperties,
                virtualPlayer.Race,
                virtualPlayer.IsFemale);

            basicCharacter.Age = virtualPlayer.BodyProperties.Age;
        }

        CharacterPreview.FillFrom(basicCharacter);

        var crpgUser = GameNetwork.MyPeer?.GetComponent<CrpgPeer>()?.User;
        if (crpgUser != null)
        {
            var equipment = CrpgCharacterBuilder.CreateCharacterEquipment(crpgUser.Character.EquippedItems);
            CharacterPreview.EquipmentCode = equipment.CalculateEquipmentCode();
        }
    }

    public override void OnFinalize()
    {
        SetEquipmentSlotEventSubscriptions(false);
    }

    public void SetEquipmentSlotEventSubscriptions(bool subscribe)
    {
        foreach (var slot in _equipmentSlots)
        {
            if (subscribe)
            {
                slot.PropertyChanged += EquipmentSlot_PropertyChanged;
            }
            else
            {
                slot.PropertyChanged -= EquipmentSlot_PropertyChanged;
            }
        }
    }

    private EquipmentSlotVM GetImageIdentifierFromCrpgSlot(CrpgItemSlot slot)
    {
        var crpgUser = GameNetwork.MyPeer?.GetComponent<CrpgPeer>()?.User;
        var vm = new EquipmentSlotVM(new ImageIdentifierVM(ImageIdentifierType.Item), slot);

        if (crpgUser?.Character == null)
        {
            return vm;
        }

        // Find the equipped item that matches this slot
        var equippedItem = crpgUser.Character.EquippedItems
            .FirstOrDefault(e => e.Slot == slot);

        if (equippedItem != null)
        {
            var itemObject = MBObjectManager.Instance.GetObject<ItemObject>(equippedItem.UserItem.ItemId);
            if (itemObject != null)
            {
                vm.ImageIdentifier = new ImageIdentifierVM(itemObject);
                vm.UserItemId = equippedItem.UserItem.Id; // This is your unique instance ID
                vm.ItemObj = itemObject;
            }
        }

        return vm;
    }

    private void UpdateAvailableItems()
    {
        var inventoryBehavior = Mission.Current?.GetMissionBehavior<CrpgInventoryBehaviorClient>();
        if (inventoryBehavior == null)
        {
            InformationManager.DisplayMessage(new InformationMessage("CrpgInventoryBehaviorClient not found in current mission."));
            return;
        }

        var userItems = inventoryBehavior.UserInventoryItems;
        if (userItems == null || userItems.Count == 0)
            return;

        var inventoryTuples = new List<(ItemObject, int, int)>();

        foreach (var userItem in userItems)
        {
            if (string.IsNullOrEmpty(userItem.ItemId))
                continue;

            var itemObj = MBObjectManager.Instance.GetObject<ItemObject>(userItem.ItemId);
            if (itemObj == null)
                continue;

            inventoryTuples.Add((itemObj, 1, userItem.Id));
        }

        InventoryGrid.SetAvailableItems(inventoryTuples);
        InventoryGrid.InitializeFilteredItemsList();
        OnPropertyChanged(nameof(InventoryGrid));
    }

    private void EquipmentSlot_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(EquipmentSlotVM.ItemObj))
        {
            var equipment = BuildEquipmentFromSlots();
            RefreshCharacterPreview(equipment);
            CalculateArmorFromEquipment(equipment);
        }
    }

    public Equipment BuildEquipmentFromSlots()
    {
        var equipment = new Equipment(false);

        foreach (var slot in _equipmentSlots)
        {
            var equipIndex = EquipmentSlotVM.ConvertToEquipmentIndex(slot.CrpgItemSlotIndex);

            if (slot.ItemObj != null)
                equipment[equipIndex] = new EquipmentElement(slot.ItemObj);
            else
                equipment[equipIndex] = EquipmentElement.Invalid;
        }

        return equipment;
    }

    private void CalculateArmorFromEquipment(Equipment equipment)
    {
        AmountHeadArmor.ArmorAmount = (int)equipment.GetHeadArmorSum();
        AmountBodyArmor.ArmorAmount = (int)equipment.GetHumanBodyArmorSum();
        AmountHandArmor.ArmorAmount = (int)equipment.GetArmArmorSum();
        AmountLegArmor.ArmorAmount = (int)equipment.GetLegArmorSum();
        AmountHorseArmor.ArmorAmount = (int)equipment.GetHorseArmorSum();
    }

    public void RefreshCharacterPreview(Equipment? useEquipment = null)
    {
        var behavior = Mission.Current.GetMissionBehavior<CrpgInventoryBehaviorClient>();
        if (behavior != null && useEquipment == null)
        {
            useEquipment = behavior.GetEquipment();
        }

        if (useEquipment != null)
        {
            CharacterPreview.EquipmentCode = useEquipment.CalculateEquipmentCode();
            CalculateArmorFromEquipment(useEquipment);
        }
    }
}
