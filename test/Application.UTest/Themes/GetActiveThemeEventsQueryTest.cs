using Crpg.Application.Themes.Queries;
using Crpg.Domain.Entities.Themes;
using NUnit.Framework;

namespace Crpg.Application.UTest.Themes;

public class GetActiveThemeEventsQueryTest : TestBase
{
    [Test]
    public async Task ShouldReturnEmptyWhenNoActiveThemeEvents()
    {
        var handler = new GetActiveThemeEventsQuery.Handler(ActDb);
        var result = await handler.Handle(new GetActiveThemeEventsQuery(), CancellationToken.None);

        Assert.That(result?.Data, Is.Empty);
    }

    [Test]
    public async Task ShouldReturnActiveThemeEvents()
    {
        var now = DateTimeOffset.UtcNow;
        var eventActiveForALimitedTime = CreateDefaultTheme(name: "Theme active for a limited time", activeFromUtc: now.AddDays(-1), activeUntilUtc: now.AddDays(1));
        var eventActiveIndefinitely = CreateDefaultTheme(name: "theme active indefinitely", activeFromUtc: now.AddDays(-1));
        var eventThatHasntStartedYet = CreateDefaultTheme("Theme that hasn't started yet.", activeFromUtc: now.AddDays(10));
        var eventThatHasAlreadyEnded = CreateDefaultTheme("Theme that has already passed.", activeFromUtc: now.AddDays(-10), activeUntilUtc: now.AddDays(-5));

        ArrangeDb.ThemeEvents.AddRange(eventActiveForALimitedTime, eventActiveIndefinitely, eventThatHasntStartedYet, eventThatHasAlreadyEnded);
        await ArrangeDb.SaveChangesAsync(CancellationToken.None);

        var handler = new GetActiveThemeEventsQuery.Handler(ActDb);

        var result = await handler.Handle(new GetActiveThemeEventsQuery(), CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Errors, Is.Null);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data, Has.Count.EqualTo(2));

        var resultOne = result.Data!.Single(x => x.Name == eventActiveForALimitedTime.Name);
        Assert.That(resultOne.Id, Is.GreaterThan(0));
        Assert.That(resultOne.Name, Is.EqualTo(eventActiveForALimitedTime.Name));
        Assert.That(resultOne.GoldMultiplier, Is.EqualTo(eventActiveForALimitedTime.GoldMultiplier));
        Assert.That(resultOne.ExpMultiplier, Is.EqualTo(eventActiveForALimitedTime.ExpMultiplier));
        Assert.That(resultOne.ActiveFromUtc, Is.EqualTo(eventActiveForALimitedTime.ActiveFromUtc));
        Assert.That(resultOne.ActiveUntilUtc, Is.EqualTo(eventActiveForALimitedTime.ActiveUntilUtc));
        Assert.That(resultOne.RequiredEquipmentSlotsMatchingTheme, Is.EquivalentTo(eventActiveForALimitedTime.RequiredEquipmentSlotsMatchingTheme));
        Assert.That(resultOne.MinumumRequiredEquipmentSlotsMatchingTheme, Is.EqualTo(eventActiveForALimitedTime.MinumumRequiredEquipmentSlotsMatchingTheme));
        Assert.That(resultOne.EventTheme.Id, Is.EqualTo(eventActiveForALimitedTime.EventTheme.Id));
        Assert.That(resultOne.EventTheme.Name, Is.EqualTo(eventActiveForALimitedTime.EventTheme.Name));

        var resultTwo = result.Data!.Single(x => x.Name == eventActiveIndefinitely.Name);
        Assert.That(resultTwo.Id, Is.GreaterThan(0));
        Assert.That(resultTwo.Name, Is.EqualTo(eventActiveIndefinitely.Name));
        Assert.That(resultTwo.GoldMultiplier, Is.EqualTo(eventActiveIndefinitely.GoldMultiplier));
        Assert.That(resultTwo.ExpMultiplier, Is.EqualTo(eventActiveIndefinitely.ExpMultiplier));
        Assert.That(resultTwo.ActiveFromUtc, Is.EqualTo(eventActiveIndefinitely.ActiveFromUtc));
        Assert.That(resultTwo.ActiveUntilUtc, Is.EqualTo(eventActiveIndefinitely.ActiveUntilUtc));
        Assert.That(resultTwo.RequiredEquipmentSlotsMatchingTheme, Is.EquivalentTo(eventActiveIndefinitely.RequiredEquipmentSlotsMatchingTheme));
        Assert.That(resultTwo.MinumumRequiredEquipmentSlotsMatchingTheme, Is.EqualTo(eventActiveIndefinitely.MinumumRequiredEquipmentSlotsMatchingTheme));
        Assert.That(resultTwo.EventTheme.Id, Is.EqualTo(eventActiveIndefinitely.EventTheme.Id));
        Assert.That(resultTwo.EventTheme.Name, Is.EqualTo(eventActiveIndefinitely.EventTheme.Name));
    }

    private static ThemeEvent CreateDefaultTheme(
        string name = "My theme event",
        float goldMultiplier = 1.5f,
        float expMultiplier = 2.0f,
        DateTimeOffset? activeFromUtc = null,
        DateTimeOffset? activeUntilUtc = null,
        List<ThemeEquipmentSlot> requiredEquipmentSlotsMatchingTheme = null!,
        int minumumRequiredEquipmentSlotsMatchingTheme = 1)
    {
        return new ThemeEvent(
            name: name,
            goldMultiplier: goldMultiplier,
            expMultiplier: expMultiplier,
            activeFromUtc: activeFromUtc ?? DateTimeOffset.UtcNow.AddDays(-1),
            activeUntilUtc: activeUntilUtc ?? DateTimeOffset.UtcNow.AddDays(1),
            requiredEquipmentSlotsMatchingTheme: requiredEquipmentSlotsMatchingTheme ?? [],
            minumumRequiredEquipmentSlotsMatchingTheme,
            theme: new Theme("theme"));
    }
}
