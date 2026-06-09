using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Items.Commands;
using Crpg.Application.UTest.Marketplace;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Marketplace;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Items;

public class UpgradeItemCommandTest : TestBase
{
    [Test]
    public async Task UpgradeItemFromRank0ToRank1()
    {
        Item item00 = new() { Id = "a_h0", BaseId = "a", Price = 100, Enabled = true, Rank = 0 };
        Item item01 = new() { Id = "a_h1", BaseId = "a", Price = 100, Enabled = true, Rank = 1 };
        Item item10 = new() { Id = "b_h0", BaseId = "b", Price = 100, Enabled = true, Rank = 1 };

        UserItem userItem0 = new() { Item = item00 };
        UserItem userItem1 = new() { Item = item10 };

        User user = new()
        {
            Gold = 100,
            HeirloomPoints = 5,
            Items = { userItem0, userItem1 },
            Characters =
            {
                new Character
                {
                    EquippedItems =
                    {
                        new EquippedItem { Slot = ItemSlot.Head, UserItem = userItem0 },
                        new EquippedItem { Slot = ItemSlot.Shoulder, UserItem = userItem0 },
                        new EquippedItem { Slot = ItemSlot.Body, UserItem = userItem1 },
                    },
                },
                new Character
                {
                    EquippedItems =
                    {
                        new EquippedItem { Slot = ItemSlot.Head, UserItem = userItem0 },
                    },
                },
            },
        };
        ArrangeDb.Users.Add(user);
        ArrangeDb.Items.AddRange(item00, item01, item10);
        await ArrangeDb.SaveChangesAsync();

        Mock<IActivityLogService> activityLogServiceMock = new() { DefaultValue = DefaultValue.Mock };

        UpgradeUserItemCommand.Handler handler = new(ActDb, Mapper, activityLogServiceMock.Object);
        var result = await handler.Handle(new UpgradeUserItemCommand
        {
            UserItemId = userItem0.Id,
            UserId = user.Id,
            UpgradeRank = 1,
        }, CancellationToken.None);

        var userDb = await AssertDb.Users
            .Include(u => u.Items)
            .Include(u => u.Characters).ThenInclude(c => c.EquippedItems).ThenInclude(ei => ei.UserItem)
            .FirstAsync(u => u.Id == user.Id);

        var upgradedUserItem = result.Data!;
        Assert.That(upgradedUserItem.Item.Rank, Is.EqualTo(1));
        Assert.That(upgradedUserItem.Item.BaseId, Is.EqualTo(item00.BaseId));
        Assert.That(userDb.HeirloomPoints, Is.EqualTo(4));

        Assert.That(userDb.Items, Has.Some.Matches<UserItem>(ui => ui.Id == upgradedUserItem.Id));

        Assert.That(userDb.Characters[0].EquippedItems[0].UserItemId, Is.EqualTo(userItem0.Id));
        Assert.That(userDb.Characters[0].EquippedItems[0].UserItem!.ItemId, Is.EqualTo(item01.Id));
        Assert.That(userDb.Characters[0].EquippedItems[1].UserItemId, Is.EqualTo(userItem0.Id));
        Assert.That(userDb.Characters[0].EquippedItems[1].UserItem!.ItemId, Is.EqualTo(item01.Id));
        Assert.That(userDb.Characters[0].EquippedItems[2].UserItemId, Is.EqualTo(userItem1.Id));
        Assert.That(userDb.Characters[0].EquippedItems[2].UserItem!.ItemId, Is.EqualTo(item10.Id));
        Assert.That(userDb.Characters[1].EquippedItems[0].UserItemId, Is.EqualTo(userItem0.Id));
        Assert.That(userDb.Characters[1].EquippedItems[0].UserItem!.ItemId, Is.EqualTo(item01.Id));
    }

