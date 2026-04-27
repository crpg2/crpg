using Crpg.Application.Common.Results;
using Crpg.Application.Marketplace.Commands;
using Crpg.Domain.Entities.Marketplace;
using Crpg.Domain.Entities.Users;
using Crpg.Sdk.Abstractions;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Marketplace.AcceptMarketplaceListing;

internal class AcceptMarketplaceListingOfferValidationTest : AcceptMarketplaceListingCommandTestBase
{
    [Test]
    public async Task ShouldReturnOfferNotFoundIfOfferDoesNotExist()
    {
        User buyer = new();
        ArrangeDb.Users.Add(buyer);
        await ArrangeDb.SaveChangesAsync();

        var result = await CreateHandler().Handle(new AcceptMarketplaceListingCommand
        {
            UserId = buyer.Id,
            ListingId = 999,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Not.Null);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.MarketplaceListingNotFound));
    }

    [Test]
    public async Task ShouldReturnSelfAcceptIfBuyerIsSeller()
    {
        User seller = new();
        ArrangeDb.Users.Add(seller);
        await ArrangeDb.SaveChangesAsync();

        var listing = MarketplaceListingFactory.CreateListing(seller.Id, offeredGold: 100, requestedGold: 50);
        ArrangeDb.MarketplaceListings.Add(listing);
        await ArrangeDb.SaveChangesAsync();

        var result = await CreateHandler().Handle(new AcceptMarketplaceListingCommand
        {
            UserId = seller.Id,
            ListingId = listing.Id,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Not.Null);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.MarketplaceListingSelfAccept));
    }

    [Test]
    public async Task ShouldReturnExpiredWhenListingExpiresAtEqualsNow()
    {
        User seller = new();
        User buyer = new();
        ArrangeDb.Users.AddRange(seller, buyer);
        await ArrangeDb.SaveChangesAsync();

        DateTime boundary = new(2024, 6, 1, 12, 0, 0, DateTimeKind.Utc);
        var listing = MarketplaceListingFactory.CreateListing(seller.Id, offeredGold: 10, expiresAt: boundary);
        ArrangeDb.MarketplaceListings.Add(listing);
        await ArrangeDb.SaveChangesAsync();

        Mock<IDateTime> dateTime = new();
        dateTime.Setup(d => d.UtcNow).Returns(boundary); // ExpiresAt <= UtcNow → expired

        var result = await CreateHandler(dateTime: dateTime.Object).Handle(new AcceptMarketplaceListingCommand
        {
            UserId = buyer.Id,
            ListingId = listing.Id,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Not.Null);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.MarketplaceListingExpired));
    }

    [Test]
    public async Task ShouldReturnInvalidAssetIfOfferedAssetIsMissing()
    {
        User seller = new();
        User buyer = new() { Gold = 100 };
        ArrangeDb.Users.AddRange(seller, buyer);
        await ArrangeDb.SaveChangesAsync();

        MarketplaceListing listing = new()
        {
            SellerId = seller.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            Assets = [new MarketplaceListingAsset { Side = MarketplaceListingAssetSide.Requested, Gold = 10 }],
        };
        ArrangeDb.MarketplaceListings.Add(listing);
        await ArrangeDb.SaveChangesAsync();

        var result = await CreateHandler().Handle(new AcceptMarketplaceListingCommand
        {
            UserId = buyer.Id,
            ListingId = listing.Id,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Not.Null);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.MarketplaceListingInvalidAsset));
    }

    [Test]
    public async Task ShouldReturnInvalidAssetIfRequestedAssetIsMissing()
    {
        User seller = new();
        User buyer = new();
        ArrangeDb.Users.AddRange(seller, buyer);
        await ArrangeDb.SaveChangesAsync();

        MarketplaceListing listing = new()
        {
            SellerId = seller.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            Assets = [new MarketplaceListingAsset { Side = MarketplaceListingAssetSide.Offered, Gold = 10 }],
        };
        ArrangeDb.MarketplaceListings.Add(listing);
        await ArrangeDb.SaveChangesAsync();

        var result = await CreateHandler().Handle(new AcceptMarketplaceListingCommand
        {
            UserId = buyer.Id,
            ListingId = listing.Id,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Not.Null);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.MarketplaceListingInvalidAsset));
    }
}
