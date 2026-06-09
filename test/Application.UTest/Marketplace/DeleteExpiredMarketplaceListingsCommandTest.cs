using Crpg.Application.Common.Services;
using Crpg.Application.Marketplace.Commands;
using Crpg.Application.Marketplace.Services;
using Crpg.Domain.Entities.ActivityLogs;
using Crpg.Domain.Entities.Notifications;
using Crpg.Domain.Entities.Users;
using Crpg.Sdk.Abstractions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Marketplace;

public class DeleteExpiredMarketplaceListingsCommandTest : TestBase
{
    [Test]
    public async Task ShouldDeleteExpiredListingsAndRefundSeller()
    {
        DateTime now = DateTime.UtcNow;
        var dateTimeMock = new Mock<IDateTime>();
        dateTimeMock.Setup(d => d.UtcNow).Returns(now);

        User seller = new() { Gold = 10, HeirloomPoints = 1 };
        ArrangeDb.Users.Add(seller);
        await ArrangeDb.SaveChangesAsync();

        var expiredListing = MarketplaceListingFactory.CreateListing(seller.Id, goldFee: 5, offeredGold: 50, offeredHeirloomPoints: 2, expiresAt: now.AddMinutes(-1));
        var activeListing = MarketplaceListingFactory.CreateListing(seller.Id, goldFee: 3, offeredGold: 7, expiresAt: now.AddMinutes(10));

        ArrangeDb.MarketplaceListings.AddRange(expiredListing, activeListing);
        await ArrangeDb.SaveChangesAsync();

        var result = await new DeleteExpiredMarketplaceListingsCommand.Handler(ActDb, dateTimeMock.Object, new UserNotificationService(new MetadataService()), new ActivityLogService(new MetadataService()), new MarketplaceService())
            .Handle(new DeleteExpiredMarketplaceListingsCommand(), CancellationToken.None);

        Assert.That(result.Errors, Is.Null);

        User? dbSeller = await AssertDb.Users.FindAsync(seller.Id);
        Assert.That(dbSeller, Is.Not.Null);
        Assert.That(dbSeller!.Gold, Is.EqualTo(65));
        Assert.That(dbSeller.HeirloomPoints, Is.EqualTo(3));
        Assert.That(AssertDb.MarketplaceListings.Count(), Is.EqualTo(1));

        var notification = await AssertDb.UserNotifications
            .Include(n => n.Metadata)
            .SingleOrDefaultAsync(n => n.UserId == seller.Id && n.Type == NotificationType.MarketplaceListingExpired);

        Assert.That(notification, Is.Not.Null);
        Assert.That(notification!.Metadata.Single(m => m.Key == "listingId").Value, Is.Not.Empty);
        Assert.That(notification.Metadata.Single(m => m.Key == "goldFee").Value, Is.EqualTo("5"));
        Assert.That(notification.Metadata.Single(m => m.Key == "offer").Value, Does.Contain("\"gold\":50"));
        Assert.That(notification.Metadata.Single(m => m.Key == "offer").Value, Does.Contain("\"heirloomPoints\":2"));

        var activityLog = await AssertDb.ActivityLogs
            .Include(l => l.Metadata)
            .SingleOrDefaultAsync(l => l.UserId == seller.Id && l.Type == ActivityLogType.MarketplaceListingExpired);

        Assert.That(activityLog, Is.Not.Null);
        Assert.That(activityLog!.Metadata.Single(m => m.Key == "listingId").Value, Is.Not.Empty);
        Assert.That(activityLog.Metadata.Single(m => m.Key == "goldFee").Value, Is.EqualTo("5"));
        Assert.That(activityLog.Metadata.Single(m => m.Key == "offer").Value, Does.Contain("\"gold\":50"));
        Assert.That(activityLog.Metadata.Single(m => m.Key == "offer").Value, Does.Contain("\"heirloomPoints\":2"));
    }
}
