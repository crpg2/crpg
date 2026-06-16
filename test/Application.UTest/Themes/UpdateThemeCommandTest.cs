using System;
using System.Collections.Generic;
using System.Text;
using Crpg.Application.Themes.Commands;
using Crpg.Domain.Entities.Themes;
using NUnit.Framework;

namespace Crpg.Application.UTest.Themes;

public class UpdateThemeCommandTest : TestBase
{
    [Test]
    public async Task ShouldUpdateTheme()
    {
        const string ExpectedNewName = "New name";

        var theme = new Theme("Old name");
        await ArrangeDb.Themes.AddAsync(theme);
        await ArrangeDb.SaveChangesAsync();

        var handler = new UpdateThemeCommand.Handler(ActDb, Mapper);
        var command = CreateDefaultUpdateThemeCommand(theme.Id, ExpectedNewName);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Errors, Is.Null.Or.Empty);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.Name, Is.EqualTo(ExpectedNewName));

        var savedTheme = await AssertDb.Themes.FindAsync(theme.Id);
        Assert.That(savedTheme!.Name, Is.EqualTo(ExpectedNewName));
    }

    [Test]
    public async Task ShouldReturnErrorWhenThemeWasNotFound()
    {
        var handler = new UpdateThemeCommand.Handler(ActDb, Mapper);
        var command = CreateDefaultUpdateThemeCommand();

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
        var validator = new UpdateThemeCommand.Validator();
        var command = CreateDefaultUpdateThemeCommand(name: string.Empty);

        var result = validator.Validate(command);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.PropertyName == "Name"), Is.True);
    }

    [Test]
    public async Task ShouldFailValidationWhenNameExceeds100Characters()
    {
        var validator = new UpdateThemeCommand.Validator();
        var command = CreateDefaultUpdateThemeCommand(name: new string('a', 101));

        var result = validator.Validate(command);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.PropertyName == "Name"), Is.True);
    }

    private static UpdateThemeCommand CreateDefaultUpdateThemeCommand(int id = 1, string name = "My theme")
    {
        return new UpdateThemeCommand
        {
            Id = id,
            Name = name,
        };
    }
}
