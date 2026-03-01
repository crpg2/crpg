using TaleWorlds.Core;

namespace Crpg.Module.Api.Models.Items;

/// <summary>
/// Copy Crpg.Domain.Entities.Items.ItemSlot.
/// </summary>
public enum CrpgItemSlot
{
    Head,
    Shoulder,
    Body,
    Hand,
    Leg,
    MountHarness,
    Mount,
    Weapon0,
    Weapon1,
    Weapon2,
    Weapon3,
    WeaponExtra,
}

internal static class CrpgItemSlotExtensions
{
    private static readonly Dictionary<CrpgItemSlot, EquipmentIndex> SlotToIndex = new()
    {
        [CrpgItemSlot.Head] = EquipmentIndex.Head,
        [CrpgItemSlot.Shoulder] = EquipmentIndex.Cape,
        [CrpgItemSlot.Body] = EquipmentIndex.Body,
        [CrpgItemSlot.Hand] = EquipmentIndex.Gloves,
        [CrpgItemSlot.Leg] = EquipmentIndex.Leg,
        [CrpgItemSlot.MountHarness] = EquipmentIndex.HorseHarness,
        [CrpgItemSlot.Mount] = EquipmentIndex.Horse,
        [CrpgItemSlot.Weapon0] = EquipmentIndex.Weapon0,
        [CrpgItemSlot.Weapon1] = EquipmentIndex.Weapon1,
        [CrpgItemSlot.Weapon2] = EquipmentIndex.Weapon2,
        [CrpgItemSlot.Weapon3] = EquipmentIndex.Weapon3,
        [CrpgItemSlot.WeaponExtra] = EquipmentIndex.ExtraWeaponSlot,
    };

    private static readonly Dictionary<EquipmentIndex, CrpgItemSlot> IndexToSlot = new()
    {
        [EquipmentIndex.Head] = CrpgItemSlot.Head,
        [EquipmentIndex.Cape] = CrpgItemSlot.Shoulder,
        [EquipmentIndex.Body] = CrpgItemSlot.Body,
        [EquipmentIndex.Gloves] = CrpgItemSlot.Hand,
        [EquipmentIndex.Leg] = CrpgItemSlot.Leg,
        [EquipmentIndex.HorseHarness] = CrpgItemSlot.MountHarness,
        [EquipmentIndex.Horse] = CrpgItemSlot.Mount,
        [EquipmentIndex.Weapon0] = CrpgItemSlot.Weapon0,
        [EquipmentIndex.Weapon1] = CrpgItemSlot.Weapon1,
        [EquipmentIndex.Weapon2] = CrpgItemSlot.Weapon2,
        [EquipmentIndex.Weapon3] = CrpgItemSlot.Weapon3,
        [EquipmentIndex.ExtraWeaponSlot] = CrpgItemSlot.WeaponExtra,
    };

    public static EquipmentIndex ToIndex(this CrpgItemSlot slot) => SlotToIndex[slot];

    public static CrpgItemSlot ToSlot(this EquipmentIndex index) => IndexToSlot[index];
}
