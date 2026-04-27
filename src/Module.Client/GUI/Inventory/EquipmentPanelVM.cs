using Crpg.Module.Api.Models.Items;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace Crpg.Module.GUI.Inventory;

public class EquipmentPanelVM : ViewModel
{
    private readonly Dictionary<CrpgItemSlot, EquipmentSlotVM> _slotLookup = new();

    public EquipmentPanelVM()
    {
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

        AmountHeadArmor = new ArmorAmountVM(tooltipText: new TextObject("{=KC9dx131}Head Armor").ToString());
        AmountBodyArmor = new ArmorAmountVM(tooltipText: new TextObject("{=KC9dx132}Body Armor").ToString());
        AmountHandArmor = new ArmorAmountVM(tooltipText: new TextObject("{=KC9dx133}Hand Armor").ToString());
        AmountLegArmor = new ArmorAmountVM(tooltipText: new TextObject("{=KC9dx134}Leg Armor").ToString());
        AmountHorseArmor = new ArmorAmountVM(tooltipText: new TextObject("{=KC9dx135}Mount Armor").ToString());
    }

    // ===== Events =====
    public event Action<EquipmentSlotVM, ViewModel>? OnItemDropped;
    public event Action<EquipmentSlotVM>? OnSlotClicked;
    public event Action<EquipmentSlotVM>? OnSlotAlternateClicked;
    public event Action<EquipmentSlotVM>? OnItemDragBegin;
    public event Action<EquipmentSlotVM>? OnItemDragEnd;

    internal void InitializeCharacterPreview(BasicCharacterObject? character = null)
    {
        character ??= MBObjectManager.Instance.GetObject<BasicCharacterObject>("mp_character");

        if (character != null)
        {
            CharacterPreview.FillFrom(character);
        }
        else
        {
            Debug.Print("[EquipmentPanelVM] Failed to find preview character");
        }
    }

    internal void SetState(Equipment equipment, IReadOnlyList<CrpgEquippedItemExtended> items)
    {
        ApplySlots(items);
        ApplyVisuals(equipment);
    }

    internal void UpdateSlot(CrpgItemSlot slot, CrpgEquippedItemExtended? item)
    {
        if (_slotLookup.TryGetValue(slot, out var vm))
        {
            vm.UserItemEx = item?.UserItem;
        }
    }

    internal void UpdateSlotAndVisuals(CrpgItemSlot slot, CrpgEquippedItemExtended? item, Equipment equipment)
    {
        UpdateSlot(slot, item);
        ApplyVisuals(equipment);
    }

    internal void RefreshVisuals(Equipment equipment)
    {
        ApplyVisuals(equipment);
    }

    private void ApplySlots(IEnumerable<CrpgEquippedItemExtended> items)
    {
        foreach (var slot in EquipmentSlots)
        {
            slot.UserItemEx = null;
        }

        foreach (var item in items)
        {
            if (_slotLookup.TryGetValue(item.Slot, out var vm))
            {
                vm.UserItemEx = item.UserItem;
            }
        }
    }

    private void ApplyVisuals(Equipment equipment)
    {
        CharacterPreview.EquipmentCode = equipment.CalculateEquipmentCode();

        AmountHeadArmor.ArmorAmount = (int)equipment.GetHeadArmorSum();
        AmountBodyArmor.ArmorAmount = (int)equipment.GetHumanBodyArmorSum();
        AmountHandArmor.ArmorAmount = (int)equipment.GetArmArmorSum();
        AmountLegArmor.ArmorAmount = (int)equipment.GetLegArmorSum();
        AmountHorseArmor.ArmorAmount = (int)equipment.GetHorseArmorSum();
    }

    private EquipmentSlotVM CreateSlot(CrpgItemSlot slot)
    {
        var vm = new EquipmentSlotVM(slot);

        EquipmentSlots.Add(vm);
        _slotLookup[slot] = vm;

        vm.OnItemDropped += (s, d) => OnItemDropped?.Invoke(s, d);
        vm.OnSlotClicked += s => OnSlotClicked?.Invoke(s);
        vm.OnSlotAlternateClicked += s => OnSlotAlternateClicked?.Invoke(s);

        vm.OnItemDragBegin += _ => OnItemDragBegin?.Invoke(vm);
        vm.OnItemDragEnd += _ => OnItemDragEnd?.Invoke(vm);

        return vm;
    }

    // ===== Bindings =====

    [DataSourceProperty]
    public bool IsVisible { get; set => SetField(ref field, value, nameof(IsVisible)); }

    [DataSourceProperty]
    public EquipmentSlotVM HeadArmor { get; set => SetField(ref field, value, nameof(HeadArmor)); } = default!;
    [DataSourceProperty]
    public EquipmentSlotVM CapeArmor { get; set => SetField(ref field, value, nameof(CapeArmor)); } = default!;
    [DataSourceProperty]
    public EquipmentSlotVM BodyArmor { get; set => SetField(ref field, value, nameof(BodyArmor)); } = default!;
    [DataSourceProperty]
    public EquipmentSlotVM HandArmor { get; set => SetField(ref field, value, nameof(HandArmor)); } = default!;
    [DataSourceProperty]
    public EquipmentSlotVM LegArmor { get; set => SetField(ref field, value, nameof(LegArmor)); } = default!;
    [DataSourceProperty]
    public EquipmentSlotVM HorseArmor { get; set => SetField(ref field, value, nameof(HorseArmor)); } = default!;

    [DataSourceProperty]
    public EquipmentSlotVM Weapon0 { get; set => SetField(ref field, value, nameof(Weapon0)); } = default!;
    [DataSourceProperty]
    public EquipmentSlotVM Weapon1 { get; set => SetField(ref field, value, nameof(Weapon1)); } = default!;
    [DataSourceProperty]
    public EquipmentSlotVM Weapon2 { get; set => SetField(ref field, value, nameof(Weapon2)); } = default!;
    [DataSourceProperty]
    public EquipmentSlotVM Weapon3 { get; set => SetField(ref field, value, nameof(Weapon3)); } = default!;
    [DataSourceProperty]
    public EquipmentSlotVM ExtraWeaponSlot { get; set => SetField(ref field, value, nameof(ExtraWeaponSlot)); } = default!;
    [DataSourceProperty]
    public EquipmentSlotVM Horse { get; set => SetField(ref field, value, nameof(Horse)); } = default!;

    [DataSourceProperty]
    public MBBindingList<EquipmentSlotVM> EquipmentSlots { get; set => SetField(ref field, value, nameof(EquipmentSlots)); } = new();
    [DataSourceProperty]
    public CharacterViewModel CharacterPreview { get; set => SetField(ref field, value, nameof(CharacterPreview)); } = default!;

    [DataSourceProperty]
    public ArmorAmountVM AmountHeadArmor { get; set => SetField(ref field, value, nameof(AmountHeadArmor)); } = default!;
    [DataSourceProperty]
    public ArmorAmountVM AmountBodyArmor { get; set => SetField(ref field, value, nameof(AmountBodyArmor)); } = default!;
    [DataSourceProperty]
    public ArmorAmountVM AmountHandArmor { get; set => SetField(ref field, value, nameof(AmountHandArmor)); } = default!;
    [DataSourceProperty]
    public ArmorAmountVM AmountLegArmor { get; set => SetField(ref field, value, nameof(AmountLegArmor)); } = default!;
    [DataSourceProperty]
    public ArmorAmountVM AmountHorseArmor { get; set => SetField(ref field, value, nameof(AmountHorseArmor)); } = default!;
}
