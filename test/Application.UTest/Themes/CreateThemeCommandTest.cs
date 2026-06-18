using Crpg.Application.Themes.Commands;
using NUnit.Framework;

namespace Crpg.Application.UTest.Themes;

public class CreateThemeCommandTest : TestBase
{
    [Test]
    public async Task ShouldCreateTheme()
    {
        var handler = new CreateThemeCommand.Handler(ActDb, Mapper);
        var command = new CreateThemeCommand
        {
            Name = "Test Theme",
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.Name, Is.EqualTo(command.Name));
    }

    [Test]
    public void ShouldFailValidationWhenNameIsEmpty()
    {
        var validator = new CreateThemeCommand.Validator();
        var command = new CreateThemeCommand
        {
            Name = string.Empty,        };

        var result = validator.Validate(command);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Has.Count.GreaterThan(0));
        Assert.That(result.Errors[0].PropertyName, Is.EqualTo("Name"));
    }

    [Test]
    public void ShouldFailValidationWhenNameExceeds100Characters()
    {
        var validator = new CreateThemeCommand.Validator();
        var command = new CreateThemeCommand
        {
            Name = new string('a', 101),
        };

        var result = validator.Validate(command);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.PropertyName == "Name"), Is.True);
    }

    [Test]
    public async Task ShouldReturnThemeWithId()
    {
        var handler = new CreateThemeCommand.Handler(ActDb, Mapper);
        var command = new CreateThemeCommand
        {
            Name = "Theme With ID",
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.Id, Is.GreaterThan(0));
    }
}
