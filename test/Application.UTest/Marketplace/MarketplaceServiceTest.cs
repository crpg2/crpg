using Crpg.Application.Marketplace.Services;
using Crpg.Domain.Entities.Marketplace;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Crpg.Application.UTest.Marketplace;

public class MarketplaceServiceTest : TestBase
{
    private IMarketplaceService _marketplaceService = default!;

    public override Task SetUp()
    {
        _marketplaceService = new MarketplaceService();
        return base.SetUp();
    }

    [Test]
    public async Task ShouldRefundOfferedGoldAndGoldFeeToSeller()
    {
        User seller = new() { Gold = 100 };
        ArrangeDb.Users.Add(seller);
        await ArrangeDb.SaveChangesAsync();

        MarketplaceListing listing = MarketplaceListingFactory.CreateListing(seller.Id, goldFee: 10, offeredGold: 50, requestedGold: 80);
        ArrangeDb.MarketplaceListings.Add(listing);
        await ArrangeDb.SaveChangesAsync();

        var loadedListing = await ActDb.MarketplaceListings
            .Include(l => l.Assets)
            .Include(l => l.Seller)
            .FirstAsync(l => l.Id == listing.Id);
        _marketplaceService.RefundAndRemoveListing(ActDb, loadedListing);
        await ActDb.SaveChangesAsync();

        User? dbSeller = await AssertDb.Users.FindAsync(seller.Id);
        Assert.That(dbSeller!.Gold, Is.EqualTo(160)); // 100 + 50 + 10
    }

    [Test]
    public async Task ShouldRefundHeirloomPointsToSeller()
    {
        User seller = new() { HeirloomPoints = 5 };
        ArrangeDb.Users.Add(seller);
        await ArrangeDb.SaveChangesAsync();

        MarketplaceListing listing = MarketplaceListingFactory.CreateListing(seller.Id, offeredHeirloomPoints: 3, requestedGold: 80);
        ArrangeDb.MarketplaceListings.Add(listing);
        await ArrangeDb.SaveChangesAsync();

        var loadedListing = await ActDb.MarketplaceListings
            .Include(l => l.Assets)
            .Include(l => l.Seller)
            .FirstAsync(l => l.Id == listing.Id);
        _marketplaceService.RefundAndRemoveListing(ActDb, loadedListing);
        await ActDb.SaveChangesAsync();

        User? dbSeller = await AssertDb.Users.FindAsync(seller.Id);
        Assert.That(dbSeller!.HeirloomPoints, Is.EqualTo(8)); // 5 + 3
    }

    [Test]
    public async Task ShouldRemoveListing()
    {
        User seller = new();
        ArrangeDb.Users.Add(seller);
        await ArrangeDb.SaveChangesAsync();

        MarketplaceListing listing = MarketplaceListingFactory.CreateListing(seller.Id, offeredGold: 25, requestedGold: 40);
        ArrangeDb.MarketplaceListings.Add(listing);
        await ArrangeDb.SaveChangesAsync();

        var loadedListing = await ActDb.MarketplaceListings
            .Include(l => l.Assets)
            .Include(l => l.Seller)
            .FirstAsync(l => l.Id == listing.Id);
        var offeredAsset = loadedListing.Assets.First(a => a.Side == MarketplaceListingAssetSide.Offered);

        _marketplaceService.RefundAndRemoveListing(ActDb, loadedListing);
        await ActDb.SaveChangesAsync();

        Assert.That(await AssertDb.MarketplaceListings.AnyAsync(l => l.Id == listing.Id), Is.False);
    }

    [Test]
    public async Task ShouldRefundBothGoldAndHeirloomPointsTogether()
    {
        User seller = new() { Gold = 200, HeirloomPoints = 2 };
        ArrangeDb.Users.Add(seller);
        await ArrangeDb.SaveChangesAsync();

        MarketplaceListing listing = MarketplaceListingFactory.CreateListing(seller.Id, goldFee: 7, offeredGold: 30, offeredHeirloomPoints: 4, requestedGold: 50);
        ArrangeDb.MarketplaceListings.Add(listing);
        await ArrangeDb.SaveChangesAsync();

        var loadedListing = await ActDb.MarketplaceListings
            .Include(l => l.Assets)
            .Include(l => l.Seller)
            .FirstAsync(l => l.Id == listing.Id);

        _marketplaceService.RefundAndRemoveListing(ActDb, loadedListing);
        await ActDb.SaveChangesAsync();

        User? dbSeller = await AssertDb.Users.FindAsync(seller.Id);
        Assert.That(dbSeller!.Gold, Is.EqualTo(237)); // 200 + 30 + 7
        Assert.That(dbSeller.HeirloomPoints, Is.EqualTo(6)); // 2 + 4
    }

    [Test]
    public async Task ShouldNotChangeGoldWhenOfferedGoldAndFeeAreZero()
    {
        User seller = new() { Gold = 50 };
        ArrangeDb.Users.Add(seller);
        await ArrangeDb.SaveChangesAsync();

        MarketplaceListing listing = MarketplaceListingFactory.CreateListing(seller.Id, goldFee: 0, offeredGold: 0, offeredHeirloomPoints: 1, requestedGold: 10);
        ArrangeDb.MarketplaceListings.Add(listing);
        await ArrangeDb.SaveChangesAsync();

        var loadedListing = await ActDb.MarketplaceListings
            .Include(l => l.Assets)
            .Include(l => l.Seller)
            .FirstAsync(l => l.Id == listing.Id);

        _marketplaceService.RefundAndRemoveListing(ActDb, loadedListing);
        await ActDb.SaveChangesAsync();

        User? dbSeller = await AssertDb.Users.FindAsync(seller.Id);
        Assert.That(dbSeller!.Gold, Is.EqualTo(50));
    }
}
