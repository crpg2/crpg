using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using System;
using System.Collections.Generic;
using System.Linq;

public class ItemPanelVM : ViewModel
{
    public MBBindingList<ItemVM> Items { get; } = new();
    private readonly List<ItemVM> _allItems;

    public ItemPanelVM(List<ItemObject> itemObjects)
    {
        _allItems = itemObjects.Select(item => new ItemVM(item)).ToList();
        FilterAll(); // default to showing all items
    }

    private void RefreshItems(Func<ItemVM, bool> predicate)
    {
        Items.Clear();
        foreach (var item in _allItems.Where(predicate))
        {
            Items.Add(item);
        }
    }

    // === Flexible Filter Methods ===
    public void FilterByType(ItemObject.ItemTypeEnum type)
        => RefreshItems(i => i.Type == type);

    public void FilterByTypes(HashSet<ItemObject.ItemTypeEnum> types)
        => RefreshItems(i => types.Contains(i.Type));

    // === Convenience Filters ===
    public void FilterAll() => RefreshItems(_ => true);

    public void FilterOneHanded() => FilterByType(ItemObject.ItemTypeEnum.OneHandedWeapon);
    public void FilterTwoHanded() => FilterByType(ItemObject.ItemTypeEnum.TwoHandedWeapon);
    public void FilterPolearm() => FilterByType(ItemObject.ItemTypeEnum.Polearm);
    public void FilterShields() => FilterByType(ItemObject.ItemTypeEnum.Shield);
    public void FilterHorses() => FilterByTypes(HorseTypes);
    public void FilterWeapons() => FilterByTypes(WeaponTypes);
    public void FilterAllArmor() => FilterByTypes(ArmorTypes);

    // === Type Sets ===
    private static readonly HashSet<ItemObject.ItemTypeEnum> WeaponTypes = new()
    {
        ItemObject.ItemTypeEnum.OneHandedWeapon,
        ItemObject.ItemTypeEnum.TwoHandedWeapon,
        ItemObject.ItemTypeEnum.Polearm,
        ItemObject.ItemTypeEnum.Bow,
        ItemObject.ItemTypeEnum.Crossbow,
        ItemObject.ItemTypeEnum.Thrown,
    };

    private static readonly HashSet<ItemObject.ItemTypeEnum> ArmorTypes = new()
    {
        ItemObject.ItemTypeEnum.HeadArmor,
        ItemObject.ItemTypeEnum.Cape,
        ItemObject.ItemTypeEnum.BodyArmor,
        ItemObject.ItemTypeEnum.LegArmor,
        ItemObject.ItemTypeEnum.HandArmor,
    };

    private static readonly HashSet<ItemObject.ItemTypeEnum> HorseTypes = new()
    {
        ItemObject.ItemTypeEnum.Horse,
        ItemObject.ItemTypeEnum.HorseHarness,
    };
}