    [Test]
    public async Task UpgradeItemToMaxRank()
    {
        Item item00 = new() { Id = "a_h0", BaseId = "a", Price = 100, Enabled = true, Rank = 0 };
        Item item03 = new() { Id = "a_h3", BaseId = "a", Price = 100, Enabled = true, Rank = 3 };

        UserItem userItem0 = new() { Item = item00 };

        User user = new()
        {
            Gold = 100,
            HeirloomPoints = 10,
            Items = { userItem0 },
        };
        ArrangeDb.Users.Add(user);
        ArrangeDb.Items.AddRange(item00, item03);
        await ArrangeDb.SaveChangesAsync();

        Mock<IActivityLogService> activityLogServiceMock = new() { DefaultValue = DefaultValue.Mock };

        UpgradeUserItemCommand.Handler handler = new(ActDb, Mapper, activityLogServiceMock.Object);
        var result = await handler.Handle(new UpgradeUserItemCommand
        {
            UserItemId = userItem0.Id,
            UserId = user.Id,
            UpgradeRank = 3,
        }, CancellationToken.None);

        var upgradedUserItem = result.Data!;
        Assert.That(upgradedUserItem.Item.Rank, Is.EqualTo(3));

        var userDb = await AssertDb.Users.Include(u => u.Items).FirstAsync(u => u.Id == user.Id);
        Assert.That(userDb.HeirloomPoints, Is.EqualTo(7)); // 10 - 3
    }

    [Test]
    public async Task UpgradeItemFromRank1ToRank2()
    {
        Item item01 = new() { Id = "a_h1", BaseId = "a", Price = 100, Enabled = true, Rank = 1 };
        Item item02 = new() { Id = "a_h2", BaseId = "a", Price = 100, Enabled = true, Rank = 2 };

        UserItem userItem0 = new() { Item = item01 };

        User user = new()
        {
            Gold = 100,
            HeirloomPoints = 5,
            Items = { userItem0 },
        };
        ArrangeDb.Users.Add(user);
        ArrangeDb.Items.AddRange(item01, item02);
        await ArrangeDb.SaveChangesAsync();

        Mock<IActivityLogService> activityLogServiceMock = new() { DefaultValue = DefaultValue.Mock };

        UpgradeUserItemCommand.Handler handler = new(ActDb, Mapper, activityLogServiceMock.Object);
        var result = await handler.Handle(new UpgradeUserItemCommand
        {
            UserItemId = userItem0.Id,
            UserId = user.Id,
            UpgradeRank = 2,
        }, CancellationToken.None);

        var upgradedUserItem = result.Data!;
        Assert.That(upgradedUserItem.Item.Rank, Is.EqualTo(2));
        Assert.That(upgradedUserItem.Item.BaseId, Is.EqualTo(item01.BaseId));

        var userDb = await AssertDb.Users.Include(u => u.Items).FirstAsync(u => u.Id == user.Id);
        Assert.That(userDb.HeirloomPoints, Is.EqualTo(4)); // 5 - 1 = 4
    }

