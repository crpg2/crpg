using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Items.Commands;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Users;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Items;

public class SellUserItemCommandTest : TestBase
{
    private static readonly Mock<IActivityLogService> ActivityLogService = new() { DefaultValue = DefaultValue.Mock };
    private static readonly IItemService ItemService = Mock.Of<IItemService>();

    [Test]
    public async Task ShouldCallItemService()
    {
        User user = new()
        {
            Gold = 0,
            Items = new List<UserItem>
            {
                new()
                {
                    Item = new Item { Price = 100, Enabled = true },
                },
            },
        };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        Mock<IItemService> itemServiceMock = new();
        SellUserItemCommand.Handler handler = new(ActDb, itemServiceMock.Object, ActivityLogService.Object);
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

        SellUserItemCommand.Handler handler = new(ActDb, ItemService, ActivityLogService.Object);
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

        SellUserItemCommand.Handler handler = new(ActDb, ItemService, ActivityLogService.Object);
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
            Items = new List<UserItem>
            {
                new()
                {
                    Item = new Item { Enabled = false },
                },
            },
        };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        SellUserItemCommand.Handler handler = new(ActDb, ItemService, ActivityLogService.Object);
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
            Items = new List<UserItem>
            {
                new()
                {
                    Id = 1,
                    Item = heirloomedItem,
                },
            },
        };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        SellUserItemCommand.Handler handler = new(ActDb, ItemService, ActivityLogService.Object);
        var result = await handler.Handle(new SellUserItemCommand
        {
            UserItemId = user.Items[0].Id,
            UserId = user.Id,
        }, CancellationToken.None);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.ItemNotSellable));
    }
}
