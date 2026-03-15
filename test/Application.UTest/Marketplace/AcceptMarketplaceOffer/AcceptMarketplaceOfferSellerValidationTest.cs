using Crpg.Application.Common.Results;
using Crpg.Application.Marketplace.Commands;
using Crpg.Domain.Entities.Clans;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Marketplace;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;
using static Crpg.Application.UTest.Marketplace.MarketplaceOfferFactory;

namespace Crpg.Application.UTest.Marketplace.AcceptMarketplaceOffer;

internal class AcceptMarketplaceOfferSellerValidationTest : AcceptMarketplaceOfferCommandTestBase
{
    [Test]
    public async Task ShouldReturnUserNotFoundIfSellerDoesNotExist()
    {
        User buyer = new();
        ArrangeDb.Users.Add(buyer);
        await ArrangeDb.SaveChangesAsync();

        // InMemory DB does not enforce FK — offer with a non-existent SellerId is queryable.
        MarketplaceOffer offer = new()
        {
            SellerId = 9999,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            Assets =
            [
                new MarketplaceOfferAsset { Side = MarketplaceOfferAssetSide.Offered, Gold = 10 },
                new MarketplaceOfferAsset { Side = MarketplaceOfferAssetSide.Requested, Gold = 5 },
            ],
        };
        ArrangeDb.MarketplaceOffers.Add(offer);
        await ArrangeDb.SaveChangesAsync();

        var result = await CreateHandler().Handle(new AcceptMarketplaceOfferCommand
        {
            UserId = buyer.Id,
            OfferId = offer.Id,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Not.Null);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.UserNotFound));
    }

    [Test]
    public async Task ShouldReturnInvalidAssetIfOfferedItemNotInSellerInventory()
    {
        User seller = new();
        User buyer = new();
        User thirdParty = new();
        ArrangeDb.Users.AddRange(seller, buyer, thirdParty);
        ArrangeDb.Items.Add(new Item { Id = "item_a" });
        await ArrangeDb.SaveChangesAsync();

        UserItem foreignItem = new() { UserId = thirdParty.Id, ItemId = "item_a" };
        ArrangeDb.UserItems.Add(foreignItem);
        await ArrangeDb.SaveChangesAsync();

        var offer = CreateOffer(seller.Id, offeredUserItemId: foreignItem.Id);
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
    public async Task ShouldReturnInvalidAssetIfOfferedItemIsBroken()
    {
        User seller = new();
        User buyer = new();
        ArrangeDb.Users.AddRange(seller, buyer);
        ArrangeDb.Items.Add(new Item { Id = "sword" });
        await ArrangeDb.SaveChangesAsync();

        UserItem sellerItem = new() { UserId = seller.Id, ItemId = "sword", IsBroken = true };
        ArrangeDb.UserItems.Add(sellerItem);
        await ArrangeDb.SaveChangesAsync();

        var offer = CreateOffer(seller.Id, offeredUserItemId: sellerItem.Id);
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
    public async Task ShouldReturnInvalidAssetIfOfferedItemIsPersonal()
    {
        User seller = new();
        User buyer = new();
        ArrangeDb.Users.AddRange(seller, buyer);
        ArrangeDb.Items.Add(new Item { Id = "sword" });
        await ArrangeDb.SaveChangesAsync();

        UserItem sellerItem = new() { UserId = seller.Id, ItemId = "sword" };
        ArrangeDb.UserItems.Add(sellerItem);
        await ArrangeDb.SaveChangesAsync();

        ArrangeDb.PersonalItems.Add(new PersonalItem { UserItemId = sellerItem.Id });
        await ArrangeDb.SaveChangesAsync();

        var offer = CreateOffer(seller.Id, offeredUserItemId: sellerItem.Id);
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
    public async Task ShouldReturnInvalidAssetIfOfferedItemIsInClanArmory()
    {
        User seller = new();
        User buyer = new();
        ArrangeDb.Users.AddRange(seller, buyer);
        ArrangeDb.Items.Add(new Item { Id = "sword" });
        await ArrangeDb.SaveChangesAsync();

        UserItem sellerItem = new() { UserId = seller.Id, ItemId = "sword" };
        ArrangeDb.UserItems.Add(sellerItem);
        await ArrangeDb.SaveChangesAsync();

        ArrangeDb.ClanArmoryItems.Add(new ClanArmoryItem { UserItemId = sellerItem.Id, LenderClanId = 1, LenderUserId = seller.Id });
        await ArrangeDb.SaveChangesAsync();

        var offer = CreateOffer(seller.Id, offeredUserItemId: sellerItem.Id);
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
    public async Task ShouldReturnInvalidAssetIfOfferedItemIsBorrowedFromClanArmory()
    {
        User seller = new();
        User buyer = new();
        ArrangeDb.Users.AddRange(seller, buyer);
        ArrangeDb.Items.Add(new Item { Id = "sword" });
        await ArrangeDb.SaveChangesAsync();

        UserItem sellerItem = new() { UserId = seller.Id, ItemId = "sword" };
        ArrangeDb.UserItems.Add(sellerItem);
        await ArrangeDb.SaveChangesAsync();

        ArrangeDb.ClanArmoryBorrowedItems.Add(new ClanArmoryBorrowedItem
        {
            UserItemId = sellerItem.Id,
            BorrowerUserId = seller.Id,
            BorrowerClanId = 1,
        });
        await ArrangeDb.SaveChangesAsync();

        var offer = CreateOffer(seller.Id, offeredUserItemId: sellerItem.Id);
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
