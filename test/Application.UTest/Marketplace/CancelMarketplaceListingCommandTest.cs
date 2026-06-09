using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Marketplace.Commands;
using Crpg.Application.Marketplace.Services;
using Crpg.Domain.Entities.ActivityLogs;
using Crpg.Domain.Entities.Marketplace;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Marketplace;

public class CancelMarketplaceListingCommandTest : TestBase
{
    [Test]
    public async Task ShouldReturnListingNotFoundIfListingDoesNotExist()
    {
        User seller = new();
        ArrangeDb.Users.Add(seller);
        await ArrangeDb.SaveChangesAsync();

        var result = await new CancelMarketplaceListingCommand.Handler(ActDb, Mock.Of<IActivityLogService>(), new MarketplaceService()).Handle(new CancelMarketplaceListingCommand
        {
            UserId = seller.Id,
            ListingId = 999,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Not.Null);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.MarketplaceListingNotFound));
    }

    [Test]
    public async Task ShouldReturnListingNotAllowedIfUserIsNotSeller()
    {
        User seller = new();
        User otherUser = new();
        ArrangeDb.Users.AddRange(seller, otherUser);
        await ArrangeDb.SaveChangesAsync();

        MarketplaceListing listing = MarketplaceListingFactory.CreateListing(seller.Id, offeredGold: 25, requestedGold: 40);
        ArrangeDb.MarketplaceListings.Add(listing);
        await ArrangeDb.SaveChangesAsync();

        var result = await new CancelMarketplaceListingCommand.Handler(ActDb, Mock.Of<IActivityLogService>(), new MarketplaceService()).Handle(new CancelMarketplaceListingCommand
        {
            UserId = otherUser.Id,
            ListingId = listing.Id,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Not.Null);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.MarketplaceListingNotAllowed));
    }

    [Test]
    public async Task ShouldRefundGoldAndHeirloomPointsAndRemoveListingWhenCancelled()
    {
        User seller = new() { Gold = 100, HeirloomPoints = 10 };
        ArrangeDb.Users.Add(seller);
        await ArrangeDb.SaveChangesAsync();

        MarketplaceListing listing = MarketplaceListingFactory.CreateListing(seller.Id, goldFee: 5, offeredGold: 25, requestedGold: 40, offeredHeirloomPoints: 3);
        ArrangeDb.MarketplaceListings.Add(listing);
        await ArrangeDb.SaveChangesAsync();

        var activityLogService = new ActivityLogService(new MetadataService());

        var result = await new CancelMarketplaceListingCommand.Handler(ActDb, activityLogService, new MarketplaceService()).Handle(new CancelMarketplaceListingCommand
        {
            UserId = seller.Id,
            ListingId = listing.Id,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Null);

        User? dbSeller = await AssertDb.Users.FindAsync(seller.Id);
        Assert.That(dbSeller, Is.Not.Null);
        Assert.That(dbSeller!.Gold, Is.EqualTo(130));
        Assert.That(dbSeller.HeirloomPoints, Is.EqualTo(13));
        Assert.That(await AssertDb.MarketplaceListings.AnyAsync(l => l.Id == listing.Id), Is.False);
    }

    [Test]
    public async Task ShouldCreateActivityLogWhenListingIsCancelled()
    {
        User seller = new() { Gold = 100, HeirloomPoints = 10 };
        ArrangeDb.Users.Add(seller);
        await ArrangeDb.SaveChangesAsync();

        MarketplaceListing listing = MarketplaceListingFactory.CreateListing(seller.Id, goldFee: 5, offeredGold: 25, requestedGold: 40, offeredHeirloomPoints: 3);
        ArrangeDb.MarketplaceListings.Add(listing);
        await ArrangeDb.SaveChangesAsync();

        var result = await new CancelMarketplaceListingCommand.Handler(ActDb, new ActivityLogService(new MetadataService()), new MarketplaceService()).Handle(new CancelMarketplaceListingCommand
        {
            UserId = seller.Id,
            ListingId = listing.Id,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Null);

        ActivityLog? activityLog = await AssertDb.ActivityLogs
            .Include(l => l.Metadata)
            .SingleOrDefaultAsync(l => l.UserId == seller.Id && l.Type == ActivityLogType.MarketplaceListingCancelled);

        Assert.That(activityLog, Is.Not.Null);
        Assert.That(activityLog!.Metadata.Single(m => m.Key == "listingId").Value, Is.EqualTo(listing.Id.ToString()));
        Assert.That(activityLog!.Metadata.Single(m => m.Key == "goldFee").Value, Is.EqualTo("5"));
        Assert.That(activityLog.Metadata.Single(m => m.Key == "offer").Value, Does.Contain("\"gold\":25"));
        Assert.That(activityLog.Metadata.Single(m => m.Key == "offer").Value, Does.Contain("\"heirloomPoints\":3"));
        Assert.That(activityLog.Metadata.Single(m => m.Key == "request").Value, Does.Contain("\"gold\":40"));
    }
}
