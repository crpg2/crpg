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
    private List<ItemObject> _allItems;
    private List<EquipmentSlotVM> _equipmentSlots;

    [DataSourceProperty]
    public InventoryGridVM InventoryGrid { get; set; }

    // testing
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
        get
        {
            return _isVisible;
        }
        set
        {
            if (value != _isVisible)
            {
                _isVisible = value;
                OnPropertyChangedWithValue(value, "IsVisible");
            }
        }
    }

    [DataSourceProperty]
    public CharacterViewModel CharacterPreview
    {
        get
        {
            return _characterPreview;
        }
        set
        {
            if (value != _characterPreview)
            {
                _characterPreview = value;
                OnPropertyChangedWithValue(value, "CharacterPreview");
            }
        }
    }

    [DataSourceProperty]
    public ArmorAmountVM AmountHeadArmor
    { get; set; }
    [DataSourceProperty]
    public ArmorAmountVM AmountBodyArmor
    { get; set; }
    [DataSourceProperty]
    public ArmorAmountVM AmountLegArmor
    { get; set; }
    [DataSourceProperty]
    public ArmorAmountVM AmountHandArmor
    { get; set; }
    [DataSourceProperty]
    public ArmorAmountVM AmountHorseArmor
    { get; set; }

    public CrpgInventoryViewModel()
    {
        Agent? agentMain = Agent.Main;
        _characterPreview = new CharacterViewModel();

        _allItems = Game.Current.ObjectManager.GetObjectTypeList<ItemObject>().ToList();

        InventoryGrid = new InventoryGridVM();
        var inventory = _allItems;
        InventoryGrid.SetAvailableItems(inventory.Select(item => (item, 1)));
        InventoryGrid.InitializeFilteredItemsList();

        OnPropertyChanged(nameof(InventoryGrid));

        InformationManager.DisplayMessage(new InformationMessage($"Inventory items: {InventoryGrid.AvailableItems.Count}"));

        HeadArmor = GetImageIdentifierFromEquipment(EquipmentIndex.Head);
        CapeArmor = GetImageIdentifierFromEquipment(EquipmentIndex.Cape);
        BodyArmor = GetImageIdentifierFromEquipment(EquipmentIndex.Body);
        HandArmor = GetImageIdentifierFromEquipment(EquipmentIndex.Gloves);
        LegArmor = GetImageIdentifierFromEquipment(EquipmentIndex.Leg);

        Weapon0 = GetImageIdentifierFromEquipment(EquipmentIndex.Weapon0);
        Weapon1 = GetImageIdentifierFromEquipment(EquipmentIndex.Weapon1);
        Weapon2 = GetImageIdentifierFromEquipment(EquipmentIndex.Weapon2);
        Weapon3 = GetImageIdentifierFromEquipment(EquipmentIndex.Weapon3);
        ExtraWeaponSlot = GetImageIdentifierFromEquipment(EquipmentIndex.ExtraWeaponSlot);

        Horse = GetImageIdentifierFromEquipment(EquipmentIndex.Horse);
        HorseArmor = GetImageIdentifierFromEquipment(EquipmentIndex.HorseHarness);

        // Create list with all slots
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

        // Get the dummy BasicCharacterObject used for the player character
        var basicCharacter = MBObjectManager.Instance.GetObject<BasicCharacterObject>("mp_character");
        if (basicCharacter == null)
        {
            InformationManager.DisplayMessage(new InformationMessage("Failed to find 'mp_character' BasicCharacterObject."));
            return;
        }

        NetworkCommunicator? myPeer = GameNetwork.MyPeer;
        VirtualPlayer? virtualPlayer = myPeer?.VirtualPlayer;

        if (virtualPlayer != null)
        {
            basicCharacter.UpdatePlayerCharacterBodyProperties(
                virtualPlayer.BodyProperties,
                virtualPlayer.Race,
                virtualPlayer.IsFemale);

            basicCharacter.Age = virtualPlayer.BodyProperties.Age;
        }

        CharacterPreview.FillFrom(basicCharacter);

        var crpgUser = myPeer?.GetComponent<CrpgPeer>()?.User;
        if (crpgUser != null)
        {
            var equipment = CrpgCharacterBuilder.CreateCharacterEquipment(crpgUser.Character.EquippedItems);
            CharacterPreview.EquipmentCode = equipment.CalculateEquipmentCode();
        }

        RefreshCharacterPreview();
    }

    public override void OnFinalize()
    {
        SetEquipmentSlotEventSubscriptions(false);
        // Any other cleanup needed here
    }
    public void Tick()
    {

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

    public void RefreshCharacterPreview(Equipment? useEquipment = null)
    {
        if (CharacterPreview == null)
        {
            return;
        }

        var crpgPeer = GameNetwork.MyPeer.GetComponent<CrpgPeer>();
        if (crpgPeer == null)
        {
            InformationManager.DisplayMessage(new InformationMessage("crpgPeer is null."));
            return;
        }

        var crpgUser = crpgPeer.User;
        if (crpgUser == null)
        {
            InformationManager.DisplayMessage(new InformationMessage("crpgUser is null."));
            return;
        }

        if (useEquipment == null)
        {
            useEquipment = CrpgCharacterBuilder.CreateCharacterEquipment(crpgUser.Character.EquippedItems);
        }

        CharacterPreview.EquipmentCode = useEquipment.CalculateEquipmentCode();

        CalculateArmorFromEquipment(useEquipment);

        // CharacterPreview.BannerCodeText = agent?.Origin?.Banner?.Serialize() ?? string.Empty;
    }

    private EquipmentSlotVM GetImageIdentifierFromEquipment(EquipmentIndex index)
    {
        var crpgPeer = GameNetwork.MyPeer.GetComponent<CrpgPeer>();
        if (crpgPeer?.User?.Character == null)
        {
            return new EquipmentSlotVM(new ImageIdentifierVM(ImageIdentifierType.Item), index);
        }

        Equipment equipment = CrpgCharacterBuilder.CreateCharacterEquipment(crpgPeer.User.Character.EquippedItems);
        EquipmentElement element = equipment[index];

        if (!element.IsEmpty && element.Item != null)
        {
            return new EquipmentSlotVM(new ImageIdentifierVM(element.Item), index);
        }

        return new EquipmentSlotVM(new ImageIdentifierVM(ImageIdentifierType.Item), index);
    }

    private void CalculateArmorFromEquipment(Equipment equipment)
    {
        AmountHeadArmor.ArmorAmount = 0;
        AmountBodyArmor.ArmorAmount = 0;
        AmountHandArmor.ArmorAmount = 0;
        AmountLegArmor.ArmorAmount = 0;
        AmountHorseArmor.ArmorAmount = 0;

        if (equipment == null)
        {
            InformationManager.DisplayMessage(new InformationMessage("Equipment is invalid or empty."));
            return;
        }

        AmountHeadArmor.ArmorAmount = (int)equipment.GetHeadArmorSum();
        AmountBodyArmor.ArmorAmount = (int)equipment.GetHumanBodyArmorSum();
        AmountHandArmor.ArmorAmount = (int)equipment.GetArmArmorSum();
        AmountLegArmor.ArmorAmount = (int)equipment.GetLegArmorSum();
        AmountHorseArmor.ArmorAmount = (int)equipment.GetHorseArmorSum();
    }

    public Equipment BuildEquipmentFromSlots()
    {
        // Create Equipment with "false" to indicate not empty
        Equipment equipment = new(false);

        foreach (var slot in _equipmentSlots)
        {
            if (slot.ImageIdentifier != null && slot.ItemObj != null)
            {
                equipment[slot.EquipmentSlot] = new EquipmentElement(slot.ItemObj);
            }
            else
            {
                equipment[slot.EquipmentSlot] = EquipmentElement.Invalid;
            }
        }

        return equipment;
    }

    private void EquipmentSlot_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(EquipmentSlotVM.ItemObj))
        {
            InformationManager.DisplayMessage(new InformationMessage("ItemObj changed"));
            Equipment equipment = BuildEquipmentFromSlots();
            RefreshCharacterPreview(equipment);
            CalculateArmorFromEquipment(equipment);
        }
    }
}
