using Crpg.Application.Themes.Commands;
using Crpg.Domain.Entities.Themes;
using NUnit.Framework;

namespace Crpg.Application.UTest.Themes;
public class UpdateThemeEventCommandTest : TestBase
{
    [Test]
    public async Task ShouldUpdateThemeEvent()
    {
        const string ExpectedNewName = "New name";

        var theme = new Theme("Old name");
        var themeEvent = CreateDefaultThemeEvent(name: "old", theme: theme);
        await ArrangeDb.Themes.AddAsync(theme);
        await ArrangeDb.ThemeEvents.AddAsync(themeEvent);
        await ArrangeDb.SaveChangesAsync();

        var handler = new UpdateThemeEventCommand.Handler(ActDb, Mapper);

        var from = DateTimeOffset.UtcNow;
        var until = from.AddDays(3);

        var command = CreateDefaultUpdateThemeEventCommand(id: themeEvent.Id,
            name: ExpectedNewName,
            goldMultiplier: 2.0f,
            expMultiplier: 3.0f,
            activeFromUtc: from,
            activeUntilUtc: until,
            requiredEquipmentSlotsMatchingTheme: new List<ThemeEquipmentSlot> { ThemeEquipmentSlot.Head, ThemeEquipmentSlot.Shoulder },
            minimumThemedItemsEquipped: 2,
            themeId: theme.Id);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Errors, Is.Null.Or.Empty);
        Assert.That(result.Data, Is.Not.Null);

