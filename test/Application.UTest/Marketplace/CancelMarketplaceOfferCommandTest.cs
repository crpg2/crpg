using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Marketplace.Commands;
using Crpg.Application.Marketplace.Services;
using Crpg.Domain.Entities.ActivityLogs;
using Crpg.Domain.Entities.Marketplace;
using Crpg.Domain.Entities.Notifications;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using static Crpg.Application.UTest.Marketplace.MarketplaceOfferFactory;

namespace Crpg.Application.UTest.Marketplace;

public class CancelMarketplaceOfferCommandTest : TestBase
{
    [Test]
    public async Task ShouldReturnOfferNotFoundIfOfferDoesNotExist()
    {
        User seller = new();
        ArrangeDb.Users.Add(seller);
        await ArrangeDb.SaveChangesAsync();

        var result = await new CancelMarketplaceOfferCommand.Handler(ActDb, Mock.Of<IActivityLogService>(), new MarketplaceService()).Handle(new CancelMarketplaceOfferCommand
        {
            UserId = seller.Id,
            OfferId = 999,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Not.Null);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.MarketplaceOfferNotFound));
    }

    [Test]
    public async Task ShouldReturnOfferNotAllowedIfUserIsNotSeller()
    {
        User seller = new();
        User otherUser = new();
        ArrangeDb.Users.AddRange(seller, otherUser);
        await ArrangeDb.SaveChangesAsync();

        MarketplaceOffer offer = CreateOffer(seller.Id, offeredGold: 25, requestedGold: 40);
        ArrangeDb.MarketplaceOffers.Add(offer);
        await ArrangeDb.SaveChangesAsync();

        var result = await new CancelMarketplaceOfferCommand.Handler(ActDb, Mock.Of<IActivityLogService>(), new MarketplaceService()).Handle(new CancelMarketplaceOfferCommand
        {
            UserId = otherUser.Id,
            OfferId = offer.Id,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Not.Null);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.MarketplaceOfferNotAllowed));
    }

    [Test]
    public async Task ShouldRefundGoldAndHeirloomPointsAndRemoveOfferWhenCancelled()
    {
        User seller = new() { Gold = 100, HeirloomPoints = 10 };
        ArrangeDb.Users.Add(seller);
        await ArrangeDb.SaveChangesAsync();

        MarketplaceOffer offer = CreateOffer(seller.Id, goldFee: 5, offeredGold: 25, requestedGold: 40, offeredHeirloomPoints: 3);
        ArrangeDb.MarketplaceOffers.Add(offer);
        await ArrangeDb.SaveChangesAsync();

        var activityLogService = new ActivityLogService(new MetadataService());

        var result = await new CancelMarketplaceOfferCommand.Handler(ActDb, activityLogService, new MarketplaceService()).Handle(new CancelMarketplaceOfferCommand
        {
            UserId = seller.Id,
            OfferId = offer.Id,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Null);

        User? dbSeller = await AssertDb.Users.FindAsync(seller.Id);
        Assert.That(dbSeller, Is.Not.Null);
        Assert.That(dbSeller!.Gold, Is.EqualTo(130));
        Assert.That(dbSeller.HeirloomPoints, Is.EqualTo(13));
        Assert.That(await AssertDb.MarketplaceOffers.AnyAsync(o => o.Id == offer.Id), Is.False);
    }

    [Test]
    public async Task ShouldCreateActivityLogWhenOfferIsCancelled()
    {
        User seller = new() { Gold = 100, HeirloomPoints = 10 };
        ArrangeDb.Users.Add(seller);
        await ArrangeDb.SaveChangesAsync();

        MarketplaceOffer offer = CreateOffer(seller.Id, goldFee: 5, offeredGold: 25, requestedGold: 40, offeredHeirloomPoints: 3);
        ArrangeDb.MarketplaceOffers.Add(offer);
        await ArrangeDb.SaveChangesAsync();

        var result = await new CancelMarketplaceOfferCommand.Handler(ActDb, new ActivityLogService(new MetadataService()), new MarketplaceService()).Handle(new CancelMarketplaceOfferCommand
        {
            UserId = seller.Id,
            OfferId = offer.Id,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Null);

        ActivityLog? activityLog = await AssertDb.ActivityLogs
            .Include(l => l.Metadata)
            .SingleOrDefaultAsync(l => l.UserId == seller.Id && l.Type == ActivityLogType.MarketplaceOfferCancelled);

        Assert.That(activityLog, Is.Not.Null);
        Assert.That(activityLog!.Metadata.Single(m => m.Key == "offerId").Value, Is.EqualTo(offer.Id.ToString()));
        Assert.That(activityLog!.Metadata.Single(m => m.Key == "goldFee").Value, Is.EqualTo("5"));
        Assert.That(activityLog.Metadata.Single(m => m.Key == "offered").Value, Does.Contain("\"gold\":25"));
        Assert.That(activityLog.Metadata.Single(m => m.Key == "offered").Value, Does.Contain("\"heirloomPoints\":3"));
        Assert.That(activityLog.Metadata.Single(m => m.Key == "requested").Value, Does.Contain("\"gold\":40"));
    }
}
