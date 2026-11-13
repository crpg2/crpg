using Crpg.Application.Themes.Commands;
using Crpg.Domain.Entities.Themes;
using NUnit.Framework;

namespace Crpg.Application.UTest.Themes;

public class DeleteThemeEventCommandTest : TestBase
{
    [Test]
    public async Task ShouldDeleteThemeEvent()
    {
        var themeEventToDelete = new ThemeEvent(
            name: "Theme event",
            goldMultiplier: 1.0f,
            expMultiplier: 1.0f,
            activeFromUtc: DateTimeOffset.UtcNow,
            activeUntilUtc: null,
            requiredEquipmentSlotsMatchingTheme: new(),
            minimumThemedItemsEquipped: 0,
            theme: new Theme(name: "default theme") { Id = 1 });

        await ArrangeDb.ThemeEvents.AddAsync(themeEventToDelete);
        await ArrangeDb.SaveChangesAsync();

        var handler = new DeleteThemeEventCommand.Handler(ActDb);
        var command = CreateDefaultDeleteThemeEventCommand(themeEventToDelete.Id);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Errors, Is.Null.Or.Empty);

        var deletedThemeEvent = await AssertDb.ThemeEvents.FindAsync(themeEventToDelete.Id);
        Assert.That(deletedThemeEvent, Is.Null);
    }

    [Test]
    public async Task ShouldReturnAsThoughAThemeEventWasRemovedWhenThemeEventWasNotFound()
    {
        var handler = new DeleteThemeEventCommand.Handler(ActDb);
        var command = CreateDefaultDeleteThemeEventCommand();

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Errors, Is.Null.Or.Empty);
    }

    private static DeleteThemeEventCommand CreateDefaultDeleteThemeEventCommand(int themeEventId = 1) => new() { Id = themeEventId };
}
