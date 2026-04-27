using Crpg.Application.Marketplace.Queries;
using Crpg.Domain.Entities.ActivityLogs;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Marketplace;

public class GetMarketplaceListingsHistoryQueryTest : TestBase
{
    [Test]
    public async Task ShouldReturnAllLogsWhenNoFilter()
    {
        User user1 = new();
        User user2 = new();
        ArrangeDb.Users.AddRange(user1, user2);
        await ArrangeDb.SaveChangesAsync();

        ArrangeDb.ActivityLogs.AddRange(
            CreateAcceptedLog(buyerId: user1.Id, sellerId: user2.Id, listingId: 1),
            CreateAcceptedLog(buyerId: user2.Id, sellerId: user1.Id, listingId: 2));
        await ArrangeDb.SaveChangesAsync();

        var res = await new GetMarketplaceListingsHistoryQuery.Handler(ActDb, Mapper).Handle(
            new GetMarketplaceListingsHistoryQuery(), CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        Assert.That(res.Data!.TotalCount, Is.EqualTo(2));
        Assert.That(res.Data.Items.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task ShouldFilterByBuyerId()
    {
        User buyer = new();
        User seller = new();
        User other = new();
        ArrangeDb.Users.AddRange(buyer, seller, other);
        await ArrangeDb.SaveChangesAsync();

        ArrangeDb.ActivityLogs.AddRange(
            CreateAcceptedLog(buyerId: buyer.Id, sellerId: seller.Id, listingId: 1),
            CreateAcceptedLog(buyerId: other.Id, sellerId: seller.Id, listingId: 2));
        await ArrangeDb.SaveChangesAsync();

        var res = await new GetMarketplaceListingsHistoryQuery.Handler(ActDb, Mapper).Handle(
            new GetMarketplaceListingsHistoryQuery { BuyerId = buyer.Id }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        Assert.That(res.Data!.TotalCount, Is.EqualTo(1));
        Assert.That(res.Data.Items[0].Buyer.Id, Is.EqualTo(buyer.Id));
    }

    [Test]
    public async Task ShouldFilterBySellerId()
    {
        User buyer = new();
        User seller = new();
        User other = new();
        ArrangeDb.Users.AddRange(buyer, seller, other);
        await ArrangeDb.SaveChangesAsync();

        ArrangeDb.ActivityLogs.AddRange(
            CreateAcceptedLog(buyerId: buyer.Id, sellerId: seller.Id, listingId: 1),
            CreateAcceptedLog(buyerId: buyer.Id, sellerId: other.Id, listingId: 2));
        await ArrangeDb.SaveChangesAsync();

        var res = await new GetMarketplaceListingsHistoryQuery.Handler(ActDb, Mapper).Handle(
            new GetMarketplaceListingsHistoryQuery { SellerId = seller.Id }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        Assert.That(res.Data!.TotalCount, Is.EqualTo(1));
        Assert.That(res.Data.Items[0].Seller.Id, Is.EqualTo(seller.Id));
    }

    [Test]
    public async Task ShouldFilterByBothBuyerAndSellerWhenDifferent()
    {
        User alice = new();
        User bob = new();
        User charlie = new();
        ArrangeDb.Users.AddRange(alice, bob, charlie);
        await ArrangeDb.SaveChangesAsync();

        // alice bought from bob
        // alice bought from charlie
        // charlie bought from bob
        ArrangeDb.ActivityLogs.AddRange(
            CreateAcceptedLog(buyerId: alice.Id, sellerId: bob.Id, listingId: 1),
            CreateAcceptedLog(buyerId: alice.Id, sellerId: charlie.Id, listingId: 2),
            CreateAcceptedLog(buyerId: charlie.Id, sellerId: bob.Id, listingId: 3));
        await ArrangeDb.SaveChangesAsync();

        var res = await new GetMarketplaceListingsHistoryQuery.Handler(ActDb, Mapper).Handle(
            new GetMarketplaceListingsHistoryQuery { BuyerId = alice.Id, SellerId = bob.Id }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        Assert.That(res.Data!.TotalCount, Is.EqualTo(1));
        Assert.That(res.Data.Items[0].Buyer.Id, Is.EqualTo(alice.Id));
        Assert.That(res.Data.Items[0].Seller.Id, Is.EqualTo(bob.Id));
    }

    [Test]
    public async Task ShouldReturnAllTradesOfUserWhenBuyerIdEqualsSellerId()
    {
        User self = new();
        User other = new();
        ArrangeDb.Users.AddRange(self, other);
        await ArrangeDb.SaveChangesAsync();

        // self sold to other (self is seller)
        // other sold to self (self is buyer)
        // other sold to other (self not involved)
        ArrangeDb.ActivityLogs.AddRange(
            CreateAcceptedLog(buyerId: other.Id, sellerId: self.Id, listingId: 1),
            CreateAcceptedLog(buyerId: self.Id, sellerId: other.Id, listingId: 2),
            CreateAcceptedLog(buyerId: other.Id, sellerId: other.Id, listingId: 3));
        await ArrangeDb.SaveChangesAsync();

        var res = await new GetMarketplaceListingsHistoryQuery.Handler(ActDb, Mapper).Handle(
            new GetMarketplaceListingsHistoryQuery { BuyerId = self.Id, SellerId = self.Id }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        Assert.That(res.Data!.TotalCount, Is.EqualTo(2));
        Assert.That(res.Data.Items.Any(i => i.Buyer.Id == self.Id), Is.True);
        Assert.That(res.Data.Items.Any(i => i.Seller.Id == self.Id), Is.True);
    }

    [Test]
    public async Task ShouldIgnoreNonMarketplaceListingAcceptedLogs()
    {
        User user = new();
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        ArrangeDb.ActivityLogs.AddRange(
            CreateAcceptedLog(buyerId: user.Id, sellerId: user.Id, listingId: 1),
            new ActivityLog
            {
                Type = ActivityLogType.UserCreated,
                UserId = user.Id,
                Metadata = [],
            });
        await ArrangeDb.SaveChangesAsync();

        var res = await new GetMarketplaceListingsHistoryQuery.Handler(ActDb, Mapper).Handle(
            new GetMarketplaceListingsHistoryQuery(), CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        Assert.That(res.Data!.TotalCount, Is.EqualTo(1));
    }

    [Test]
    public async Task ShouldPaginateResults()
    {
        User buyer = new();
        User seller = new();
        ArrangeDb.Users.AddRange(buyer, seller);
        await ArrangeDb.SaveChangesAsync();

        for (int i = 0; i < 5; i++)
        {
            ArrangeDb.ActivityLogs.Add(CreateAcceptedLog(buyerId: buyer.Id, sellerId: seller.Id, listingId: i + 1));
        }

        await ArrangeDb.SaveChangesAsync();

        var res = await new GetMarketplaceListingsHistoryQuery.Handler(ActDb, Mapper).Handle(
            new GetMarketplaceListingsHistoryQuery { Page = 2, PageSize = 2 }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        Assert.That(res.Data!.TotalCount, Is.EqualTo(5));
        Assert.That(res.Data.Items.Count, Is.EqualTo(2));
    }

    private static ActivityLog CreateAcceptedLog(int buyerId, int sellerId, int listingId = 1)
    {
        return new ActivityLog
        {
            Type = ActivityLogType.MarketplaceListingAccepted,
            UserId = buyerId,
            Metadata =
            [
                new ActivityLogMetadata("listingId", listingId.ToString()),
                new ActivityLogMetadata("sellerId", sellerId.ToString()),
                new ActivityLogMetadata("goldFee", "0"),
                new ActivityLogMetadata("offer", "{\"gold\":0,\"heirloomPoints\":0}"),
                new ActivityLogMetadata("request", "{\"gold\":0,\"heirloomPoints\":0}"),
            ],
        };
    }
}
