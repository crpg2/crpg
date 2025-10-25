using Crpg.Application.UTest;
using FluentValidation.Results;
using NUnit.Framework;

namespace Crpg.Application.Settings.Commands;

public class EditSettingCommandTest : TestBase
{
    [Test]
    public async Task EditSettingsPartial()
    {
        ArrangeDb.Settings.Add(new()
        {
            Id = 1,
            Discord = "link",
            Steam = "link",
            Patreon = "link",
            Github = "link",
            Reddit = "link",
            ModDb = "link",
            HappyHours = string.Empty,
        });
        await ArrangeDb.SaveChangesAsync();

        var result = await new EditSettingsCommand.Handler(ActDb, Mapper).Handle(new EditSettingsCommand
        {
            Steam = "new_link",
        }, CancellationToken.None);

        var updatedSettings = result.Data!;
        Assert.That(updatedSettings.Steam, Is.EqualTo("new_link"));
        Assert.That(updatedSettings.Discord, Is.EqualTo("link"));
    }

    [Test]
    public void ShouldThrowIfHappyHoursInvalidFormat()
    {
        EditSettingsCommand.Validator validator = new();
        ValidationResult res = validator.Validate(new EditSettingsCommand
        {
            HappyHours = "...Invalid format...",
        });

        Assert.That(res.IsValid, Is.False);
    }
}
