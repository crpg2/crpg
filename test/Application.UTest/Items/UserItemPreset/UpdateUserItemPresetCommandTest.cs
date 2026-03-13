using Crpg.Application.Items.Commands;
using Crpg.Application.Items.Models;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Items;

public class UpdateUserItemPresetCommandTest : TestBase
{
    [Test]
    public async Task Update()
    {
        var user = ArrangeDb.Users.Add(new User()).Entity;
        ArrangeDb.Items.AddRange(
            new Item { Id = "item_a", Enabled = true },
            new Item { Id = "item_b", Enabled = true });

        var preset = ArrangeDb.UserItemPresets.Add(new UserItemPreset
        {
            UserId = user.Id,
            Name = "old",
            Slots = FullSlots(new Dictionary<ItemSlot, string?>
            {
                [ItemSlot.Head] = "item_a",
            }).Select(s => new UserItemPresetSlot { Slot = s.Slot, ItemId = s.ItemId }).ToList(),
        }).Entity;
        await ArrangeDb.SaveChangesAsync();

        var cmd = new UpdateUserItemPresetCommand
        {
            UserId = user.Id,
            UserItemPresetId = preset.Id,
            Name = "new",
            Slots = FullSlots(new Dictionary<ItemSlot, string?>
            {
                [ItemSlot.Weapon0] = "item_b",
            }),
        };

        var result = await new UpdateUserItemPresetCommand.Handler(ActDb, Mapper).Handle(cmd, CancellationToken.None);

        Assert.That(result.Errors, Is.Null);
        Assert.That(result.Data!.Name, Is.EqualTo("new"));
        Assert.That(result.Data.Slots.First(s => s.Slot == ItemSlot.Weapon0).ItemId, Is.EqualTo("item_b"));

        var saved = AssertDb.UserItemPresets.First(p => p.Id == preset.Id);
        Assert.That(saved.Name, Is.EqualTo("new"));
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
