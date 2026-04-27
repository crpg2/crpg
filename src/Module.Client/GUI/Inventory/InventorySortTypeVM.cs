using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Crpg.Module.GUI.Inventory;

public class InventorySortTypeVM : ViewModel
{
    private readonly Action<InventorySortTypeVM>? _onClick;
    public string Id { get; }
    internal Func<InventorySlotVM, bool> Predicate { get; }

    public InventorySortTypeVM(string id, string iconSprite, Func<InventorySlotVM, bool> predicate, bool isIconVisible = true, Action<InventorySortTypeVM>? handleClick = null)
    {
        Id = id;
        ShowInventoryTypeIcon = isIconVisible;
        IconSprite = iconSprite;
        _onClick = handleClick;
        Predicate = predicate;
    }

    private void ExecuteClick()
    {
        IsSelected = !IsSelected;
        _onClick?.Invoke(this);
    }

    private void ExecuteBeginHint()
    {
        MBInformationManager.ShowHint(GetToolTipTextFromId(Id));
    }

    private void ExecuteEndHint()
    {
        MBInformationManager.HideInformations();
    }

    private static string GetToolTipTextFromId(string id) => id switch
    {
        "onehanded" => new TextObject("{=KC9dx139}One Handed").ToString(),
        "twohanded" => new TextObject("{=KC9dx140}Two Handed").ToString(),
        "polearm" => new TextObject("{=KC9dx141}Polearms").ToString(),
        "shield" => new TextObject("{=KC9dx142}Shields").ToString(),
        "thrown" => new TextObject("{=KC9dx143}Throwing").ToString(),
        "bow" => new TextObject("{=KC9dx144}Bows").ToString(),
        "crossbow" => new TextObject("{=KC9dx145}Crossbows").ToString(),
        "gun" => new TextObject("{=KC9dx146}Guns").ToString(),
        "ammo" => new TextObject("{=KC9dx147}Ammunition").ToString(),
        "headarmor" => new TextObject("{=KC9dx148}Head Armor").ToString(),
        "cape" => new TextObject("{=KC9dx149}Cape").ToString(),
        "chestarmor" => new TextObject("{=KC9dx150}Chest Armor").ToString(),
        "handarmor" => new TextObject("{=KC9dx151}Hand Armor").ToString(),
        "legarmor" => new TextObject("{=KC9dx152}Leg Armor").ToString(),
        "mount" => new TextObject("{=KC9dx153}Mount").ToString(),
        "mountarmor" => new TextObject("{=KC9dx154}Mount Harness").ToString(),
        "banner" => new TextObject("{=KC9dx155}Banner").ToString(),
        _ => id,
    };

    [DataSourceProperty]
    public bool IsSelected { get; set => SetField(ref field, value, nameof(IsSelected)); }

    [DataSourceProperty]
    public string IconSprite { get; set => SetField(ref field, value, nameof(IconSprite)); } = string.Empty;

    [DataSourceProperty]
    public bool ShowInventoryTypeIcon { get; set => SetField(ref field, value, nameof(ShowInventoryTypeIcon)); }
}
