using Crpg.Application.Common;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.ActivityLogs;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Notifications;
using Crpg.Domain.Entities.Users;
using Crpg.Sdk.Abstractions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Common.Services;

public class ItemServiceTest : TestBase
{
    private static readonly Constants Constants = new()
    {
        ItemSellCostPenalty = 0.5f,
    };

    [Theory]
    public async Task SellItemUnequipped(bool recentlyBought)
    {
        User user = new()
        {
            Gold = 0,
            Items =
            [
                new()
                {
                    Item = new Item { Price = 100 },
                    CreatedAt = recentlyBought ? new DateTime(2000, 01, 02) : new DateTime(2000, 01, 01),
                },
            ],
        };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        Mock<IDateTime> dateTimeMock = new();
        dateTimeMock.Setup(dt => dt.UtcNow).Returns(new DateTime(2000, 01, 02));

        ItemService itemService = new(dateTimeMock.Object, Constants);
        var userItem = await ActDb.UserItems
            .Include(ui => ui.User)
            .Include(ui => ui.Item)
            .Include(ui => ui.EquippedItems)
            .FirstAsync();
        itemService.SellUserItem(ActDb, userItem);
        await ActDb.SaveChangesAsync();

        user = await AssertDb.Users
            .Include(u => u.Items)
            .FirstAsync(u => u.Id == user.Id);
        Assert.That(recentlyBought ? 100 : 50, Is.EqualTo(user.Gold));
        Assert.That(user.Items, Has.None.Matches<UserItem>(ui => ui.Id == user.Items[0].Id));
    }

    [Test]
    public async Task SellItemEquipped()
    {
        Item item = new() { Id = "0", Price = 100 };
        UserItem userItem = new() { Item = item };
        List<Character> characters =
        [
            new() { EquippedItems = { new EquippedItem { UserItem = userItem, Slot = ItemSlot.Head } } },
            new() { EquippedItems = { new EquippedItem { UserItem = userItem, Slot = ItemSlot.Shoulder } } },
            new() { EquippedItems = { new EquippedItem { UserItem = userItem, Slot = ItemSlot.Body } } },
            new() { EquippedItems = { new EquippedItem { UserItem = userItem, Slot = ItemSlot.Hand } } },
            new() { EquippedItems = { new EquippedItem { UserItem = userItem, Slot = ItemSlot.Leg } } },
            new() { EquippedItems = { new EquippedItem { UserItem = userItem, Slot = ItemSlot.MountHarness } } },
            new() { EquippedItems = { new EquippedItem { UserItem = userItem, Slot = ItemSlot.Mount } } },
            new() { EquippedItems = { new EquippedItem { UserItem = userItem, Slot = ItemSlot.Weapon0 } } },
            new() { EquippedItems = { new EquippedItem { UserItem = userItem, Slot = ItemSlot.Weapon1 } } },
            new() { EquippedItems = { new EquippedItem { UserItem = userItem, Slot = ItemSlot.Weapon2 } } },
            new() { EquippedItems = { new EquippedItem { UserItem = userItem, Slot = ItemSlot.Weapon3 } } },
            new() { EquippedItems = { new EquippedItem { UserItem = userItem, Slot = ItemSlot.WeaponExtra } } },
        ];
        User user = new()
        {
            Gold = 0,
            Items = { userItem },
            Characters = characters,
        };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        Mock<IDateTime> dateTimeMock = new();
        dateTimeMock.Setup(dt => dt.UtcNow).Returns(new DateTime(2000, 01, 01));

        ItemService itemService = new(dateTimeMock.Object, Constants);
        userItem = await ActDb.UserItems
            .Include(ui => ui.User)
            .Include(ui => ui.Item)
            .Include(ui => ui.EquippedItems)
            .FirstAsync();
        itemService.SellUserItem(ActDb, userItem);
        await ActDb.SaveChangesAsync();

        user = await AssertDb.Users
            .Include(u => u.Characters)
            .Include(u => u.Items)
            .FirstAsync(u => u.Id == user.Id);
        Assert.That(user.Gold, Is.EqualTo(50));
        Assert.That(user.Items, Has.None.Matches<UserItem>(ui => ui.Id == userItem.Id));
        Assert.That(user.Characters[0].EquippedItems, Is.Empty);
        Assert.That(user.Characters[1].EquippedItems, Is.Empty);
        Assert.That(user.Characters[2].EquippedItems, Is.Empty);
        Assert.That(user.Characters[3].EquippedItems, Is.Empty);
        Assert.That(user.Characters[4].EquippedItems, Is.Empty);
        Assert.That(user.Characters[5].EquippedItems, Is.Empty);
        Assert.That(user.Characters[6].EquippedItems, Is.Empty);
        Assert.That(user.Characters[7].EquippedItems, Is.Empty);
        Assert.That(user.Characters[8].EquippedItems, Is.Empty);
        Assert.That(user.Characters[9].EquippedItems, Is.Empty);
        Assert.That(user.Characters[10].EquippedItems, Is.Empty);
        Assert.That(user.Characters[11].EquippedItems, Is.Empty);
    }

