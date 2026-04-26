using Crpg.Application.Common.Results;
using Crpg.Application.Marketplace.Commands;
using Crpg.Domain.Entities.Clans;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Marketplace.AcceptMarketplaceListing;

internal class AcceptMarketplaceListingBuyerValidationTest : AcceptMarketplaceListingCommandTestBase
{
    [Test]
    public async Task ShouldReturnUserNotFoundIfBuyerDoesNotExist()
    {
        User seller = new();
        ArrangeDb.Users.Add(seller);
        await ArrangeDb.SaveChangesAsync();

        var listing = MarketplaceListingFactory.CreateListing(seller.Id, offeredGold: 10);
        ArrangeDb.MarketplaceListings.Add(listing);
        await ArrangeDb.SaveChangesAsync();

        var result = await CreateHandler().Handle(new AcceptMarketplaceListingCommand
        {
            UserId = 9999,
            ListingId = listing.Id,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Not.Null);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.UserNotFound));
    }

    [Test]
    public async Task ShouldReturnNotEnoughGoldIfBuyerGoldIsOneLessThanRequired()
    {
        User seller = new();
        User buyer = new() { Gold = 99 };
        ArrangeDb.Users.AddRange(seller, buyer);
        await ArrangeDb.SaveChangesAsync();

        var listing = MarketplaceListingFactory.CreateListing(seller.Id, offeredGold: 10, requestedGold: 100);
        ArrangeDb.MarketplaceListings.Add(listing);
        await ArrangeDb.SaveChangesAsync();

        var result = await CreateHandler().Handle(new AcceptMarketplaceListingCommand
        {
            UserId = buyer.Id,
            ListingId = listing.Id,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Not.Null);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.NotEnoughGold));
    }

    [Test]
    public async Task ShouldReturnNotEnoughHeirloomPointsIfBuyerHeirloomPointsAreLessThanRequired()
    {
        User seller = new();
        User buyer = new() { Gold = 1000, HeirloomPoints = 2 };
        ArrangeDb.Users.AddRange(seller, buyer);
        await ArrangeDb.SaveChangesAsync();

        var listing = MarketplaceListingFactory.CreateListing(seller.Id, requestedHeirloomPoints: 3);
        ArrangeDb.MarketplaceListings.Add(listing);
        await ArrangeDb.SaveChangesAsync();

        var result = await CreateHandler().Handle(new AcceptMarketplaceListingCommand
        {
            UserId = buyer.Id,
            ListingId = listing.Id,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Not.Null);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.NotEnoughHeirloomPoints));
    }

    [Test]
    public async Task ShouldReturnInvalidAssetIfBuyerDoesNotOwnRequestedItem()
    {
        User seller = new();
        User buyer = new() { Gold = 100 };
        ArrangeDb.Users.AddRange(seller, buyer);
        ArrangeDb.Items.Add(new Item { Id = "shield" });
        await ArrangeDb.SaveChangesAsync();

        // buyer owns no "shield"
        var listing = MarketplaceListingFactory.CreateListing(seller.Id, offeredGold: 10, requestedItemId: "shield");
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
    public async Task ShouldReturnInvalidAssetIfRequestedItemIsBroken()
    {
        User seller = new();
        User buyer = new();
        ArrangeDb.Users.AddRange(seller, buyer);
        ArrangeDb.Items.Add(new Item { Id = "shield" });
        await ArrangeDb.SaveChangesAsync();

        UserItem buyerItem = new() { UserId = buyer.Id, ItemId = "shield", IsBroken = true };
        ArrangeDb.UserItems.Add(buyerItem);
        await ArrangeDb.SaveChangesAsync();

        var listing = MarketplaceListingFactory.CreateListing(seller.Id, offeredGold: 10, requestedItemId: "shield");
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
    public async Task ShouldReturnInvalidAssetIfRequestedItemIsPersonal()
    {
        User seller = new();
        User buyer = new();
        ArrangeDb.Users.AddRange(seller, buyer);
        ArrangeDb.Items.Add(new Item { Id = "shield" });
        await ArrangeDb.SaveChangesAsync();

        UserItem buyerItem = new() { UserId = buyer.Id, ItemId = "shield" };
        ArrangeDb.UserItems.Add(buyerItem);
        await ArrangeDb.SaveChangesAsync();

        ArrangeDb.PersonalItems.Add(new PersonalItem { UserItemId = buyerItem.Id });
        await ArrangeDb.SaveChangesAsync();

        var listing = MarketplaceListingFactory.CreateListing(seller.Id, offeredGold: 10, requestedItemId: "shield");
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
    public async Task ShouldReturnInvalidAssetIfRequestedItemIsInClanArmory()
    {
        User seller = new();
        User buyer = new();
        ArrangeDb.Users.AddRange(seller, buyer);
        ArrangeDb.Items.Add(new Item { Id = "shield" });
        await ArrangeDb.SaveChangesAsync();

        UserItem buyerItem = new() { UserId = buyer.Id, ItemId = "shield" };
        ArrangeDb.UserItems.Add(buyerItem);
        await ArrangeDb.SaveChangesAsync();

        ArrangeDb.ClanArmoryItems.Add(new ClanArmoryItem { UserItemId = buyerItem.Id, LenderClanId = 1, LenderUserId = buyer.Id });
        await ArrangeDb.SaveChangesAsync();

        var listing = MarketplaceListingFactory.CreateListing(seller.Id, offeredGold: 10, requestedItemId: "shield");
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
    public async Task ShouldReturnInvalidAssetIfRequestedItemIsBorrowedFromClanArmory()
    {
        User seller = new();
        User buyer = new();
        ArrangeDb.Users.AddRange(seller, buyer);
        ArrangeDb.Items.Add(new Item { Id = "shield" });
        await ArrangeDb.SaveChangesAsync();

        UserItem buyerItem = new() { UserId = buyer.Id, ItemId = "shield" };
        ArrangeDb.UserItems.Add(buyerItem);
        await ArrangeDb.SaveChangesAsync();

        ArrangeDb.ClanArmoryBorrowedItems.Add(new ClanArmoryBorrowedItem
        {
            UserItemId = buyerItem.Id,
            BorrowerUserId = buyer.Id,
            BorrowerClanId = 1,
        });
        await ArrangeDb.SaveChangesAsync();

        var listing = MarketplaceListingFactory.CreateListing(seller.Id, offeredGold: 10, requestedItemId: "shield");
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
