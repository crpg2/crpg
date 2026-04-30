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

namespace Crpg.Application.UTest.Marketplace.AcceptMarketplaceListing;

internal class AcceptMarketplaceListingSideEffectsTest : AcceptMarketplaceListingCommandTestBase
{
    [Test]
    public async Task ShouldCreateActivityLogOnAccept()
    {
        User seller = new() { Gold = 0 };
        User buyer = new() { Gold = 100 };
        ArrangeDb.Users.AddRange(seller, buyer);
        await ArrangeDb.SaveChangesAsync();

        var listing = MarketplaceListingFactory.CreateListing(seller.Id, goldFee: 5, offeredGold: 50, requestedGold: 100);
        ArrangeDb.MarketplaceListings.Add(listing);
        await ArrangeDb.SaveChangesAsync();

        await CreateHandler(activityLog: new ActivityLogService(new MetadataService())).Handle(
            new AcceptMarketplaceListingCommand { UserId = buyer.Id, ListingId = listing.Id },
            CancellationToken.None);

        ActivityLog? log = await AssertDb.ActivityLogs
            .Include(l => l.Metadata)
            .SingleOrDefaultAsync(l => l.UserId == buyer.Id && l.Type == ActivityLogType.MarketplaceListingAccepted);

        Assert.That(log, Is.Not.Null);
        Assert.That(log!.Metadata.Single(m => m.Key == "listingId").Value, Is.EqualTo(listing.Id.ToString()));
        Assert.That(log.Metadata.Single(m => m.Key == "sellerId").Value, Is.EqualTo(seller.Id.ToString()));
        Assert.That(log.Metadata.Single(m => m.Key == "goldFee").Value, Is.EqualTo("5"));
        Assert.That(log.Metadata.Single(m => m.Key == "offer").Value, Does.Contain("\"gold\":50"));
        Assert.That(log.Metadata.Single(m => m.Key == "request").Value, Does.Contain("\"gold\":100"));
    }

    [Test]
    public async Task ShouldCreateNotificationsForBothSellerAndBuyerOnAccept()
    {
        User seller = new() { Gold = 0 };
        User buyer = new() { Gold = 100 };
        ArrangeDb.Users.AddRange(seller, buyer);
        await ArrangeDb.SaveChangesAsync();

        var listing = MarketplaceListingFactory.CreateListing(seller.Id, offeredGold: 50, requestedGold: 100);
        ArrangeDb.MarketplaceListings.Add(listing);
        await ArrangeDb.SaveChangesAsync();

        await CreateHandler(notifications: new UserNotificationService(new MetadataService())).Handle(
            new AcceptMarketplaceListingCommand { UserId = buyer.Id, ListingId = listing.Id },
            CancellationToken.None);

        bool sellerNotified = await AssertDb.UserNotifications
            .AnyAsync(n => n.UserId == seller.Id && n.Type == NotificationType.MarketplaceListingAcceptedToSeller);
        bool buyerNotified = await AssertDb.UserNotifications
            .AnyAsync(n => n.UserId == buyer.Id && n.Type == NotificationType.MarketplaceListingAcceptedToBuyer);

        Assert.That(sellerNotified, Is.True);
        Assert.That(buyerNotified, Is.True);
    }

    [Test]
    public async Task ShouldInvalidateSellerListingsContainingOfferedItemAfterTransfer()
    {
        User seller = new();
        User buyer = new();
        ArrangeDb.Users.AddRange(seller, buyer);
        ArrangeDb.Items.Add(new Item { Id = "sword" });
        await ArrangeDb.SaveChangesAsync();

        UserItem sellerItem = new() { UserId = seller.Id, ItemId = "sword" };
        ArrangeDb.UserItems.Add(sellerItem);
        await ArrangeDb.SaveChangesAsync();

        var listing = MarketplaceListingFactory.CreateListing(seller.Id, offeredUserItemId: sellerItem.Id);
        ArrangeDb.MarketplaceListings.Add(listing);
        await ArrangeDb.SaveChangesAsync();

        Mock<IMarketplaceService> marketplace = new();

        await CreateHandler(marketplace: marketplace.Object).Handle(new AcceptMarketplaceListingCommand
        {
            UserId = buyer.Id,
            ListingId = listing.Id,
        }, CancellationToken.None);

        marketplace.Verify(m => m.InvalidateListingsByUserItemIdAsync(
            It.IsAny<ICrpgDbContext>(),
            It.IsAny<IActivityLogService>(),
            It.IsAny<IUserNotificationService>(),
            sellerId: seller.Id,
            userItemId: sellerItem.Id,
            excludeListingId: listing.Id,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ShouldInvalidateBuyerListingsContainingRequestedItemAfterTransfer()
    {
        User seller = new();
        User buyer = new();
        ArrangeDb.Users.AddRange(seller, buyer);
        ArrangeDb.Items.Add(new Item { Id = "shield" });
        await ArrangeDb.SaveChangesAsync();

        UserItem buyerItem = new() { UserId = buyer.Id, ItemId = "shield" };
        ArrangeDb.UserItems.Add(buyerItem);
        await ArrangeDb.SaveChangesAsync();

        var listing = MarketplaceListingFactory.CreateListing(seller.Id, offeredGold: 10, requestedItemId: buyerItem.ItemId);
        ArrangeDb.MarketplaceListings.Add(listing);
        await ArrangeDb.SaveChangesAsync();

        Mock<IMarketplaceService> marketplace = new();

        await CreateHandler(marketplace: marketplace.Object).Handle(new AcceptMarketplaceListingCommand
        {
            UserId = buyer.Id,
            ListingId = listing.Id,
        }, CancellationToken.None);

        marketplace.Verify(m => m.InvalidateListingsByUserItemIdAsync(
            It.IsAny<ICrpgDbContext>(),
            It.IsAny<IActivityLogService>(),
            It.IsAny<IUserNotificationService>(),
            sellerId: buyer.Id,
            userItemId: buyerItem.Id,
            excludeListingId: listing.Id,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ShouldNotCallInvalidateWhenNoItemsAreTraded()
    {
        User seller = new();
        User buyer = new() { Gold = 100 };
        ArrangeDb.Users.AddRange(seller, buyer);
        await ArrangeDb.SaveChangesAsync();

        var listing = MarketplaceListingFactory.CreateListing(seller.Id, offeredGold: 50, requestedGold: 100);
        ArrangeDb.MarketplaceListings.Add(listing);
        await ArrangeDb.SaveChangesAsync();

        Mock<IMarketplaceService> marketplace = new();

        await CreateHandler(marketplace: marketplace.Object).Handle(new AcceptMarketplaceListingCommand
        {
            UserId = buyer.Id,
            ListingId = listing.Id,
        }, CancellationToken.None);

        marketplace.Verify(m => m.InvalidateListingsByUserItemIdAsync(
            It.IsAny<ICrpgDbContext>(),
            It.IsAny<IActivityLogService>(),
            It.IsAny<IUserNotificationService>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }
}
