using Crpg.Module.Api.Models.Items;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

namespace Crpg.Module.GUI.Inventory;

public class EquipmentPanelVM : ViewModel
{
    private bool _isVisible;
    [DataSourceProperty]
    public bool IsVisible { get => _isVisible; set => SetField(ref _isVisible, value, nameof(IsVisible)); }

    private MBBindingList<EquipmentSlotVM> _equipmentSlots = new();

    // Slots
    private EquipmentSlotVM _headArmor = default!;
    private EquipmentSlotVM _capeArmor = default!;
    private EquipmentSlotVM _bodyArmor = default!;
    private EquipmentSlotVM _handArmor = default!;
    private EquipmentSlotVM _legArmor = default!;
    private EquipmentSlotVM _horseArmor = default!;

    private EquipmentSlotVM _weapon0 = default!;
    private EquipmentSlotVM _weapon1 = default!;
    private EquipmentSlotVM _weapon2 = default!;
    private EquipmentSlotVM _weapon3 = default!;
    private EquipmentSlotVM _extraWeaponSlot = default!;
    private EquipmentSlotVM _horse = default!;

    private CharacterViewModel _characterPreview = default!;

    // Armor values
    private ArmorAmountVM _amountHeadArmor = default!;
    private ArmorAmountVM _amountBodyArmor = default!;
    private ArmorAmountVM _amountHandArmor = default!;
    private ArmorAmountVM _amountLegArmor = default!;
    private ArmorAmountVM _amountHorseArmor = default!;

    public EquipmentPanelVM()
    {
        // Initialize slots
        HeadArmor = CreateSlot(CrpgItemSlot.Head);
        CapeArmor = CreateSlot(CrpgItemSlot.Shoulder);
        BodyArmor = CreateSlot(CrpgItemSlot.Body);
        HandArmor = CreateSlot(CrpgItemSlot.Hand);
        LegArmor = CreateSlot(CrpgItemSlot.Leg);
        HorseArmor = CreateSlot(CrpgItemSlot.MountHarness);

        Weapon0 = CreateSlot(CrpgItemSlot.Weapon0);
        Weapon1 = CreateSlot(CrpgItemSlot.Weapon1);
        Weapon2 = CreateSlot(CrpgItemSlot.Weapon2);
        Weapon3 = CreateSlot(CrpgItemSlot.Weapon3);
        ExtraWeaponSlot = CreateSlot(CrpgItemSlot.WeaponExtra);
        Horse = CreateSlot(CrpgItemSlot.Mount);

        CharacterPreview = new CharacterViewModel();

        AmountHeadArmor = new ArmorAmountVM();
        AmountBodyArmor = new ArmorAmountVM();
        AmountHandArmor = new ArmorAmountVM();
        AmountLegArmor = new ArmorAmountVM();
        AmountHorseArmor = new ArmorAmountVM();
    }

    // ===== Events to bubble slot actions up to parent =====
    public event Action<EquipmentSlotVM, ViewModel>? OnItemDropped;
    public event Action<EquipmentSlotVM>? OnSlotClicked;
    public event Action<EquipmentSlotVM>? OnSlotAlternateClicked;
    public event Action<EquipmentSlotVM>? OnItemDragBegin;
    public event Action<EquipmentSlotVM>? OnItemDragEnd;

    // ===== Exposed Slots =====
    [DataSourceProperty]
    public EquipmentSlotVM HeadArmor { get => _headArmor; set => SetField(ref _headArmor, value, nameof(HeadArmor)); }
    [DataSourceProperty]
    public EquipmentSlotVM CapeArmor { get => _capeArmor; set => SetField(ref _capeArmor, value, nameof(CapeArmor)); }
    [DataSourceProperty]
    public EquipmentSlotVM BodyArmor { get => _bodyArmor; set => SetField(ref _bodyArmor, value, nameof(BodyArmor)); }
    [DataSourceProperty]
    public EquipmentSlotVM HandArmor { get => _handArmor; set => SetField(ref _handArmor, value, nameof(HandArmor)); }
    [DataSourceProperty]
    public EquipmentSlotVM LegArmor { get => _legArmor; set => SetField(ref _legArmor, value, nameof(LegArmor)); }
    [DataSourceProperty]
    public EquipmentSlotVM HorseArmor { get => _horseArmor; set => SetField(ref _horseArmor, value, nameof(HorseArmor)); }

