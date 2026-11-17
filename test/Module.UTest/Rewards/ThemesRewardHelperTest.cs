using Crpg.Module.Api.Models.Items;
using Crpg.Module.Api.Models.Themes;
using Crpg.Module.Rewards.Themes;
using NUnit.Framework;

namespace Crpg.Module.UTest.Rewards;

public class ThemesRewardHelperTest
{
    private static CrpgEquippedItem Equipped(CrpgItemSlot slot, string itemId) =>
        new() { Slot = slot, UserItem = new CrpgUserItem { ItemId = itemId } };

    private static ThemeEvent ThemeEvent(
        float expMultiplier = 2f,
        float goldMultiplier = 2f,
        int minimumThemedItemsEquipped = 0,
        List<ThemeEquipmentSlot>? requiredSlots = null,
        List<string>? eligibleItemIds = null) =>
        new()
        {
            ExpMultiplier = expMultiplier,
            GoldMultiplier = goldMultiplier,
            MinimumThemedItemsEquipped = minimumThemedItemsEquipped,
            RequiredEquipmentSlotsMatchingTheme = requiredSlots ?? new(),
            EligibleItemIds = eligibleItemIds ?? new(),
        };

    [Test]
    public void ShouldReturnNeutralMultipliersWhenNoActiveEvents()
    {
        var (exp, gold) = ThemesRewardHelper.GetActiveEventMultipliers(
            new List<CrpgEquippedItem>(), new List<ThemeEvent>());

        Assert.That(exp, Is.EqualTo(1.0f));
        Assert.That(gold, Is.EqualTo(1.0f));
    }

    [Test]
    public void ShouldApplyMultipliersWhenPlayerMeetsRequirements()
    {
        var equipped = new List<CrpgEquippedItem> { Equipped(CrpgItemSlot.Head, "themed_helmet") };
        var themeEvent = ThemeEvent(
            requiredSlots: new() { ThemeEquipmentSlot.Head },
            minimumThemedItemsEquipped: 1,
            eligibleItemIds: new() { "themed_helmet" });

        var (exp, gold) = ThemesRewardHelper.GetActiveEventMultipliers(equipped, new List<ThemeEvent> { themeEvent });

        Assert.That(exp, Is.EqualTo(2f));
        Assert.That(gold, Is.EqualTo(2f));
    }

    [Test]
    public void ShouldNotApplyMultipliersWhenBelowMinimumThemedItems()
    {
        var equipped = new List<CrpgEquippedItem> { Equipped(CrpgItemSlot.Head, "themed_helmet") };
        var themeEvent = ThemeEvent(
            minimumThemedItemsEquipped: 2,
            eligibleItemIds: new() { "themed_helmet" });

        var (exp, gold) = ThemesRewardHelper.GetActiveEventMultipliers(equipped, new List<ThemeEvent> { themeEvent });

        Assert.That(exp, Is.EqualTo(1f));
        Assert.That(gold, Is.EqualTo(1f));
    }

    [Test]
    public void ShouldNotApplyMultipliersWhenRequiredNonWeaponSlotIsMissing()
    {
        var equipped = new List<CrpgEquippedItem> { Equipped(CrpgItemSlot.Head, "themed_helmet") };
        var themeEvent = ThemeEvent(
            requiredSlots: new() { ThemeEquipmentSlot.Body },
            eligibleItemIds: new() { "themed_helmet" });

        var (exp, gold) = ThemesRewardHelper.GetActiveEventMultipliers(equipped, new List<ThemeEvent> { themeEvent });

        Assert.That(exp, Is.EqualTo(1f));
        Assert.That(gold, Is.EqualTo(1f));
    }

    [Test]
    public void ShouldNotApplyMultipliersWhenRequiredWeaponCountNotMet()
    {
        var equipped = new List<CrpgEquippedItem> { Equipped(CrpgItemSlot.Weapon0, "themed_sword") };
        var themeEvent = ThemeEvent(
            requiredSlots: new() { ThemeEquipmentSlot.Weapon, ThemeEquipmentSlot.Weapon },
            eligibleItemIds: new() { "themed_sword" });

        var (exp, gold) = ThemesRewardHelper.GetActiveEventMultipliers(equipped, new List<ThemeEvent> { themeEvent });

        Assert.That(exp, Is.EqualTo(1f));
        Assert.That(gold, Is.EqualTo(1f));
    }

    [Test]
    public void ShouldCountAnyWeaponSlotTowardsRequiredWeapons()
    {
        var equipped = new List<CrpgEquippedItem>
        {
            Equipped(CrpgItemSlot.Weapon0, "themed_sword"),
            Equipped(CrpgItemSlot.Weapon3, "themed_bow"),
        };
        var themeEvent = ThemeEvent(
            requiredSlots: new() { ThemeEquipmentSlot.Weapon, ThemeEquipmentSlot.Weapon },
            eligibleItemIds: new() { "themed_sword", "themed_bow" });

        var (exp, gold) = ThemesRewardHelper.GetActiveEventMultipliers(equipped, new List<ThemeEvent> { themeEvent });

        Assert.That(exp, Is.EqualTo(2f));
        Assert.That(gold, Is.EqualTo(2f));
    }

    [Test]
    public void ShouldCountBannerSlotTowardsRequiredWeapons()
    {
        var equipped = new List<CrpgEquippedItem> { Equipped(CrpgItemSlot.WeaponExtra, "themed_banner") };
        var themeEvent = ThemeEvent(
            requiredSlots: new() { ThemeEquipmentSlot.Weapon },
            eligibleItemIds: new() { "themed_banner" });

        var (exp, gold) = ThemesRewardHelper.GetActiveEventMultipliers(equipped, new List<ThemeEvent> { themeEvent });

        Assert.That(exp, Is.EqualTo(2f));
        Assert.That(gold, Is.EqualTo(2f));
    }

    [Test]
    public void ShouldTakeHighestExpAndGoldMultipliersIndependentlyAcrossEligibleEvents()
    {
        var equipped = new List<CrpgEquippedItem> { Equipped(CrpgItemSlot.Head, "themed_helmet") };
        var events = new List<ThemeEvent>
        {
            ThemeEvent(expMultiplier: 3f, goldMultiplier: 1.5f, eligibleItemIds: new() { "themed_helmet" }),
            ThemeEvent(expMultiplier: 2f, goldMultiplier: 4f, eligibleItemIds: new() { "themed_helmet" }),
        };

        var (exp, gold) = ThemesRewardHelper.GetActiveEventMultipliers(equipped, events);

        Assert.That(exp, Is.EqualTo(3f));
        Assert.That(gold, Is.EqualTo(4f));
    }

    [Test]
    public void ShouldIgnoreMultipliersFromEventsThePlayerIsNotEligibleFor()
    {
        var equipped = new List<CrpgEquippedItem> { Equipped(CrpgItemSlot.Head, "themed_helmet") };
        var events = new List<ThemeEvent>
        {
            // Eligible.
            ThemeEvent(expMultiplier: 2f, goldMultiplier: 2f, eligibleItemIds: new() { "themed_helmet" }),
            // Not eligible: requires a body item the player doesn't have.
            ThemeEvent(expMultiplier: 5f, goldMultiplier: 5f,
                requiredSlots: new() { ThemeEquipmentSlot.Body },
                eligibleItemIds: new() { "themed_cuirass" }),
        };

        var (exp, gold) = ThemesRewardHelper.GetActiveEventMultipliers(equipped, events);

        Assert.That(exp, Is.EqualTo(2f));
        Assert.That(gold, Is.EqualTo(2f));
    }
}
