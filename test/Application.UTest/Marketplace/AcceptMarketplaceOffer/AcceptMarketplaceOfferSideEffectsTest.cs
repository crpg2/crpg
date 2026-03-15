using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Services;
using Crpg.Application.Marketplace.Commands;
using Crpg.Application.Marketplace.Services;
using Crpg.Domain.Entities.ActivityLogs;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Notifications;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using static Crpg.Application.UTest.Marketplace.MarketplaceOfferFactory;

namespace Crpg.Application.UTest.Marketplace.AcceptMarketplaceOffer;

internal class AcceptMarketplaceOfferSideEffectsTest : AcceptMarketplaceOfferCommandTestBase
{
    [Test]
    public async Task ShouldCreateActivityLogOnAccept()
    {
        User seller = new() { Gold = 0 };
        User buyer = new() { Gold = 100 };
        ArrangeDb.Users.AddRange(seller, buyer);
        await ArrangeDb.SaveChangesAsync();

        var offer = CreateOffer(seller.Id, goldFee: 5, offeredGold: 50, requestedGold: 100);
        ArrangeDb.MarketplaceOffers.Add(offer);
        await ArrangeDb.SaveChangesAsync();

        await CreateHandler(activityLog: new ActivityLogService(new MetadataService())).Handle(
            new AcceptMarketplaceOfferCommand { UserId = buyer.Id, OfferId = offer.Id },
            CancellationToken.None);

        ActivityLog? log = await AssertDb.ActivityLogs
            .Include(l => l.Metadata)
            .SingleOrDefaultAsync(l => l.UserId == buyer.Id && l.Type == ActivityLogType.MarketplaceOfferAccepted);

        Assert.That(log, Is.Not.Null);
        Assert.That(log!.Metadata.Single(m => m.Key == "offerId").Value, Is.EqualTo(offer.Id.ToString()));
        Assert.That(log.Metadata.Single(m => m.Key == "sellerId").Value, Is.EqualTo(seller.Id.ToString()));
        Assert.That(log.Metadata.Single(m => m.Key == "goldFee").Value, Is.EqualTo("5"));
        Assert.That(log.Metadata.Single(m => m.Key == "offered").Value, Does.Contain("\"gold\":50"));
        Assert.That(log.Metadata.Single(m => m.Key == "requested").Value, Does.Contain("\"gold\":100"));
    }

    [Test]
    public async Task ShouldCreateNotificationsForBothSellerAndBuyerOnAccept()
    {
        User seller = new() { Gold = 0 };
        User buyer = new() { Gold = 100 };
        ArrangeDb.Users.AddRange(seller, buyer);
        await ArrangeDb.SaveChangesAsync();

        var offer = CreateOffer(seller.Id, offeredGold: 50, requestedGold: 100);
        ArrangeDb.MarketplaceOffers.Add(offer);
        await ArrangeDb.SaveChangesAsync();

        await CreateHandler(notifications: new UserNotificationService(new MetadataService())).Handle(
            new AcceptMarketplaceOfferCommand { UserId = buyer.Id, OfferId = offer.Id },
            CancellationToken.None);

        bool sellerNotified = await AssertDb.UserNotifications
            .AnyAsync(n => n.UserId == seller.Id && n.Type == NotificationType.MarketplaceOfferAcceptedToSeller);
        bool buyerNotified = await AssertDb.UserNotifications
            .AnyAsync(n => n.UserId == buyer.Id && n.Type == NotificationType.MarketplaceOfferAcceptedToBuyer);

        Assert.That(sellerNotified, Is.True);
        Assert.That(buyerNotified, Is.True);
    }

    [Test]
    public async Task ShouldInvalidateSellerOffersContainingOfferedItemAfterTransfer()
    {
        User seller = new();
        User buyer = new();
        ArrangeDb.Users.AddRange(seller, buyer);
        ArrangeDb.Items.Add(new Item { Id = "sword" });
        await ArrangeDb.SaveChangesAsync();

        UserItem sellerItem = new() { UserId = seller.Id, ItemId = "sword" };
        ArrangeDb.UserItems.Add(sellerItem);
        await ArrangeDb.SaveChangesAsync();

        var offer = CreateOffer(seller.Id, offeredUserItemId: sellerItem.Id);
        ArrangeDb.MarketplaceOffers.Add(offer);
        await ArrangeDb.SaveChangesAsync();

        Mock<IMarketplaceService> marketplace = new();

        await CreateHandler(marketplace: marketplace.Object).Handle(new AcceptMarketplaceOfferCommand
        {
            UserId = buyer.Id,
            OfferId = offer.Id,
        }, CancellationToken.None);

        marketplace.Verify(m => m.InvalidateOffersByUserItemIdAsync(
            It.IsAny<ICrpgDbContext>(),
            It.IsAny<IActivityLogService>(),
            It.IsAny<IUserNotificationService>(),
            sellerId: seller.Id,
            userItemId: sellerItem.Id,
            excludeOfferId: offer.Id,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ShouldInvalidateBuyerOffersContainingRequestedItemAfterTransfer()
    {
        User seller = new();
        User buyer = new();
        ArrangeDb.Users.AddRange(seller, buyer);
        ArrangeDb.Items.Add(new Item { Id = "shield" });
        await ArrangeDb.SaveChangesAsync();

        UserItem buyerItem = new() { UserId = buyer.Id, ItemId = "shield" };
        ArrangeDb.UserItems.Add(buyerItem);
        await ArrangeDb.SaveChangesAsync();

        var offer = CreateOffer(seller.Id, offeredGold: 10, requestedItemId: "shield");
        ArrangeDb.MarketplaceOffers.Add(offer);
        await ArrangeDb.SaveChangesAsync();

        Mock<IMarketplaceService> marketplace = new();

        await CreateHandler(marketplace: marketplace.Object).Handle(new AcceptMarketplaceOfferCommand
        {
            UserId = buyer.Id,
            OfferId = offer.Id,
        }, CancellationToken.None);

        marketplace.Verify(m => m.InvalidateOffersByUserItemIdAsync(
            It.IsAny<ICrpgDbContext>(),
            It.IsAny<IActivityLogService>(),
            It.IsAny<IUserNotificationService>(),
            sellerId: buyer.Id,
            userItemId: buyerItem.Id,
            excludeOfferId: offer.Id,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ShouldNotCallInvalidateWhenNoItemsAreTraded()
    {
        User seller = new();
        User buyer = new() { Gold = 100 };
        ArrangeDb.Users.AddRange(seller, buyer);
        await ArrangeDb.SaveChangesAsync();

        var offer = CreateOffer(seller.Id, offeredGold: 50, requestedGold: 100);
        ArrangeDb.MarketplaceOffers.Add(offer);
        await ArrangeDb.SaveChangesAsync();

        Mock<IMarketplaceService> marketplace = new();

        await CreateHandler(marketplace: marketplace.Object).Handle(new AcceptMarketplaceOfferCommand
        {
            UserId = buyer.Id,
            OfferId = offer.Id,
        }, CancellationToken.None);

        marketplace.Verify(m => m.InvalidateOffersByUserItemIdAsync(
            It.IsAny<ICrpgDbContext>(),
            It.IsAny<IActivityLogService>(),
            It.IsAny<IUserNotificationService>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }
}