    [Test]
    public async Task BrokenItemCannotBeUpgraded()
    {
        Item item0 = new() { Id = "a_h0", BaseId = "a", Price = 100, Enabled = true, Rank = 0 };
        Item item1 = new() { Id = "a_h1", BaseId = "a", Price = 100, Enabled = true, Rank = 1 };

        UserItem userItem0 = new() { Item = item0, IsBroken = true };

        User user = new()
        {
            Gold = 100,
            HeirloomPoints = 5,
            Items = { userItem0 },
        };

        ArrangeDb.Users.Add(user);
        ArrangeDb.Items.AddRange(item0, item1);
        await ArrangeDb.SaveChangesAsync();

        Mock<IActivityLogService> activityLogServiceMock = new() { DefaultValue = DefaultValue.Mock };

        UpgradeUserItemCommand.Handler handler = new(ActDb, Mapper, activityLogServiceMock.Object);
        var result = await handler.Handle(new UpgradeUserItemCommand
        {
            UserItemId = userItem0.Id,
            UserId = user.Id,
            UpgradeRank = 1,
        }, CancellationToken.None);

        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.ItemBroken));
    }

    [Test]
    public async Task CannotUpgradeNonExistingItem()
    {
        User user = new()
        {
            Gold = 100,
            Items = { new() },
            HeirloomPoints = 10,
        };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        Mock<IActivityLogService> activityLogServiceMock = new() { DefaultValue = DefaultValue.Mock };

        UpgradeUserItemCommand.Handler handler = new(ActDb, Mapper, activityLogServiceMock.Object);
        var result = await handler.Handle(new UpgradeUserItemCommand
        {
            UserItemId = 15, // Non-existent UserItem ID
            UserId = user.Id,
            UpgradeRank = 1,
        }, CancellationToken.None);

        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.UserItemNotFound));
    }

    [Test]
    public async Task CannotUpgradeItemForNonExistentUser()
    {
        Mock<IActivityLogService> activityLogServiceMock = new() { DefaultValue = DefaultValue.Mock };
        UpgradeUserItemCommand.Handler handler = new(ActDb, Mapper, activityLogServiceMock.Object);
        var result = await handler.Handle(new UpgradeUserItemCommand
        {
            UserItemId = 50,
            UserId = 1, // Non-existent user ID
            UpgradeRank = 1,
        }, CancellationToken.None);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.UserNotFound));
    }

    [Test]
    public async Task BannerCannotBeUpgraded()
    {
        Item item0 = new() { Id = "a_h0", BaseId = "a", Price = 100, Enabled = true, Rank = 0, Type = ItemType.Banner };
        User user = new()
        {
            Gold = 100,
            Items = { new UserItem { Item = item0 } },
            HeirloomPoints = 5,
        };

        ArrangeDb.Users.Add(user);
        ArrangeDb.Items.Add(item0);
        await ArrangeDb.SaveChangesAsync();

        Mock<IActivityLogService> activityLogServiceMock = new() { DefaultValue = DefaultValue.Mock };

        UpgradeUserItemCommand.Handler handler = new(ActDb, Mapper, activityLogServiceMock.Object);
        var result = await handler.Handle(new UpgradeUserItemCommand
        {
            UserItemId = user.Items[0].Id,
            UserId = user.Id,
            UpgradeRank = 1,
        }, CancellationToken.None);

        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.ItemNotUpgradable));
    }

    [Test]
    public async Task InsufficientHeirloomPointsForUpgrade()
    {
        Item item0 = new() { Id = "a_h0", BaseId = "a", Price = 100, Enabled = true, Rank = 0 };
        Item item3 = new() { Id = "a_h3", BaseId = "a", Price = 100, Enabled = true, Rank = 3 };
        User user = new()
        {
            Gold = 100,
            Items = { new() { Item = item0 } },
            HeirloomPoints = 1,
        };
        ArrangeDb.Users.Add(user);
        ArrangeDb.Items.Add(item0);
        ArrangeDb.Items.Add(item3);
        await ArrangeDb.SaveChangesAsync();

        Mock<IActivityLogService> activityLogServiceMock = new() { DefaultValue = DefaultValue.Mock };

        UpgradeUserItemCommand.Handler handler = new(ActDb, Mapper, activityLogServiceMock.Object);
        var result = await handler.Handle(new UpgradeUserItemCommand
        {
            UserItemId = user.Items[0].Id,
            UserId = user.Id,
            UpgradeRank = 3,
        }, CancellationToken.None);

        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.NotEnoughHeirloomPoints));
    }

    [Test]
    public async Task CannotUpgradeWithInvalidRankZero()
    {
        Item item0 = new() { Id = "a_h0", BaseId = "a", Price = 100, Enabled = true, Rank = 0 };
        User user = new()
        {
            Gold = 100,
            Items = { new() { Item = item0 } },
            HeirloomPoints = 10,
        };
        ArrangeDb.Users.Add(user);
        ArrangeDb.Items.Add(item0);
        await ArrangeDb.SaveChangesAsync();

        Mock<IActivityLogService> activityLogServiceMock = new() { DefaultValue = DefaultValue.Mock };

        UpgradeUserItemCommand.Handler handler = new(ActDb, Mapper, activityLogServiceMock.Object);
        var result = await handler.Handle(new UpgradeUserItemCommand
        {
            UserItemId = user.Items[0].Id,
            UserId = user.Id,
            UpgradeRank = 0,
        }, CancellationToken.None);

        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.InvalidItemUpgradeRank));
    }

    [Test]
    public async Task CannotUpgradeWithInvalidRankAboveMax()
    {
        Item item0 = new() { Id = "a_h0", BaseId = "a", Price = 100, Enabled = true, Rank = 0 };
        User user = new()
        {
            Gold = 100,
            Items = { new() { Item = item0 } },
            HeirloomPoints = 10,
        };
        ArrangeDb.Users.Add(user);
        ArrangeDb.Items.Add(item0);
        await ArrangeDb.SaveChangesAsync();

        Mock<IActivityLogService> activityLogServiceMock = new() { DefaultValue = DefaultValue.Mock };

        UpgradeUserItemCommand.Handler handler = new(ActDb, Mapper, activityLogServiceMock.Object);
        var result = await handler.Handle(new UpgradeUserItemCommand
        {
            UserItemId = user.Items[0].Id,
            UserId = user.Id,
            UpgradeRank = 4,
        }, CancellationToken.None);

        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.InvalidItemUpgradeRank));
    }

    [Test]
    public async Task TargetItemDoesNotExist()
    {
        Item item0 = new() { Id = "a_h0", BaseId = "a", Price = 100, Enabled = true, Rank = 0 };
        User user = new()
        {
            Gold = 100,
            Items = { new() { Item = item0 } },
            HeirloomPoints = 10,
        };
        ArrangeDb.Users.Add(user);
        ArrangeDb.Items.Add(item0);

        await ArrangeDb.SaveChangesAsync();

        Mock<IActivityLogService> activityLogServiceMock = new() { DefaultValue = DefaultValue.Mock };

        UpgradeUserItemCommand.Handler handler = new(ActDb, Mapper, activityLogServiceMock.Object);
        var result = await handler.Handle(new UpgradeUserItemCommand
        {
            UserItemId = user.Items[0].Id,
            UserId = user.Id,
            UpgradeRank = 1,
        }, CancellationToken.None);

        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.UserItemMaxRankReached));
    }

    [Test]
    public async Task MarketplaceListedItemShouldNotBeUpgradeable()
    {
        Item item0 = new() { Id = "a_h0", BaseId = "a", Price = 100, Enabled = true, Rank = 0 };
        Item item1 = new() { Id = "a_h1", BaseId = "a", Price = 100, Enabled = true, Rank = 1 };
        User user = new()
        {
            Gold = 100,
            HeirloomPoints = 10,
            Items = { new() { Item = item0 } },
        };
        ArrangeDb.Users.Add(user);
        ArrangeDb.Items.AddRange(item0, item1);
        ArrangeDb.MarketplaceListings.Add(MarketplaceListingFactory.CreateListing(sellerId: user.Id, offeredUserItemId: user.Items[0].Id));
        await ArrangeDb.SaveChangesAsync();

        Mock<IActivityLogService> activityLogServiceMock = new() { DefaultValue = DefaultValue.Mock };

        UpgradeUserItemCommand.Handler handler = new(ActDb, Mapper, activityLogServiceMock.Object);
        var result = await handler.Handle(new UpgradeUserItemCommand
        {
            UserItemId = user.Items[0].Id,
            UserId = user.Id,
            UpgradeRank = 1,
        }, CancellationToken.None);

        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.UserItemInMarketplace));
    }
}
