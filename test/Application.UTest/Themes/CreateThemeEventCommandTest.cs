using Crpg.Application.Themes.Commands;
using Crpg.Domain.Entities.Themes;
using NUnit.Framework;

namespace Crpg.Application.UTest.Themes;

public class CreateThemeEventCommandTest : TestBase
{
    [Test]
    public async Task ShouldCreateThemeEvent()
    {
        var theme = new Theme(name: "my theme");
        await ArrangeDb.Themes.AddAsync(theme);
        await ArrangeDb.SaveChangesAsync();

        var handler = new CreateThemeEventCommand.Handler(ActDb, Mapper);
        var command = new CreateThemeEventCommand
        {
            Name = "Test Theme",
            GoldMultiplier = 1.5f,
            ExpMultiplier = 2.0f,
            ActiveFromUtc = DateTimeOffset.UtcNow,
            ActiveUntilUtc = DateTimeOffset.UtcNow.AddDays(7),
            RequiredEquipmentSlotsMatchingTheme = new List<ThemeEquipmentSlot> { ThemeEquipmentSlot.Head },
            MinimumThemedItemsEquipped = 3,
            ThemeId = theme.Id,
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.Name, Is.EqualTo(command.Name));
        Assert.That(result.Data.GoldMultiplier, Is.EqualTo(command.GoldMultiplier));
        Assert.That(result.Data.ExpMultiplier, Is.EqualTo(command.ExpMultiplier));
        Assert.That(result.Data.ActiveFromUtc, Is.EqualTo(command.ActiveFromUtc));
        Assert.That(result.Data.ActiveUntilUtc, Is.EqualTo(command.ActiveUntilUtc));
        Assert.That(result.Data.RequiredEquipmentSlotsMatchingTheme, Is.EquivalentTo(command.RequiredEquipmentSlotsMatchingTheme));
        Assert.That(result.Data.MinimumThemedItemsEquipped, Is.EqualTo(command.MinimumThemedItemsEquipped));
        Assert.That(result.Data.EventTheme.Id, Is.EqualTo(command.ThemeId));

    }

    [Test]
    public async Task ShouldDefaultMinimumThemedItemsEquippedToRequiredSlotCountWhenNotProvided()
    {
        var theme = new Theme(name: "my theme");
        await ArrangeDb.Themes.AddAsync(theme);
        await ArrangeDb.SaveChangesAsync();

        var handler = new CreateThemeEventCommand.Handler(ActDb, Mapper);
        var command = new CreateThemeEventCommand
        {
            Name = "Test Theme",
            ActiveFromUtc = DateTimeOffset.UtcNow,
            RequiredEquipmentSlotsMatchingTheme = new List<ThemeEquipmentSlot> { ThemeEquipmentSlot.Head, ThemeEquipmentSlot.Body },
            MinimumThemedItemsEquipped = null,
            ThemeId = theme.Id,
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.MinimumThemedItemsEquipped, Is.EqualTo(2));
    }

    [Test]
    public async Task ShouldCreateThemeEventWithDefaultMultipliers()
    {
        var theme = new Theme(name: "my theme");
        await ArrangeDb.Themes.AddAsync(theme);
        await ArrangeDb.SaveChangesAsync();

        var handler = new CreateThemeEventCommand.Handler(ActDb, Mapper);
        var command = new CreateThemeEventCommand
        {
            Name = "Default Theme",
            ActiveFromUtc = DateTimeOffset.UtcNow,
            ThemeId = theme.Id,
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.GoldMultiplier, Is.EqualTo(1.0f));
        Assert.That(result.Data.ExpMultiplier, Is.EqualTo(1.0f));
        Assert.That(result.Data.ActiveUntilUtc, Is.Null);
    }

    [Test]
    public async Task ShouldCreateThemeEventWithoutActiveUntil()
    {
        var theme = new Theme(name: "my theme");
        await ArrangeDb.Themes.AddAsync(theme);
        await ArrangeDb.SaveChangesAsync();

        var handler = new CreateThemeEventCommand.Handler(ActDb, Mapper);
        var now = DateTimeOffset.UtcNow;
        var command = new CreateThemeEventCommand
        {
            Name = "Indefinite Theme",
            GoldMultiplier = 2.0f,
            ExpMultiplier = 2.5f,
            ActiveFromUtc = now,
            ActiveUntilUtc = null,
            ThemeId = theme.Id,
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.ActiveUntilUtc, Is.Null);
    }

    [Test]
    public async Task ShouldReturnErrorWhenThemeWasNotFound()
    {
        var handler = new CreateThemeEventCommand.Handler(ActDb, Mapper);
        var now = DateTimeOffset.UtcNow;
        var command = new CreateThemeEventCommand
        {
            Name = "Indefinite Theme",
            GoldMultiplier = 2.0f,
            ExpMultiplier = 2.5f,
            ActiveFromUtc = now,
            ActiveUntilUtc = null,
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.That(result.Data, Is.Null);
        Assert.That(result.Errors, Is.Not.Null);

        var error = result.Errors!.Single();
        Assert.That(error.Code.ToString(), Is.EqualTo("ThemeNotFound"));
    }

    [Test]
    public void ShouldFailValidationWhenNameIsEmpty()
    {
        var validator = new CreateThemeEventCommand.Validator();
        var command = new CreateThemeEventCommand
        {
            Name = string.Empty,
            ActiveFromUtc = DateTimeOffset.UtcNow,
        };

        var result = validator.Validate(command);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Has.Count.GreaterThan(0));
        Assert.That(result.Errors[0].PropertyName, Is.EqualTo("Name"));
    }

