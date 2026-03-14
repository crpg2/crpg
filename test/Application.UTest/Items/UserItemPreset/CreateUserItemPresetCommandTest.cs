using Crpg.Application.Common.Results;
using Crpg.Application.Items.Commands;
using Crpg.Application.Items.Models;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Items;

public class CreateUserItemPresetCommandTest : TestBase
{
    [Test]
    public async Task Create()
    {
        var user = ArrangeDb.Users.Add(new User()).Entity;
        ArrangeDb.Items.AddRange(
            new Item { Id = "item_a", Enabled = true },
            new Item { Id = "item_b", Enabled = true });
        await ArrangeDb.SaveChangesAsync();

        var cmd = new CreateUserItemPresetCommand
        {
            UserId = user.Id,
            Name = "My preset",
            Slots = FullSlots(new Dictionary<ItemSlot, string?>
            {
                [ItemSlot.Head] = "item_a",
                [ItemSlot.Weapon0] = "item_b",
            }),
        };

        var result = await new CreateUserItemPresetCommand.Handler(ActDb, Mapper).Handle(cmd, CancellationToken.None);

        Assert.That(result.Errors, Is.Null);
        Assert.That(result.Data!.Name, Is.EqualTo("My preset"));
        Assert.That(result.Data.Slots.Count, Is.EqualTo(Enum.GetValues<ItemSlot>().Length));

        var saved = AssertDb.UserItemPresets.First(p => p.Id == result.Data.Id);
        Assert.That(saved.UserId, Is.EqualTo(user.Id));
    }

    [Test]
    public async Task CreateFailsWithInvalidSlots()
    {
        var user = ArrangeDb.Users.Add(new User()).Entity;
        await ArrangeDb.SaveChangesAsync();

        var cmd = new CreateUserItemPresetCommand
        {
            UserId = user.Id,
            Name = "Broken",
            Slots = new List<UserItemPresetSlotInputModel>
            {
                new() { Slot = ItemSlot.Head, ItemId = null },
            },
        };

        var result = await new CreateUserItemPresetCommand.Handler(ActDb, Mapper).Handle(cmd, CancellationToken.None);

        Assert.That(result.Errors, Is.Not.Null);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.UserItemPresetBadSlots));
    }

    [Test]
    public async Task CreateFailsWithUnknownItemId()
    {
        var user = ArrangeDb.Users.Add(new User()).Entity;
        ArrangeDb.Items.Add(new Item { Id = "item_a", Enabled = true });
        await ArrangeDb.SaveChangesAsync();

        var cmd = new CreateUserItemPresetCommand
        {
            UserId = user.Id,
            Name = "Invalid item",
            Slots = FullSlots(new Dictionary<ItemSlot, string?>
            {
                [ItemSlot.Head] = "item_a",
                [ItemSlot.Weapon0] = "missing_item",
            }),
        };

        var result = await new CreateUserItemPresetCommand.Handler(ActDb, Mapper).Handle(cmd, CancellationToken.None);

        Assert.That(result.Errors, Is.Not.Null);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.ItemNotFound));
    }

    private static IList<UserItemPresetSlotInputModel> FullSlots(IDictionary<ItemSlot, string?>? customValues = null)
    {
        var values = customValues ?? new Dictionary<ItemSlot, string?>();

        return Enum.GetValues<ItemSlot>()
            .Select(slot => new UserItemPresetSlotInputModel
            {
                Slot = slot,
                ItemId = values.TryGetValue(slot, out string? itemId) ? itemId : null,
            })
            .ToList();
    }
}
