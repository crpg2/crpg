using Crpg.Application.Common.Results;
using Crpg.Application.Marketplace.Commands;
using Crpg.Domain.Entities.Marketplace;
using Crpg.Domain.Entities.Users;
using Crpg.Sdk.Abstractions;
using Moq;
using NUnit.Framework;
using static Crpg.Application.UTest.Marketplace.MarketplaceOfferFactory;

namespace Crpg.Application.UTest.Marketplace.AcceptMarketplaceOffer;

internal class AcceptMarketplaceOfferOfferValidationTest : AcceptMarketplaceOfferCommandTestBase
{
    [Test]
    public async Task ShouldReturnOfferNotFoundIfOfferDoesNotExist()
    {
        User buyer = new();
        ArrangeDb.Users.Add(buyer);
        await ArrangeDb.SaveChangesAsync();

        var result = await CreateHandler().Handle(new AcceptMarketplaceOfferCommand
        {
            UserId = buyer.Id,
            OfferId = 999,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Not.Null);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.MarketplaceOfferNotFound));
    }

    [Test]
    public async Task ShouldReturnSelfAcceptIfBuyerIsSeller()
    {
        User seller = new();
        ArrangeDb.Users.Add(seller);
        await ArrangeDb.SaveChangesAsync();

        var offer = CreateOffer(seller.Id, offeredGold: 100, requestedGold: 50);
        ArrangeDb.MarketplaceOffers.Add(offer);
        await ArrangeDb.SaveChangesAsync();

        var result = await CreateHandler().Handle(new AcceptMarketplaceOfferCommand
        {
            UserId = seller.Id,
            OfferId = offer.Id,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Not.Null);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.MarketplaceOfferSelfAccept));
    }

    [Test]
    public async Task ShouldReturnExpiredWhenOfferExpiresAtEqualsNow()
    {
        User seller = new();
        User buyer = new();
        ArrangeDb.Users.AddRange(seller, buyer);
        await ArrangeDb.SaveChangesAsync();

        DateTime boundary = new(2024, 6, 1, 12, 0, 0, DateTimeKind.Utc);
        var offer = CreateOffer(seller.Id, offeredGold: 10, expiresAt: boundary);
        ArrangeDb.MarketplaceOffers.Add(offer);
        await ArrangeDb.SaveChangesAsync();

        Mock<IDateTime> dateTime = new();
        dateTime.Setup(d => d.UtcNow).Returns(boundary); // ExpiresAt <= UtcNow → expired

        var result = await CreateHandler(dateTime: dateTime.Object).Handle(new AcceptMarketplaceOfferCommand
        {
            UserId = buyer.Id,
            OfferId = offer.Id,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Not.Null);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.MarketplaceOfferExpired));
    }

    [Test]
    public async Task ShouldReturnInvalidAssetIfOfferedAssetIsMissing()
    {
        User seller = new();
        User buyer = new();
        ArrangeDb.Users.AddRange(seller, buyer);
        await ArrangeDb.SaveChangesAsync();

        MarketplaceOffer offer = new()
        {
            SellerId = seller.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            Assets = [new MarketplaceOfferAsset { Side = MarketplaceOfferAssetSide.Requested, Gold = 10 }],
        };
        ArrangeDb.MarketplaceOffers.Add(offer);
        await ArrangeDb.SaveChangesAsync();

        var result = await CreateHandler().Handle(new AcceptMarketplaceOfferCommand
        {
            UserId = buyer.Id,
            OfferId = offer.Id,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Not.Null);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.MarketplaceOfferInvalidAsset));
    }

    [Test]
    public async Task ShouldReturnInvalidAssetIfRequestedAssetIsMissing()
    {
        User seller = new();
        User buyer = new();
        ArrangeDb.Users.AddRange(seller, buyer);
        await ArrangeDb.SaveChangesAsync();

        MarketplaceOffer offer = new()
        {
            SellerId = seller.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            Assets = [new MarketplaceOfferAsset { Side = MarketplaceOfferAssetSide.Offered, Gold = 10 }],
        };
        ArrangeDb.MarketplaceOffers.Add(offer);
        await ArrangeDb.SaveChangesAsync();

        var result = await CreateHandler().Handle(new AcceptMarketplaceOfferCommand
        {
            UserId = buyer.Id,
            OfferId = offer.Id,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Not.Null);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.MarketplaceOfferInvalidAsset));
    }
}