        Assert.That(result.Data!.Name, Is.EqualTo(ExpectedNewName));
        Assert.That(result.Data!.GoldMultiplier, Is.EqualTo(2.0f));
        Assert.That(result.Data!.ExpMultiplier, Is.EqualTo(3.0f));
        Assert.That(result.Data!.ActiveFromUtc, Is.EqualTo(from));
        Assert.That(result.Data!.ActiveUntilUtc, Is.EqualTo(until));
        Assert.That(result.Data!.RequiredEquipmentSlotsMatchingTheme, Is.EquivalentTo(new List<ThemeEquipmentSlot> { ThemeEquipmentSlot.Head, ThemeEquipmentSlot.Shoulder }));
        Assert.That(result.Data!.MinimumThemedItemsEquipped, Is.EqualTo(2));
        Assert.That(result.Data!.EventTheme.Id, Is.EqualTo(theme.Id));
    }

    [Test]
    public async Task ShouldChangeThemeWhenThemeIdChanges()
    {
        var themeOne = new Theme("Theme One");
        var themeTwo = new Theme("Theme Two");
        var themeEvent = CreateDefaultThemeEvent(theme: themeOne);
        await ArrangeDb.Themes.AddRangeAsync(themeOne, themeTwo);
        await ArrangeDb.ThemeEvents.AddAsync(themeEvent);
        await ArrangeDb.SaveChangesAsync();

        var handler = new UpdateThemeEventCommand.Handler(ActDb, Mapper);

        var command = CreateDefaultUpdateThemeEventCommand(id: themeEvent.Id, themeId: themeTwo.Id);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Errors, Is.Null.Or.Empty);
        Assert.That(result.Data, Is.Not.Null);

        Assert.That(result.Data!.EventTheme.Id, Is.EqualTo(themeTwo.Id));
    }

    [Test]
    public async Task ShouldReturnErrorWhenThemeEventIsNotFound()
    {
        var handler = new UpdateThemeEventCommand.Handler(ActDb, Mapper);

        var command = CreateDefaultUpdateThemeEventCommand();

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Data, Is.Null);
        Assert.That(result.Errors, Is.Not.Null.Or.Empty);

        var error = result.Errors!.Single();
        Assert.That(error.Code.ToString(), Is.EqualTo("ThemeEventNotFound"));
    }

    [Test]
    public async Task ShouldReturnErrorWhenThemeIsNotFound()
    {
        var themeEvent = CreateDefaultThemeEvent();
        await ArrangeDb.Themes.AddAsync(themeEvent.EventTheme);
        await ArrangeDb.ThemeEvents.AddAsync(themeEvent);
        await ArrangeDb.SaveChangesAsync();

        var handler = new UpdateThemeEventCommand.Handler(ActDb, Mapper);

        var command = CreateDefaultUpdateThemeEventCommand(themeId: 999);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Data, Is.Null);
        Assert.That(result.Errors, Is.Not.Null.Or.Empty);

        var error = result.Errors!.Single();
        Assert.That(error.Code.ToString(), Is.EqualTo("ThemeNotFound"));
    }

    [Test]
    public async Task ShouldFailValidationWhenNameIsEmpty()
    {
        var validator = new UpdateThemeEventCommand.Validator();
        var command = CreateDefaultUpdateThemeEventCommand(name: string.Empty);

        var result = validator.Validate(command);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.PropertyName == "Name"), Is.True);
    }

    [Test]
    public async Task ShouldFailValidationWhenNameExceeds100Characters()
    {
        var validator = new UpdateThemeEventCommand.Validator();
        var command = CreateDefaultUpdateThemeEventCommand(name: new string('a', 101));

        var result = validator.Validate(command);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.PropertyName == "Name"), Is.True);
    }

    [Test]
    public async Task ShouldFailValidationWhenGoldMultiPlierIsZero()
    {
        var validator = new UpdateThemeEventCommand.Validator();
        var command = CreateDefaultUpdateThemeEventCommand(goldMultiplier: 0);

        var result = validator.Validate(command);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.PropertyName == "GoldMultiplier"), Is.True);
    }

    [Test]
    public async Task ShouldFailValidationWhenExpMultiPlierIsZero()
    {
        var validator = new UpdateThemeEventCommand.Validator();
        var command = CreateDefaultUpdateThemeEventCommand(expMultiplier: 0);

        var result = validator.Validate(command);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.PropertyName == "ExpMultiplier"), Is.True);
    }

    [Test]
    public async Task ShouldFailValidationWhenGoldMultiplierIsBelowOne()
    {
        var validator = new UpdateThemeEventCommand.Validator();
        var command = CreateDefaultUpdateThemeEventCommand(goldMultiplier: 0.5f);

        var result = validator.Validate(command);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.PropertyName == "GoldMultiplier"), Is.True);
    }

    [Test]
    public async Task ShouldFailValidationWhenExpMultiplierIsBelowOne()
    {
        var validator = new UpdateThemeEventCommand.Validator();
        var command = CreateDefaultUpdateThemeEventCommand(expMultiplier: 0.5f);

        var result = validator.Validate(command);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.PropertyName == "ExpMultiplier"), Is.True);
    }

    [Test]
    public async Task ShouldFailValidationWhenActiveUntilIsBeforeActiveFrom()
    {
        var validator = new UpdateThemeEventCommand.Validator();
        var now = DateTimeOffset.UtcNow;
        var command = CreateDefaultUpdateThemeEventCommand(activeFromUtc: now, activeUntilUtc: now.AddDays(-1));

        var result = validator.Validate(command);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.PropertyName == "ActiveUntilUtc"), Is.True);
    }

    [Test]
    public async Task ShouldFailValidationWhenActiveFromIsMissing()
    {
        var validator = new UpdateThemeEventCommand.Validator();
        var command = CreateDefaultUpdateThemeEventCommand();
        command.ActiveFromUtc = default;

        var result = validator.Validate(command);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.PropertyName == "ActiveFromUtc"), Is.True);
    }

    [Test]
    public async Task ShouldFailValidationWhenActiveFromIsNotUtc()
    {
        var validator = new UpdateThemeEventCommand.Validator();
        var command = CreateDefaultUpdateThemeEventCommand(activeFromUtc: new DateTimeOffset(DateTime.Now.Ticks, TimeSpan.FromHours(2)));

        var result = validator.Validate(command);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.PropertyName == "ActiveFromUtc"), Is.True);
    }

    [Test]
    public async Task ShouldFailValidationWhenActiveUntilExistsButIsNotUtc()
    {
        var validator = new UpdateThemeEventCommand.Validator();
        var command = CreateDefaultUpdateThemeEventCommand(activeUntilUtc: new DateTimeOffset(DateTime.Now.Ticks, TimeSpan.FromHours(2)));

        var result = validator.Validate(command);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.PropertyName == "ActiveUntilUtc"), Is.True);
    }

    private static UpdateThemeEventCommand CreateDefaultUpdateThemeEventCommand(
            int id = 1,
            string name = "Theme event",
            float goldMultiplier = 1.0f,
            float expMultiplier = 1.0f,
            DateTimeOffset activeFromUtc = default,
            DateTimeOffset? activeUntilUtc = null,
            List<ThemeEquipmentSlot> requiredEquipmentSlotsMatchingTheme = null!,
            int? minimumThemedItemsEquipped = null,
            int themeId = 1)
    {
        return new UpdateThemeEventCommand
        {
            Id = id,
            Name = name,
            GoldMultiplier = goldMultiplier,
            ExpMultiplier = expMultiplier,
            ActiveFromUtc = activeFromUtc == default ? DateTimeOffset.UtcNow : activeFromUtc,
            ActiveUntilUtc = activeUntilUtc,
            RequiredEquipmentSlotsMatchingTheme = requiredEquipmentSlotsMatchingTheme ?? new(),
            MinimumThemedItemsEquipped = minimumThemedItemsEquipped,
            ThemeId = themeId,
        };
    }

    private ThemeEvent CreateDefaultThemeEvent(
            string name = "Theme event",
            float goldMultiplier = 1.0f,
            float expMultiplier = 1.0f,
            DateTimeOffset activeFromUtc = default,
            DateTimeOffset? activeUntilUtc = null,
            List<ThemeEquipmentSlot> requiredEquipmentSlotsMatchingTheme = null!,
            int minimumThemedItemsEquipped = 0,
            Theme theme = null!)
    {
        return new ThemeEvent(
            name: name,
            goldMultiplier: goldMultiplier,
            expMultiplier: expMultiplier,
            activeFromUtc: activeFromUtc == default ? DateTimeOffset.UtcNow : activeFromUtc,
            activeUntilUtc: activeUntilUtc,
            requiredEquipmentSlotsMatchingTheme: requiredEquipmentSlotsMatchingTheme ?? new(),
            minimumThemedItemsEquipped: minimumThemedItemsEquipped,
            theme: theme ?? new Theme(name: "default theme") { Id = 1 });
    }
}
