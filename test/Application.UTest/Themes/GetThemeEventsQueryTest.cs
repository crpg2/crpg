using Crpg.Application.Themes.Queries;
using Crpg.Domain.Entities.Themes;
using NUnit.Framework;

namespace Crpg.Application.UTest.Themes;

public class GetThemeEventsQueryTest : TestBase
{
    [Test]
    public async Task ShouldReturnEmptyWhenNoThemeEvents()
    {
        var handler = new GetThemeEventsQuery.Handler(ActDb);
        var result = await handler.Handle(new GetThemeEventsQuery(), CancellationToken.None);

        Assert.That(result?.Data, Is.Empty);
    }

    [Test]
    public async Task ShouldReturnThemeEvents()
    {
        var themeEventOne = CreateDefaultTheme(name: "Theme 1");
        var themeEventTwo = CreateDefaultTheme(name: "Theme 2");
        ArrangeDb.ThemeEvents.AddRange(themeEventOne, themeEventTwo);
        await ArrangeDb.SaveChangesAsync(CancellationToken.None);

        var handler = new GetThemeEventsQuery.Handler(ActDb);

        var result = await handler.Handle(new GetThemeEventsQuery(), CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Errors, Is.Null);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data, Has.Count.EqualTo(2));

        var resultOne = result.Data!.Single(x => x.Name == themeEventOne.Name);
        Assert.That(resultOne.Id, Is.GreaterThan(0));
        Assert.That(resultOne.Name, Is.EqualTo(themeEventOne.Name));
        Assert.That(resultOne.GoldMultiplier, Is.EqualTo(themeEventOne.GoldMultiplier));
        Assert.That(resultOne.ExpMultiplier, Is.EqualTo(themeEventOne.ExpMultiplier));
        Assert.That(resultOne.ActiveFromUtc, Is.EqualTo(themeEventOne.ActiveFromUtc));
        Assert.That(resultOne.ActiveUntilUtc, Is.EqualTo(themeEventOne.ActiveUntilUtc));
        Assert.That(resultOne.RequiredEquipmentSlotsMatchingTheme, Is.EquivalentTo(themeEventOne.RequiredEquipmentSlotsMatchingTheme));
        Assert.That(resultOne.MinimumThemedItemsEquipped, Is.EqualTo(themeEventOne.MinimumThemedItemsEquipped));
        Assert.That(resultOne.EventTheme.Id, Is.EqualTo(themeEventOne.EventTheme.Id));
        Assert.That(resultOne.EventTheme.Name, Is.EqualTo(themeEventOne.EventTheme.Name));

        var resultTwo = result.Data!.Single(x => x.Name == themeEventTwo.Name);
        Assert.That(resultTwo.Id, Is.GreaterThan(0));
        Assert.That(resultTwo.Name, Is.EqualTo(themeEventTwo.Name));
        Assert.That(resultTwo.GoldMultiplier, Is.EqualTo(themeEventTwo.GoldMultiplier));
        Assert.That(resultTwo.ExpMultiplier, Is.EqualTo(themeEventTwo.ExpMultiplier));
        Assert.That(resultTwo.ActiveFromUtc, Is.EqualTo(themeEventTwo.ActiveFromUtc));
        Assert.That(resultTwo.ActiveUntilUtc, Is.EqualTo(themeEventTwo.ActiveUntilUtc));
        Assert.That(resultTwo.RequiredEquipmentSlotsMatchingTheme, Is.EquivalentTo(themeEventTwo.RequiredEquipmentSlotsMatchingTheme));
        Assert.That(resultTwo.MinimumThemedItemsEquipped, Is.EqualTo(themeEventTwo.MinimumThemedItemsEquipped));
        Assert.That(resultTwo.EventTheme.Id, Is.EqualTo(themeEventTwo.EventTheme.Id));
        Assert.That(resultTwo.EventTheme.Name, Is.EqualTo(themeEventTwo.EventTheme.Name));
    }

    private static ThemeEvent CreateDefaultTheme(
        string name = "My theme event",
        float goldMultiplier = 1.5f,
        float expMultiplier = 2.0f,
        DateTimeOffset? activeFromUtc = null,
        DateTimeOffset? activeUntilUtc = null,
        List<ThemeEquipmentSlot> requiredEquipmentSlotsMatchingTheme = null!,
        int minimumThemedItemsEquipped = 1)
    {
        return new ThemeEvent(
            name: name,
            goldMultiplier: goldMultiplier,
            expMultiplier: expMultiplier,
            activeFromUtc: activeFromUtc ?? DateTimeOffset.UtcNow.AddDays(-1),
            activeUntilUtc: activeUntilUtc ?? DateTimeOffset.UtcNow.AddDays(1),
            requiredEquipmentSlotsMatchingTheme: requiredEquipmentSlotsMatchingTheme ?? [],
            minimumThemedItemsEquipped,
            theme: new Theme("theme"));
    }
}