    [Test]
    public async Task RefundItemNoHeirloomPoints()
    {
        User user = new()
        {
            Gold = 0,
            HeirloomPoints = 0,
            Items =
            [
                new()
                {
                    Item = new Item { Id = "item_0", Price = 200, Rank = 0 },
                },
            ],
        };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        Mock<IDateTime> dateTimeMock = new();
        ItemService itemService = new(dateTimeMock.Object, Constants);
        var userItem = await ActDb.UserItems
            .Include(ui => ui.User)
            .Include(ui => ui.Item)
            .FirstAsync();
        itemService.RefundUserItem(ActDb, new ActivityLogService(new MetadataService()), new UserNotificationService(new MetadataService()), userItem);
        await ActDb.SaveChangesAsync();

        user = await AssertDb.Users
            .Include(u => u.Items)
            .FirstAsync(u => u.Id == user.Id);
        Assert.That(user.Gold, Is.EqualTo(200));
        Assert.That(user.HeirloomPoints, Is.EqualTo(0));
        Assert.That(user.Items, Is.Empty);
        var activityLog = await AssertDb.ActivityLogs.FirstAsync();
        Assert.That(activityLog.Type, Is.EqualTo(ActivityLogType.ItemReturned));
        var notification = await AssertDb.UserNotifications.FirstAsync();
        Assert.That(notification.Type, Is.EqualTo(NotificationType.ItemReturned));
    }

    [Test]
    public async Task RefundItemWithHeirloomPoints()
    {
        User user = new()
        {
            Gold = 0,
            HeirloomPoints = 0,
            Items =
            [
                new()
                {
                    Item = new Item { Id = "item_0", Price = 300, Rank = 3 },
                },
            ],
        };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        Mock<IDateTime> dateTimeMock = new();
        ItemService itemService = new(dateTimeMock.Object, Constants);
        var userItem = await ActDb.UserItems
            .Include(ui => ui.User)
            .Include(ui => ui.Item)
            .FirstAsync();
        itemService.RefundUserItem(ActDb, new ActivityLogService(new MetadataService()), new UserNotificationService(new MetadataService()), userItem);
        await ActDb.SaveChangesAsync();

        user = await AssertDb.Users
            .Include(u => u.Items)
            .FirstAsync(u => u.Id == user.Id);
        Assert.That(user.Gold, Is.EqualTo(300));
        Assert.That(user.HeirloomPoints, Is.EqualTo(3));
        Assert.That(user.Items, Is.Empty);
        var activityLog = await AssertDb.ActivityLogs.FirstAsync();
        Assert.That(activityLog.Type, Is.EqualTo(ActivityLogType.ItemReturned));
        var notification = await AssertDb.UserNotifications.FirstAsync();
        Assert.That(notification.Type, Is.EqualTo(NotificationType.ItemReturned));
    }

    [Test]
    public async Task RefundUserItemsByItemAsync_RefundsAllOwners()
    {
        Item item = new() { Id = "crpg_sword_1", Price = 100, Rank = 1 };
        ArrangeDb.Items.Add(item);
        User user1 = new() { Gold = 0, HeirloomPoints = 0, Items = { new UserItem { Item = item } } };
        User user2 = new() { Gold = 50, HeirloomPoints = 2, Items = { new UserItem { Item = item } } };
        ArrangeDb.Users.AddRange(user1, user2);
        await ArrangeDb.SaveChangesAsync();

        Mock<IDateTime> dateTimeMock = new();
        ItemService itemService = new(dateTimeMock.Object, Constants);
        await itemService.RefundUserItemsByItemAsync(ActDb, new ActivityLogService(new MetadataService()), new UserNotificationService(new MetadataService()), item.Id, CancellationToken.None);
        await ActDb.SaveChangesAsync();

        user1 = await AssertDb.Users.Include(u => u.Items).FirstAsync(u => u.Id == user1.Id);
        user2 = await AssertDb.Users.Include(u => u.Items).FirstAsync(u => u.Id == user2.Id);
        Assert.That(user1.Gold, Is.EqualTo(100));
        Assert.That(user1.HeirloomPoints, Is.EqualTo(1));
        Assert.That(user1.Items, Is.Empty);
        Assert.That(user2.Gold, Is.EqualTo(150));
        Assert.That(user2.HeirloomPoints, Is.EqualTo(3));
        Assert.That(user2.Items, Is.Empty);
        Assert.That(await AssertDb.ActivityLogs.CountAsync(), Is.EqualTo(2));
        Assert.That(await AssertDb.UserNotifications.CountAsync(), Is.EqualTo(2));
    }

    [Test]
    public async Task RefundUserItemsByItemAsync_NoOwners_DoesNothing()
    {
        Mock<IDateTime> dateTimeMock = new();
        ItemService itemService = new(dateTimeMock.Object, Constants);
        await itemService.RefundUserItemsByItemAsync(ActDb, new ActivityLogService(new MetadataService()), new UserNotificationService(new MetadataService()), "crpg_nonexistent", CancellationToken.None);
        await ActDb.SaveChangesAsync();

        Assert.That(await AssertDb.UserItems.CountAsync(), Is.EqualTo(0));
        Assert.That(await AssertDb.ActivityLogs.CountAsync(), Is.EqualTo(0));
    }
}
