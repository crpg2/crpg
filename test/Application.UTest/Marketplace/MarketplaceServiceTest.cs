using Crpg.Application.Marketplace.Services;
using Crpg.Domain.Entities.Marketplace;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using static Crpg.Application.UTest.Marketplace.MarketplaceOfferFactory;

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

        MarketplaceOffer offer = CreateOffer(seller.Id, goldFee: 10, offeredGold: 50, requestedGold: 80);
        ArrangeDb.MarketplaceOffers.Add(offer);
        await ArrangeDb.SaveChangesAsync();

        var loadedOffer = await ActDb.MarketplaceOffers
            .Include(o => o.Assets)
            .Include(o => o.Seller)
            .FirstAsync(o => o.Id == offer.Id);
        _marketplaceService.RefundAndRemoveOffer(ActDb, loadedOffer);
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

        MarketplaceOffer offer = CreateOffer(seller.Id, offeredHeirloomPoints: 3, requestedGold: 80);
        ArrangeDb.MarketplaceOffers.Add(offer);
        await ArrangeDb.SaveChangesAsync();

        var loadedOffer = await ActDb.MarketplaceOffers
            .Include(o => o.Assets)
            .Include(o => o.Seller)
            .FirstAsync(o => o.Id == offer.Id);
        _marketplaceService.RefundAndRemoveOffer(ActDb, loadedOffer);
        await ActDb.SaveChangesAsync();

        User? dbSeller = await AssertDb.Users.FindAsync(seller.Id);
        Assert.That(dbSeller!.HeirloomPoints, Is.EqualTo(8)); // 5 + 3
    }

    [Test]
    public async Task ShouldRemoveOffer()
    {
        User seller = new();
        ArrangeDb.Users.Add(seller);
        await ArrangeDb.SaveChangesAsync();

        MarketplaceOffer offer = CreateOffer(seller.Id, offeredGold: 25, requestedGold: 40);
        ArrangeDb.MarketplaceOffers.Add(offer);
        await ArrangeDb.SaveChangesAsync();

        var loadedOffer = await ActDb.MarketplaceOffers
            .Include(o => o.Assets)
            .Include(o => o.Seller)
            .FirstAsync(o => o.Id == offer.Id);
        var offeredAsset = loadedOffer.Assets.First(a => a.Side == MarketplaceOfferAssetSide.Offered);

        _marketplaceService.RefundAndRemoveOffer(ActDb, loadedOffer);
        await ActDb.SaveChangesAsync();

        Assert.That(await AssertDb.MarketplaceOffers.AnyAsync(o => o.Id == offer.Id), Is.False);
    }

    [Test]
    public async Task ShouldRefundBothGoldAndHeirloomPointsTogether()
    {
        User seller = new() { Gold = 200, HeirloomPoints = 2 };
        ArrangeDb.Users.Add(seller);
        await ArrangeDb.SaveChangesAsync();

        MarketplaceOffer offer = CreateOffer(seller.Id, goldFee: 7, offeredGold: 30, offeredHeirloomPoints: 4, requestedGold: 50);
        ArrangeDb.MarketplaceOffers.Add(offer);
        await ArrangeDb.SaveChangesAsync();

        var loadedOffer = await ActDb.MarketplaceOffers
            .Include(o => o.Assets)
            .Include(o => o.Seller)
            .FirstAsync(o => o.Id == offer.Id);

        _marketplaceService.RefundAndRemoveOffer(ActDb, loadedOffer);
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

        MarketplaceOffer offer = CreateOffer(seller.Id, goldFee: 0, offeredGold: 0, offeredHeirloomPoints: 1, requestedGold: 10);
        ArrangeDb.MarketplaceOffers.Add(offer);
        await ArrangeDb.SaveChangesAsync();

        var loadedOffer = await ActDb.MarketplaceOffers
            .Include(o => o.Assets)
            .Include(o => o.Seller)
            .FirstAsync(o => o.Id == offer.Id);

        _marketplaceService.RefundAndRemoveOffer(ActDb, loadedOffer);
        await ActDb.SaveChangesAsync();

        User? dbSeller = await AssertDb.Users.FindAsync(seller.Id);
        Assert.That(dbSeller!.Gold, Is.EqualTo(50));
    }
}
