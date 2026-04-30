using Crpg.Application.Marketplace.Commands;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Crpg.Application.UTest.Marketplace.AcceptMarketplaceListing;

internal class AcceptMarketplaceListingTransferTest : AcceptMarketplaceListingCommandTestBase
{
    [Test]
    public async Task ShouldTransferGoldCorrectlyOnPureGoldTrade()
    {
        // Seller offers 50 gold, asks for 100 gold.
        User seller = new() { Gold = 0 };
        User buyer = new() { Gold = 100 };
        ArrangeDb.Users.AddRange(seller, buyer);
        await ArrangeDb.SaveChangesAsync();

        var listing = MarketplaceListingFactory.CreateListing(seller.Id, offeredGold: 50, requestedGold: 100);
        ArrangeDb.MarketplaceListings.Add(listing);
        await ArrangeDb.SaveChangesAsync();

        var result = await CreateHandler().Handle(new AcceptMarketplaceListingCommand
        {
            UserId = buyer.Id,
            ListingId = listing.Id,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Null);

        User? dbSeller = await AssertDb.Users.FindAsync(seller.Id);
        User? dbBuyer = await AssertDb.Users.FindAsync(buyer.Id);
        Assert.That(dbSeller!.Gold, Is.EqualTo(100)); // received 100, had 0
        Assert.That(dbBuyer!.Gold, Is.EqualTo(50));   // paid 100, received 50
    }

    [Test]
    public async Task ShouldSucceedWhenBuyerHasExactlyEnoughGold()
    {
        User seller = new() { Gold = 0 };
        User buyer = new() { Gold = 100 };
        ArrangeDb.Users.AddRange(seller, buyer);
        await ArrangeDb.SaveChangesAsync();

        var listing = MarketplaceListingFactory.CreateListing(seller.Id, requestedGold: 100);
        ArrangeDb.MarketplaceListings.Add(listing);
        await ArrangeDb.SaveChangesAsync();

        var result = await CreateHandler().Handle(new AcceptMarketplaceListingCommand
        {
            UserId = buyer.Id,
            ListingId = listing.Id,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Null);
    }

    [Test]
    public async Task ShouldTransferHeirloomPointsCorrectlyOnPureHeirloomTrade()
    {
        // Seller offers 2 HP, asks for 5 HP.
        User seller = new() { HeirloomPoints = 0 };
        User buyer = new() { HeirloomPoints = 5 };
        ArrangeDb.Users.AddRange(seller, buyer);
        await ArrangeDb.SaveChangesAsync();

        var listing = MarketplaceListingFactory.CreateListing(seller.Id, offeredHeirloomPoints: 2, requestedHeirloomPoints: 5);
        ArrangeDb.MarketplaceListings.Add(listing);
        await ArrangeDb.SaveChangesAsync();

        var result = await CreateHandler().Handle(new AcceptMarketplaceListingCommand
        {
            UserId = buyer.Id,
            ListingId = listing.Id,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Null);

        User? dbSeller = await AssertDb.Users.FindAsync(seller.Id);
        User? dbBuyer = await AssertDb.Users.FindAsync(buyer.Id);
        Assert.That(dbSeller!.HeirloomPoints, Is.EqualTo(5));
        Assert.That(dbBuyer!.HeirloomPoints, Is.EqualTo(2));
    }

    [Test]
    public async Task ShouldRemoveListingAfterAccept()
    {
        User seller = new();
        User buyer = new() { Gold = 50 };
        ArrangeDb.Users.AddRange(seller, buyer);
        await ArrangeDb.SaveChangesAsync();

        var listing = MarketplaceListingFactory.CreateListing(seller.Id, offeredGold: 10, requestedGold: 50);
        ArrangeDb.MarketplaceListings.Add(listing);
        await ArrangeDb.SaveChangesAsync();

        await CreateHandler().Handle(new AcceptMarketplaceListingCommand
        {
            UserId = buyer.Id,
            ListingId = listing.Id,
        }, CancellationToken.None);

        Assert.That(await AssertDb.MarketplaceListings.AnyAsync(l => l.Id == listing.Id), Is.False);
    }

    [Test]
    public async Task ShouldTransferOfferedItemToBuyerInItemForGoldTrade()
    {
        User seller = new();
        User buyer = new() { Gold = 200 };
        ArrangeDb.Users.AddRange(seller, buyer);
        ArrangeDb.Items.Add(new Item { Id = "sword" });
        await ArrangeDb.SaveChangesAsync();

        UserItem sellerItem = new() { UserId = seller.Id, ItemId = "sword" };
        ArrangeDb.UserItems.Add(sellerItem);
        await ArrangeDb.SaveChangesAsync();

        var listing = MarketplaceListingFactory.CreateListing(seller.Id, offeredUserItemId: sellerItem.Id, requestedGold: 200);
        ArrangeDb.MarketplaceListings.Add(listing);
        await ArrangeDb.SaveChangesAsync();

        var result = await CreateHandler().Handle(new AcceptMarketplaceListingCommand
        {
            UserId = buyer.Id,
            ListingId = listing.Id,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Null);

        UserItem? dbItem = await AssertDb.UserItems.FindAsync(sellerItem.Id);
        Assert.That(dbItem!.UserId, Is.EqualTo(buyer.Id));

        User? dbSeller = await AssertDb.Users.FindAsync(seller.Id);
        User? dbBuyer = await AssertDb.Users.FindAsync(buyer.Id);
        Assert.That(dbSeller!.Gold, Is.EqualTo(200));
        Assert.That(dbBuyer!.Gold, Is.EqualTo(0));
    }

    [Test]
    public async Task ShouldTransferRequestedItemToSellerInGoldForItemTrade()
    {
        // Seller offers 100 gold in return for buyer's shield.
        User seller = new() { Gold = 0 };
        User buyer = new() { Gold = 0 };
        ArrangeDb.Users.AddRange(seller, buyer);
        ArrangeDb.Items.Add(new Item { Id = "shield" });
        await ArrangeDb.SaveChangesAsync();

        UserItem buyerItem = new() { UserId = buyer.Id, ItemId = "shield" };
        ArrangeDb.UserItems.Add(buyerItem);
        await ArrangeDb.SaveChangesAsync();

        var listing = MarketplaceListingFactory.CreateListing(seller.Id, offeredGold: 100, requestedItemId: "shield");
        ArrangeDb.MarketplaceListings.Add(listing);
        await ArrangeDb.SaveChangesAsync();

        var result = await CreateHandler().Handle(new AcceptMarketplaceListingCommand
        {
            UserId = buyer.Id,
            ListingId = listing.Id,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Null);

        UserItem? dbItem = await AssertDb.UserItems.FindAsync(buyerItem.Id);
        Assert.That(dbItem!.UserId, Is.EqualTo(seller.Id));

        User? dbBuyer = await AssertDb.Users.FindAsync(buyer.Id);
        Assert.That(dbBuyer!.Gold, Is.EqualTo(100)); // received the locked gold
    }

    [Test]
    public async Task ShouldSwapBothItemsInItemForItemTrade()
    {
        User seller = new();
        User buyer = new();
        ArrangeDb.Users.AddRange(seller, buyer);
        ArrangeDb.Items.AddRange(new Item { Id = "sword" }, new Item { Id = "shield" });
        await ArrangeDb.SaveChangesAsync();

        UserItem sellerItem = new() { UserId = seller.Id, ItemId = "sword" };
        UserItem buyerItem = new() { UserId = buyer.Id, ItemId = "shield" };
        ArrangeDb.UserItems.AddRange(sellerItem, buyerItem);
        await ArrangeDb.SaveChangesAsync();

        var listing = MarketplaceListingFactory.CreateListing(seller.Id, offeredUserItemId: sellerItem.Id, requestedItemId: "shield");
        ArrangeDb.MarketplaceListings.Add(listing);
        await ArrangeDb.SaveChangesAsync();

        var result = await CreateHandler().Handle(new AcceptMarketplaceListingCommand
        {
            UserId = buyer.Id,
            ListingId = listing.Id,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Null);

        UserItem? dbSellerItem = await AssertDb.UserItems.FindAsync(sellerItem.Id);
        UserItem? dbBuyerItem = await AssertDb.UserItems.FindAsync(buyerItem.Id);
        Assert.That(dbSellerItem!.UserId, Is.EqualTo(buyer.Id));
        Assert.That(dbBuyerItem!.UserId, Is.EqualTo(seller.Id));
    }

    [Test]
    public async Task ShouldRemoveEquipmentRecordsFromOfferedItemOnTransfer()
    {
        User seller = new();
        User buyer = new();
        ArrangeDb.Users.AddRange(seller, buyer);
        ArrangeDb.Items.Add(new Item { Id = "sword" });
        await ArrangeDb.SaveChangesAsync();

        UserItem sellerItem = new() { UserId = seller.Id, ItemId = "sword" };
        ArrangeDb.UserItems.Add(sellerItem);
        await ArrangeDb.SaveChangesAsync();

        // Seller had the item equipped
        ArrangeDb.EquippedItems.Add(new EquippedItem { UserItemId = sellerItem.Id, CharacterId = 1, Slot = ItemSlot.Weapon0 });
        await ArrangeDb.SaveChangesAsync();

        var listing = MarketplaceListingFactory.CreateListing(seller.Id, offeredUserItemId: sellerItem.Id);
        ArrangeDb.MarketplaceListings.Add(listing);
        await ArrangeDb.SaveChangesAsync();

        var result = await CreateHandler().Handle(new AcceptMarketplaceListingCommand
        {
            UserId = buyer.Id,
            ListingId = listing.Id,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Null);
        Assert.That(await AssertDb.EquippedItems.AnyAsync(e => e.UserItemId == sellerItem.Id), Is.False);
    }

    [Test]
    public async Task ShouldRemoveEquipmentRecordsFromRequestedItemOnTransfer()
    {
        User seller = new();
        User buyer = new();
        ArrangeDb.Users.AddRange(seller, buyer);
        ArrangeDb.Items.Add(new Item { Id = "shield" });
        await ArrangeDb.SaveChangesAsync();

        UserItem buyerItem = new() { UserId = buyer.Id, ItemId = "shield" };
        ArrangeDb.UserItems.Add(buyerItem);
        await ArrangeDb.SaveChangesAsync();

        // Buyer had the item equipped
        ArrangeDb.EquippedItems.Add(new EquippedItem { UserItemId = buyerItem.Id, CharacterId = 1, Slot = ItemSlot.Head });
        await ArrangeDb.SaveChangesAsync();

        var listing = MarketplaceListingFactory.CreateListing(seller.Id, offeredGold: 10, requestedItemId: "shield");
        ArrangeDb.MarketplaceListings.Add(listing);
        await ArrangeDb.SaveChangesAsync();

        var result = await CreateHandler().Handle(new AcceptMarketplaceListingCommand
        {
            UserId = buyer.Id,
            ListingId = listing.Id,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Null);
        Assert.That(await AssertDb.EquippedItems.AnyAsync(e => e.UserItemId == buyerItem.Id), Is.False);
    }
}
