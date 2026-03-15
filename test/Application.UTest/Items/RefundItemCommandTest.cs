using Crpg.Application.Common;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Items.Commands;
using Crpg.Application.Marketplace.Services;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Marketplace;
using Crpg.Domain.Entities.Users;
using Crpg.Sdk.Abstractions;
using Moq;
using NUnit.Framework;
using static Crpg.Application.UTest.Marketplace.MarketplaceOfferFactory;

namespace Crpg.Application.UTest.Items;

public class RefundItemCommandTest : TestBase
{
    [Test]
    public async Task ShouldReturnErrorIfItemIsNotFound()
    {
        User user = new();
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        var result = await new RefundItemCommand.Handler(ActDb, new ActivityLogService(new MetadataService()), Mock.Of<IItemService>(), new MarketplaceService(), new UserNotificationService(new MetadataService())).Handle(new RefundItemCommand
        {
            ItemId = "a",
            UserId = user.Id,
        }, CancellationToken.None);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.ItemNotFound));
    }

    [Test]
    public async Task ShouldRefundItem()
    {
        Item item = new() { Id = "a", Enabled = true, Price = 100, Rank = 1 };
        ArrangeDb.Items.Add(item);
        User user = new();
        ArrangeDb.Users.Add(user);
        UserItem userItem = new() { User = user, ItemId = item.Id };
        ArrangeDb.UserItems.Add(userItem);
        await ArrangeDb.SaveChangesAsync();

        var result = await new RefundItemCommand.Handler(ActDb, new ActivityLogService(new MetadataService()), new ItemService(Mock.Of<IDateTime>(), new Constants()), new MarketplaceService(), new UserNotificationService(new MetadataService())).Handle(new RefundItemCommand
        {
            ItemId = item.Id,
            UserId = user.Id,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Null);
        user = AssertDb.Users.First(u => u.Id == user.Id);
        Assert.That(user.Gold, Is.EqualTo(100));
        Assert.That(user.HeirloomPoints, Is.EqualTo(1));
    }

    [Test]
    public async Task ShouldRefundItemWithNoHeirloomPointsIfRankIsZero()
    {
        Item item = new() { Id = "a", Enabled = true, Price = 200, Rank = 0 };
        ArrangeDb.Items.Add(item);
        User user = new();
        ArrangeDb.Users.Add(user);
        UserItem userItem = new() { User = user, ItemId = item.Id };
        ArrangeDb.UserItems.Add(userItem);
        await ArrangeDb.SaveChangesAsync();

        var result = await new RefundItemCommand.Handler(ActDb, new ActivityLogService(new MetadataService()), new ItemService(Mock.Of<IDateTime>(), new Constants()), new MarketplaceService(), new UserNotificationService(new MetadataService())).Handle(new RefundItemCommand
        {
            ItemId = item.Id,
            UserId = user.Id,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Null);
        user = AssertDb.Users.First(u => u.Id == user.Id);
        Assert.That(user.Gold, Is.EqualTo(200));
        Assert.That(user.HeirloomPoints, Is.EqualTo(0));
    }

    [Test]
    public async Task ShouldInvalidateMarketplaceOffersOnRefund()
    {
        Item item = new() { Id = "a", Enabled = true, Price = 100, Rank = 0 };
        ArrangeDb.Items.Add(item);
        User seller = new() { Gold = 0 };
        ArrangeDb.Users.Add(seller);
        UserItem sellerUserItem = new() { User = seller, ItemId = item.Id };
        ArrangeDb.UserItems.Add(sellerUserItem);
        await ArrangeDb.SaveChangesAsync();

        MarketplaceOffer offer = CreateOffer(seller.Id, goldFee: 10, offeredGold: 50, offeredUserItemId: sellerUserItem.Id);
        ArrangeDb.MarketplaceOffers.Add(offer);
        await ArrangeDb.SaveChangesAsync();

        var result = await new RefundItemCommand.Handler(ActDb, new ActivityLogService(new MetadataService()), new ItemService(Mock.Of<IDateTime>(), new Constants()), new MarketplaceService(), new UserNotificationService(new MetadataService())).Handle(new RefundItemCommand
        {
            ItemId = item.Id,
            UserId = seller.Id,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Null);
        Assert.That(AssertDb.MarketplaceOffers.Any(), Is.False);
        seller = AssertDb.Users.First(u => u.Id == seller.Id);
        // item refund (Price=100) + offer refund (offeredGold=50 + goldFee=10)
        Assert.That(seller.Gold, Is.EqualTo(160));
    }
}