    [DataSourceProperty]
    public EquipmentSlotVM Weapon0 { get => _weapon0; set => SetField(ref _weapon0, value, nameof(Weapon0)); }
    [DataSourceProperty]
    public EquipmentSlotVM Weapon1 { get => _weapon1; set => SetField(ref _weapon1, value, nameof(Weapon1)); }
    [DataSourceProperty]
    public EquipmentSlotVM Weapon2 { get => _weapon2; set => SetField(ref _weapon2, value, nameof(Weapon2)); }
    [DataSourceProperty]
    public EquipmentSlotVM Weapon3 { get => _weapon3; set => SetField(ref _weapon3, value, nameof(Weapon3)); }
    [DataSourceProperty]
    public EquipmentSlotVM ExtraWeaponSlot { get => _extraWeaponSlot; set => SetField(ref _extraWeaponSlot, value, nameof(ExtraWeaponSlot)); }
    [DataSourceProperty]
    public EquipmentSlotVM Horse { get => _horse; set => SetField(ref _horse, value, nameof(Horse)); }

    [DataSourceProperty]
    public MBBindingList<EquipmentSlotVM> EquipmentSlots { get => _equipmentSlots; set => SetField(ref _equipmentSlots, value, nameof(EquipmentSlots)); }
    [DataSourceProperty]
    public CharacterViewModel CharacterPreview { get => _characterPreview; set => SetField(ref _characterPreview, value, nameof(CharacterPreview)); }

    // ===== Exposed Armor Values =====
    [DataSourceProperty]
    public ArmorAmountVM AmountHeadArmor { get => _amountHeadArmor; set => SetField(ref _amountHeadArmor, value, nameof(AmountHeadArmor)); }
    [DataSourceProperty]
    public ArmorAmountVM AmountBodyArmor { get => _amountBodyArmor; set => SetField(ref _amountBodyArmor, value, nameof(AmountBodyArmor)); }
    [DataSourceProperty]
    public ArmorAmountVM AmountHandArmor { get => _amountHandArmor; set => SetField(ref _amountHandArmor, value, nameof(AmountHandArmor)); }
    [DataSourceProperty]
    public ArmorAmountVM AmountLegArmor { get => _amountLegArmor; set => SetField(ref _amountLegArmor, value, nameof(AmountLegArmor)); }
    [DataSourceProperty]
    public ArmorAmountVM AmountHorseArmor { get => _amountHorseArmor; set => SetField(ref _amountHorseArmor, value, nameof(AmountHorseArmor)); }

    // ===== Public API =====
    public void RefreshFromEquipment(Equipment equipment)
    {
        foreach (var slot in _equipmentSlots)
        {
            slot.RefreshFromEquipment(equipment);
        }

        RefreshArmorAmounts(equipment);
    }

    public void RefreshArmorAmounts(Equipment equipment)
    {
        AmountHeadArmor.ArmorAmount = (int)equipment.GetHeadArmorSum();
        AmountBodyArmor.ArmorAmount = (int)equipment.GetHumanBodyArmorSum();
        AmountHandArmor.ArmorAmount = (int)equipment.GetArmArmorSum();
        AmountLegArmor.ArmorAmount = (int)equipment.GetLegArmorSum();
        AmountHorseArmor.ArmorAmount = (int)equipment.GetHorseArmorSum();
    }

    public void InitializeCharacterPreview(BasicCharacterObject? character = null)
    {
        character ??= MBObjectManager.Instance.GetObject<BasicCharacterObject>("mp_character");

        if (character != null)
        {
            CharacterPreview.FillFrom(character);
        }
        else
        {
            InformationManager.DisplayMessage(new InformationMessage("[EquipmentPanelVM] Failed to find BasicCharacterObject for preview", Colors.Red));
        }
    }

    internal void RefreshCharacterPreview(Equipment equipment)
    {
        if (equipment != null)
        {
            CharacterPreview.EquipmentCode = equipment.CalculateEquipmentCode();
            RefreshArmorAmounts(equipment);
        }
        else
        {
            InformationManager.DisplayMessage(new InformationMessage("[EquipmentPanelVM] Failed to update CharacterPreview equipment code.", Colors.Red));
        }
    }

    // ===== Internal helpers =====
    private EquipmentSlotVM CreateSlot(CrpgItemSlot slot)
    {
        var vm = new EquipmentSlotVM(slot);
        _equipmentSlots.Add(vm);

        // OnItemDropped already has correct signature
        vm.OnItemDropped += (sender, draggedItem) => OnItemDropped?.Invoke(sender, draggedItem);

        // Click events are fine
        vm.OnSlotClicked += s => OnSlotClicked?.Invoke(s);
        vm.OnSlotAlternateClicked += s => OnSlotAlternateClicked?.Invoke(s);

        // Drag events need to match ItemObject
        vm.OnItemDragBegin += item => OnItemDragBegin?.Invoke(vm);
        vm.OnItemDragEnd += item => OnItemDragEnd?.Invoke(vm);

        return vm;
    }
}