    [Test]
    public void ShouldFailValidationWhenNameExceeds100Characters()
    {
        var validator = new CreateThemeEventCommand.Validator();
        var command = new CreateThemeEventCommand
        {
            Name = new string('a', 101),
            ActiveFromUtc = DateTimeOffset.UtcNow,
        };

        var result = validator.Validate(command);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.PropertyName == "Name"), Is.True);
    }

    [Test]
    public void ShouldPassValidationWhenNameIs100Characters()
    {
        var validator = new CreateThemeEventCommand.Validator();
        var command = new CreateThemeEventCommand
        {
            Name = new string('a', 100),
            GoldMultiplier = 1.0f,
            ExpMultiplier = 1.0f,
            ActiveFromUtc = DateTimeOffset.UtcNow,
        };

        var result = validator.Validate(command);

        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void ShouldFailValidationWhenGoldMultiplierIsZero()
    {
        var validator = new CreateThemeEventCommand.Validator();
        var command = new CreateThemeEventCommand
        {
            Name = "Test",
            GoldMultiplier = 0f,
            ExpMultiplier = 1.0f,
            ActiveFromUtc = DateTimeOffset.UtcNow,
        };

        var result = validator.Validate(command);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.PropertyName == "GoldMultiplier"), Is.True);
    }

    [Test]
    public void ShouldFailValidationWhenGoldMultiplierIsNegative()
    {
        var validator = new CreateThemeEventCommand.Validator();
        var command = new CreateThemeEventCommand
        {
            Name = "Test",
            GoldMultiplier = -1.5f,
            ExpMultiplier = 1.0f,
            ActiveFromUtc = DateTimeOffset.UtcNow,
        };

        var result = validator.Validate(command);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.PropertyName == "GoldMultiplier"), Is.True);
    }

    [Test]
    public void ShouldFailValidationWhenExpMultiplierIsZero()
    {
        var validator = new CreateThemeEventCommand.Validator();
        var command = new CreateThemeEventCommand
        {
            Name = "Test",
            GoldMultiplier = 1.0f,
            ExpMultiplier = 0f,
            ActiveFromUtc = DateTimeOffset.UtcNow,
        };

        var result = validator.Validate(command);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.PropertyName == "ExpMultiplier"), Is.True);
    }

    [Test]
    public void ShouldFailValidationWhenExpMultiplierIsNegative()
    {
        var validator = new CreateThemeEventCommand.Validator();
        var command = new CreateThemeEventCommand
        {
            Name = "Test",
            GoldMultiplier = 1.0f,
            ExpMultiplier = -2.0f,
            ActiveFromUtc = DateTimeOffset.UtcNow,
        };

        var result = validator.Validate(command);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.PropertyName == "ExpMultiplier"), Is.True);
    }

    [Test]
    public void ShouldPassValidationWhenMessageIs1000Characters()
    {
        var validator = new CreateThemeEventCommand.Validator();
        var command = new CreateThemeEventCommand
        {
            Name = "Test",
            GoldMultiplier = 1.0f,
            ExpMultiplier = 1.0f,
            ActiveFromUtc = DateTimeOffset.UtcNow,
        };

        var result = validator.Validate(command);

        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void ShouldFailValidationWhenActiveUntilUtcIsBeforeActiveFromUtc()
    {
        var validator = new CreateThemeEventCommand.Validator();
        var now = DateTimeOffset.UtcNow;
        var command = new CreateThemeEventCommand
        {
            Name = "Test",
            GoldMultiplier = 1.0f,
            ExpMultiplier = 1.0f,
            ActiveFromUtc = now,
            ActiveUntilUtc = now.AddDays(-1),
        };

        var result = validator.Validate(command);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.PropertyName == "ActiveUntilUtc"), Is.True);
    }

    [Test]
    public void ShouldPassValidationWhenActiveUntilUtcEqualsActiveFromUtc()
    {
        var validator = new CreateThemeEventCommand.Validator();
        var now = DateTimeOffset.UtcNow;
        var command = new CreateThemeEventCommand
        {
            Name = "Test",
            GoldMultiplier = 1.0f,
            ExpMultiplier = 1.0f,
            ActiveFromUtc = now,
            ActiveUntilUtc = now,
        };

        var result = validator.Validate(command);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.PropertyName == "ActiveUntilUtc"), Is.True);
    }

    [Test]
    public async Task ShouldReturnThemeEventWithId()
    {
        var theme = new Theme(name: "my theme");
        await ArrangeDb.Themes.AddAsync(theme);
        await ArrangeDb.SaveChangesAsync();

        var handler = new CreateThemeEventCommand.Handler(ActDb, Mapper);
        var command = new CreateThemeEventCommand
        {
            Name = "Theme With ID",
            ActiveFromUtc = DateTimeOffset.UtcNow,
            ThemeId = theme.Id,
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.Id, Is.GreaterThan(0));
    }
}
