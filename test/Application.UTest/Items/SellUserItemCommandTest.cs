using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Items.Commands;
using Crpg.Application.UTest.Marketplace;
using Crpg.Domain.Entities.Clans;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Marketplace;
using Crpg.Domain.Entities.Users;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Items;

public class SellUserItemCommandTest : TestBase
{
    [Test]
    public async Task ShouldCallItemService()
    {
        User user = new()
        {
            Gold = 0,
            Items =
            [
                new()
                {
                    Item = new Item { Price = 100, Enabled = true },
                },
            ],
        };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        Mock<IItemService> itemServiceMock = new();
        Mock<IActivityLogService> activityLogServiceMock = new() { DefaultValue = DefaultValue.Mock };

        SellUserItemCommand.Handler handler = new(ActDb, itemServiceMock.Object, activityLogServiceMock.Object);
        await handler.Handle(new SellUserItemCommand
        {
            UserItemId = user.Items[0].Id,
            UserId = user.Id,
        }, CancellationToken.None);

        itemServiceMock.Verify(s => s.SellUserItem(ActDb, It.IsAny<UserItem>()));
    }

    [Test]
    public async Task NotFoundItem()
    {
        var user = ArrangeDb.Users.Add(new User());
        await ArrangeDb.SaveChangesAsync();

        var itemService = Mock.Of<IItemService>();
        SellUserItemCommand.Handler handler = new(ActDb, itemService, Mock.Of<IActivityLogService>());
        var result = await handler.Handle(new SellUserItemCommand
        {
            UserItemId = 1,
            UserId = user.Entity.Id,
        }, CancellationToken.None);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.UserItemNotFound));
    }

    [Test]
    public async Task NotFoundUserItem()
    {
        User user = new();
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        var itemService = Mock.Of<IItemService>();
        SellUserItemCommand.Handler handler = new(ActDb, itemService, Mock.Of<IActivityLogService>());
        var result = await handler.Handle(new SellUserItemCommand
        {
            UserItemId = 1,
            UserId = user.Id,
        }, CancellationToken.None);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.UserItemNotFound));
    }

    [Test]
    public async Task ShouldReturnErrorIfItemIsNotEnabled()
    {
        User user = new()
        {
            Gold = 0,
            Items =
            [
                new()
                {
                    Item = new Item { Enabled = false },
                },
            ],
        };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        IItemService itemService = Mock.Of<IItemService>();
        SellUserItemCommand.Handler handler = new(ActDb, itemService, Mock.Of<IActivityLogService>());
        var result = await handler.Handle(new SellUserItemCommand
        {
            UserItemId = user.Items[0].Id,
            UserId = user.Id,
        }, CancellationToken.None);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.ItemDisabled));
    }

    [Test]
    public async Task HeirloomShouldNotBeSellable()
    {
        Item heirloomedItem = new() { Id = "heirloomedItem_h1", Rank = 1, Enabled = true };
        User user = new()
        {
            Gold = 0,
            Items =
            [
                new()
                {
                    Id = 1,
                    Item = heirloomedItem,
                },
            ],
        };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        IItemService itemService = Mock.Of<IItemService>();
        SellUserItemCommand.Handler handler = new(ActDb, itemService, Mock.Of<IActivityLogService>());
        var result = await handler.Handle(new SellUserItemCommand
        {
            UserItemId = user.Items[0].Id,
            UserId = user.Id,
        }, CancellationToken.None);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.ItemNotSellable));
    }

    [Test]
    public async Task PersonalItemShouldNotBeSellable()
    {
        User user = new()
        {
            Items = [
                new()
                {
                    Item = new Item { Enabled = true },
                    PersonalItem = new(),
                }
            ],
        };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        SellUserItemCommand.Handler handler = new(ActDb, Mock.Of<IItemService>(), Mock.Of<IActivityLogService>());
        var result = await handler.Handle(new SellUserItemCommand
        {
            UserItemId = user.Items[0].Id,
            UserId = user.Id,
        }, CancellationToken.None);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.ItemNotSellable));
    }

    [Test]
    public async Task ClanArmoryItemShouldNotBeSellable()
    {
        User user = new()
        {
            Items =
            [
                new()
                {
                    Item = new Item { Enabled = true },
                    ClanArmoryItem = new ClanArmoryItem(),
                },
            ],
        };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        SellUserItemCommand.Handler handler = new(ActDb, Mock.Of<IItemService>(), Mock.Of<IActivityLogService>());
        var result = await handler.Handle(new SellUserItemCommand
        {
            UserItemId = user.Items[0].Id,
            UserId = user.Id,
        }, CancellationToken.None);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.ItemNotSellable));
    }

    [Test]
    public async Task ClanArmoryBorrowedItemShouldNotBeSellable()
    {
        User user = new()
        {
            Items =
            {
                new()
                {
                    Item = new Item { Enabled = true },
                    ClanArmoryBorrowedItem = new ClanArmoryBorrowedItem { BorrowerUserId = 99 },
                },
            },
        };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        SellUserItemCommand.Handler handler = new(ActDb, Mock.Of<IItemService>(), Mock.Of<IActivityLogService>());
        var result = await handler.Handle(new SellUserItemCommand
        {
            UserItemId = user.Items[0].Id,
            UserId = user.Id,
        }, CancellationToken.None);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.ItemNotSellable));
    }

    [Test]
    public async Task MarketplaceListedItemShouldNotBeSellable()
    {
        User user = new()
        {
            Items = [
                new()
                {
                    Item = new Item { Enabled = true },
                }
            ],
        };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        ArrangeDb.MarketplaceListings.Add(MarketplaceListingFactory.CreateListing(sellerId: user.Id, offeredUserItemId: user.Items[0].Id));
        await ArrangeDb.SaveChangesAsync();

        SellUserItemCommand.Handler handler = new(ActDb, Mock.Of<IItemService>(), Mock.Of<IActivityLogService>());
        var result = await handler.Handle(new SellUserItemCommand
        {
            UserItemId = user.Items[0].Id,
            UserId = user.Id,
        }, CancellationToken.None);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.ItemNotSellable));
    }
}
