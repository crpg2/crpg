using Crpg.Application.Common.Results;
using Crpg.Application.Items.Commands;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Themes;
using NUnit.Framework;

namespace Crpg.Application.UTest.Items;

public class RemoveThemesFromItemsCommandTest : TestBase
{
    [Test]
    public void ShouldFailValidationWhenItemIdsIsEmpty()
    {
        var result = new RemoveThemesFromItemsCommand.Validator().Validate(
            new RemoveThemesFromItemsCommand { ItemIds = Array.Empty<string>(), ThemeIds = new[] { 1 } });

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.PropertyName == nameof(RemoveThemesFromItemsCommand.ItemIds)), Is.True);
    }

    [Test]
    public void ShouldFailValidationWhenThemeIdsIsEmpty()
    {
        var result = new RemoveThemesFromItemsCommand.Validator().Validate(
            new RemoveThemesFromItemsCommand { ItemIds = new[] { "1" }, ThemeIds = Array.Empty<int>() });

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.PropertyName == nameof(RemoveThemesFromItemsCommand.ThemeIds)), Is.True);
    }

    [Test]
    public async Task ShouldRemoveGivenThemesPreservingOthers()
    {
        Theme winter = new("winter");
        Theme summer = new("summer");
        Theme spring = new("spring");
        ArrangeDb.Themes.AddRange(winter, summer, spring);
        Item item1 = new()
        {
            Id = "1",
            Name = "a_h0",
            BaseId = "a",
            Rank = 0,
            Price = 100,
            Type = ItemType.BodyArmor,
            Enabled = true,
            Themes = new() { winter, summer, spring },
        };
        Item item2 = new()
        {
            Id = "2",
            Name = "b_h0",
            BaseId = "b",
            Rank = 0,
            Price = 100,
            Type = ItemType.HandArmor,
            Enabled = true,
            Themes = new() { summer },
        };
        ArrangeDb.Items.AddRange(item1, item2);
        await ArrangeDb.SaveChangesAsync();

        var result = await new RemoveThemesFromItemsCommand.Handler(ActDb).Handle(
            new RemoveThemesFromItemsCommand
            {
                ItemIds = new[] { "1", "2" },
                ThemeIds = new[] { winter.Id, summer.Id },
            },
            CancellationToken.None);

        Assert.That(result.Errors, Is.Null);

        var dbItem1 = await AssertDb.Items.FindAsync("1");
        await AssertDb.Entry(dbItem1!).Collection(i => i.Themes).LoadAsync();
        Assert.That(dbItem1!.Themes.Select(t => t.Id), Is.EquivalentTo(new[] { spring.Id }));

        var dbItem2 = await AssertDb.Items.FindAsync("2");
        await AssertDb.Entry(dbItem2!).Collection(i => i.Themes).LoadAsync();
        Assert.That(dbItem2!.Themes, Is.Empty);
    }

    [Test]
    public async Task ShouldIgnoreThemesNotPresentOnItem()
    {
        Theme winter = new("winter");
        Theme summer = new("summer");
        ArrangeDb.Themes.AddRange(winter, summer);
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

        var result = await new RemoveThemesFromItemsCommand.Handler(ActDb).Handle(
            new RemoveThemesFromItemsCommand
            {
                ItemIds = new[] { "1" },
                ThemeIds = new[] { summer.Id },
            },
            CancellationToken.None);

        Assert.That(result.Errors, Is.Null);
        var dbItem = await AssertDb.Items.FindAsync("1");
        await AssertDb.Entry(dbItem!).Collection(i => i.Themes).LoadAsync();
        Assert.That(dbItem!.Themes.Select(t => t.Id), Is.EquivalentTo(new[] { winter.Id }));
    }

    [Test]
    public async Task ShouldReturnErrorIfAnItemNotFound()
    {
        Theme winter = new("winter");
        ArrangeDb.Themes.Add(winter);
        ArrangeDb.Items.Add(new()
        {
            Id = "1",
            Name = "a_h0",
            BaseId = "a",
            Rank = 0,
            Price = 100,
            Type = ItemType.BodyArmor,
            Enabled = true,
        });
        await ArrangeDb.SaveChangesAsync();

        var result = await new RemoveThemesFromItemsCommand.Handler(ActDb).Handle(
            new RemoveThemesFromItemsCommand
            {
                ItemIds = new[] { "1", "unknown" },
                ThemeIds = new[] { winter.Id },
            },
            CancellationToken.None);

        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.ItemNotFound));
    }

    [Test]
    public async Task ShouldReturnErrorIfAThemeNotFound()
    {
        ArrangeDb.Items.Add(new()
        {
            Id = "1",
            Name = "a_h0",
            BaseId = "a",
            Rank = 0,
            Price = 100,
            Type = ItemType.BodyArmor,
            Enabled = true,
        });
        await ArrangeDb.SaveChangesAsync();

        var result = await new RemoveThemesFromItemsCommand.Handler(ActDb).Handle(
            new RemoveThemesFromItemsCommand
            {
                ItemIds = new[] { "1" },
                ThemeIds = new[] { 999 },
            },
            CancellationToken.None);

        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.ThemeNotFound));
    }
}
