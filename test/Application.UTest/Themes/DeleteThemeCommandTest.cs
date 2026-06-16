using Crpg.Application.Themes.Commands;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Themes;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Crpg.Application.UTest.Themes;

public class DeleteThemeCommandTest : TestBase
{
    [Test]
    public async Task ShouldDeleteTheme()
    {
        var theme = new Theme("To delete");
        await ArrangeDb.Themes.AddAsync(theme);
        await ArrangeDb.SaveChangesAsync();

        var handler = new DeleteThemeCommand.Handler(ActDb);
        var command = CreateDefaultDeleteThemeCommand(theme.Id);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Errors, Is.Null.Or.Empty);

        var deletedTheme = await AssertDb.Themes.FindAsync(theme.Id);
        Assert.That(deletedTheme, Is.Null);
    }

    [Test]
    public async Task ShouldReturnAsThoughAThemeWasRemovedWhenThemeWasNotFound()
    {
        var handler = new DeleteThemeCommand.Handler(ActDb);
        var command = CreateDefaultDeleteThemeCommand();

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Errors, Is.Null.Or.Empty);
    }

    [Test]
    public async Task ShouldCascadeDeleteThemeEventsAndItemThemeTags()
    {
        var theme = new Theme("Cascade");
        var themeEvent = new ThemeEvent(
                name: "Event",
                goldMultiplier: 1.0f,
                expMultiplier: 1.0f,
                activeFromUtc: DateTime.UtcNow,
                activeUntilUtc: null,
                requiredEquipmentSlotsMatchingTheme: new(),
                minumumRequiredEquipmentSlotsMatchingTheme: 0,
                theme: theme);
        var item = new Item
        {
            Id = "test_item_1",
            Name = "Test Item",
            Themes = new List<Theme> { theme },
        };

        await ArrangeDb.Themes.AddAsync(theme);
        await ArrangeDb.ThemeEvents.AddAsync(themeEvent);
        await ArrangeDb.Items.AddAsync(item);
        await ArrangeDb.SaveChangesAsync();

        var handler = new DeleteThemeCommand.Handler(ActDb);
        var command = CreateDefaultDeleteThemeCommand();

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Errors, Is.Null.Or.Empty);

        var deletedTheme = await AssertDb.Themes.FindAsync(theme.Id);
        Assert.That(deletedTheme, Is.Null);

        var deletedThemeEvent = await AssertDb.ThemeEvents.FindAsync(themeEvent.Id);
        Assert.That(deletedThemeEvent, Is.Null);

        var untaggedItem = await AssertDb.Items.Include(i => i.Themes).SingleAsync(i => i.Id == item.Id);
        Assert.That(untaggedItem, Is.Not.Null);
        Assert.That(untaggedItem.Themes, Is.Empty);
    }

    private static DeleteThemeCommand CreateDefaultDeleteThemeCommand(int id = 1) => new() { Id = id, };
}
