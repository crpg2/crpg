using Crpg.Application.Themes.Queries;
using Crpg.Domain.Entities.Themes;
using NUnit.Framework;

namespace Crpg.Application.UTest.Themes;

public class GetThemeEventsQueryTest : TestBase
{
    [Test]
    public async Task ShouldReturnEmptyWhenNoThemes()
    {
        var handler = new GetThemeEventsQuery.Handler(ActDb, Mapper);
        var result = await handler.Handle(new GetThemeEventsQuery(), CancellationToken.None);

        Assert.That(result?.Data, Is.Empty);
    }

    [Test]
    public async Task ShouldReturnThemes()
    {
        var themeOne = CreateDefaultTheme(name: "Theme 1");
        var themeTwo = CreateDefaultTheme(name: "Theme 2");
        ArrangeDb.ThemeEvents.AddRange(themeOne, themeTwo);
        await ArrangeDb.SaveChangesAsync(CancellationToken.None);

        var handler = new GetThemeEventsQuery.Handler(ActDb, Mapper);

        var result = await handler.Handle(new GetThemeEventsQuery(), CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Errors, Is.Null);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data, Has.Count.EqualTo(2));

        var resultOne = result.Data!.Single(x => x.Name == themeOne.Name);
        Assert.That(resultOne.Id, Is.GreaterThan(0));
        Assert.That(resultOne.Name, Is.EqualTo(themeOne.Name));
        Assert.That(resultOne.GoldMultiplier, Is.EqualTo(themeOne.GoldMultiplier));
        Assert.That(resultOne.ExpMultiplier, Is.EqualTo(themeOne.ExpMultiplier));
        Assert.That(resultOne.ActiveFromUtc, Is.EqualTo(themeOne.ActiveFromUtc));
        Assert.That(resultOne.ActiveUntilUtc, Is.EqualTo(themeOne.ActiveUntilUtc));

        var resultTwo = result.Data!.Single(x => x.Name == themeTwo.Name);
        Assert.That(resultTwo.Id, Is.GreaterThan(0));
        Assert.That(resultTwo.Name, Is.EqualTo(themeTwo.Name));
        Assert.That(resultTwo.GoldMultiplier, Is.EqualTo(themeTwo.GoldMultiplier));
        Assert.That(resultTwo.ExpMultiplier, Is.EqualTo(themeTwo.ExpMultiplier));
        Assert.That(resultTwo.ActiveFromUtc, Is.EqualTo(themeTwo.ActiveFromUtc));
        Assert.That(resultTwo.ActiveUntilUtc, Is.EqualTo(themeTwo.ActiveUntilUtc));
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
