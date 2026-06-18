using Crpg.Application.Common.Results;
using Crpg.Application.Items.Commands;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Themes;
using NUnit.Framework;

namespace Crpg.Application.UTest.Items;

public class SetItemThemesCommandTest : TestBase
{
    [Test]
    public async Task ShouldReplaceItemThemes()
    {
        Theme winter = new("winter");
        Theme summer = new("summer");
        Theme spring = new("spring");
        ArrangeDb.Themes.AddRange(winter, summer, spring);
        Item item = new()
        {
            Id = "1",
            Name = "a_h0",
            BaseId = "a",
            Rank = 0,
            Price = 100,
            Type = ItemType.BodyArmor,
            Enabled = true,
            Themes = new() { winter },
        };
        ArrangeDb.Items.Add(item);
        await ArrangeDb.SaveChangesAsync();

        var result = await new SetItemThemesCommand.Handler(ActDb, Mapper).Handle(
            new SetItemThemesCommand { ItemId = "1", ThemeIds = new[] { summer.Id, spring.Id } },
            CancellationToken.None);

        Assert.That(result.Errors, Is.Null);
        var dbItem = await AssertDb.Items.FindAsync("1");
        await AssertDb.Entry(dbItem!).Collection(i => i.Themes).LoadAsync();
        Assert.That(dbItem!.Themes.Select(t => t.Id), Is.EquivalentTo(new[] { summer.Id, spring.Id }));
    }

    [Test]
    public async Task ShouldClearThemesWhenGivenEmptyList()
    {
        Theme winter = new("winter");
        ArrangeDb.Themes.Add(winter);
        Item item = new()
        {
            Id = "1",
            Name = "a_h0",
            BaseId = "a",
            Rank = 0,
            Price = 100,
            Type = ItemType.BodyArmor,
            Enabled = true,
            Themes = new() { winter },
        };
        ArrangeDb.Items.Add(item);
        await ArrangeDb.SaveChangesAsync();

        var result = await new SetItemThemesCommand.Handler(ActDb, Mapper).Handle(
            new SetItemThemesCommand { ItemId = "1", ThemeIds = Array.Empty<int>() },
            CancellationToken.None);

        Assert.That(result.Errors, Is.Null);
        var dbItem = await AssertDb.Items.FindAsync("1");
        await AssertDb.Entry(dbItem!).Collection(i => i.Themes).LoadAsync();
        Assert.That(dbItem!.Themes, Is.Empty);
    }

    [Test]
    public async Task ShouldReturnErrorIfItemNotFound()
    {
        var result = await new SetItemThemesCommand.Handler(ActDb, Mapper).Handle(
            new SetItemThemesCommand { ItemId = "unknown", ThemeIds = Array.Empty<int>() },
            CancellationToken.None);

        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.ItemNotFound));
    }

    [Test]
    public async Task ShouldReturnErrorIfThemeNotFound()
    {
        Item item = new()
        {
            Id = "1",
            Name = "a_h0",
            BaseId = "a",
            Rank = 0,
            Price = 100,
            Type = ItemType.BodyArmor,
            Enabled = true,
        };
        ArrangeDb.Items.Add(item);
        await ArrangeDb.SaveChangesAsync();

        var result = await new SetItemThemesCommand.Handler(ActDb, Mapper).Handle(
            new SetItemThemesCommand { ItemId = "1", ThemeIds = new[] { 999 } },
            CancellationToken.None);

        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.ThemeNotFound));
    }
}
