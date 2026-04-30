using Crpg.Application.Marketplace.Queries;
using Crpg.Domain.Entities.Clans;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Users;
using Crpg.Sdk.Abstractions;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Marketplace;

public class GetMarketplaceListingsQueryTest : TestBase
{
    [Test]
    public async Task ShouldReturnOnlySellerListingsInSellerMode()
    {
        User user1 = new();
        User user2 = new();
        ArrangeDb.Users.AddRange(user1, user2);
        await ArrangeDb.SaveChangesAsync();

        ArrangeDb.MarketplaceListings.AddRange(
            MarketplaceListingFactory.CreateListing(user1.Id),
            MarketplaceListingFactory.CreateListing(user1.Id),
            MarketplaceListingFactory.CreateListing(user2.Id));
        await ArrangeDb.SaveChangesAsync();

        var res = await new GetMarketplaceListingsQuery.Handler(ActDb, Mapper, Mock.Of<IDateTime>()).Handle(new GetMarketplaceListingsQuery
        {
            SellerId = user1.Id,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        Assert.That(res.Data, Is.Not.Null);
        Assert.That(res.Data!.TotalCount, Is.EqualTo(2));
        Assert.That(res.Data.Items.Count, Is.EqualTo(2));
        Assert.That(res.Data.Items.All(o => o.Seller.Id == user1.Id), Is.True);
    }

    [Test]
    public async Task ShouldIgnoreOtherFiltersInSellerMode()
    {
        User user1 = new();
        User user2 = new();
        User buyer = new();
        ArrangeDb.Users.AddRange(user1, user2, buyer);
        await ArrangeDb.SaveChangesAsync();

        ArrangeDb.MarketplaceListings.AddRange(
            MarketplaceListingFactory.CreateListing(user1.Id, offeredGold: 0, requestedGold: 9999),
            MarketplaceListingFactory.CreateListing(user1.Id, offeredGold: 150, requestedGold: 0),
            MarketplaceListingFactory.CreateListing(user2.Id));
        await ArrangeDb.SaveChangesAsync();

        var res = await new GetMarketplaceListingsQuery.Handler(ActDb, Mapper, Mock.Of<IDateTime>()).Handle(new GetMarketplaceListingsQuery
        {
            SellerId = user1.Id,
            BuyerId = buyer.Id,
            Offered = new GetMarketplaceListingsQuery.SideFilter
            {
                ItemId = "non-existing-item",
                Gold = new MarketplaceCurrencyFilter { Mode = MarketplaceCurrencyFilterMode.None },
            },
            Requested = new GetMarketplaceListingsQuery.SideFilter
            {
                Gold = new MarketplaceCurrencyFilter { Mode = MarketplaceCurrencyFilterMode.CustomRange, Range = (1, 1) },
                HeirloomPoints = new MarketplaceCurrencyFilter { Mode = MarketplaceCurrencyFilterMode.CustomRange, Range = (5, 5) },
            },
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        Assert.That(res.Data, Is.Not.Null);
        Assert.That(res.Data!.TotalCount, Is.EqualTo(2));
        Assert.That(res.Data.Items.Count, Is.EqualTo(2));
        Assert.That(res.Data.Items.All(o => o.Seller.Id == user1.Id), Is.True);
    }

    [Test]
    public async Task ShouldOrderByCreatedAtDescending()
    {
        DateTime now = DateTime.UtcNow;
        User user1 = new();
        User user2 = new();
        User user3 = new();
        ArrangeDb.Users.AddRange(user1, user2, user3);
        await ArrangeDb.SaveChangesAsync();

        ArrangeDb.MarketplaceListings.AddRange(
            MarketplaceListingFactory.CreateListing(user1.Id, createdAt: now.AddMinutes(-3)),
            MarketplaceListingFactory.CreateListing(user2.Id, createdAt: now.AddMinutes(-1)),
            MarketplaceListingFactory.CreateListing(user3.Id, createdAt: now.AddMinutes(-2)));
        await ArrangeDb.SaveChangesAsync();

        var res = await new GetMarketplaceListingsQuery.Handler(ActDb, Mapper, Mock.Of<IDateTime>()).Handle(new GetMarketplaceListingsQuery
        {
            Page = 1,
            PageSize = 15,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        Assert.That(res.Data, Is.Not.Null);
        Assert.That(res.Data!.TotalCount, Is.EqualTo(3));
        Assert.That(res.Data.Items.Select(o => o.Seller.Id), Is.EqualTo([user2.Id, user3.Id, user1.Id]));
    }

    [Test]
    public async Task ShouldClampPageToOneWhenPageLessThanOne()
    {
        User user1 = new();
        User user2 = new();
        User user3 = new();
        ArrangeDb.Users.AddRange(user1, user2, user3);
        await ArrangeDb.SaveChangesAsync();

        ArrangeDb.MarketplaceListings.AddRange(
            MarketplaceListingFactory.CreateListing(user1.Id),
            MarketplaceListingFactory.CreateListing(user2.Id),
            MarketplaceListingFactory.CreateListing(user3.Id));
        await ArrangeDb.SaveChangesAsync();

        var res = await new GetMarketplaceListingsQuery.Handler(ActDb, Mapper, Mock.Of<IDateTime>()).Handle(new GetMarketplaceListingsQuery
        {
            Page = 0,
            PageSize = 2,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        Assert.That(res.Data, Is.Not.Null);
        Assert.That(res.Data!.TotalCount, Is.EqualTo(3));
        Assert.That(res.Data.Items.Count, Is.EqualTo(2));
        Assert.That(res.Data.Items.Select(o => o.Seller.Id), Is.EqualTo([user3.Id, user2.Id]));
    }

    [Test]
    public async Task ShouldClampPageSizeToOneWhenPageSizeLessThanOne()
    {
        User user1 = new();
        User user2 = new();
        ArrangeDb.Users.AddRange(user1, user2);
        await ArrangeDb.SaveChangesAsync();

        ArrangeDb.MarketplaceListings.AddRange(
            MarketplaceListingFactory.CreateListing(user1.Id),
            MarketplaceListingFactory.CreateListing(user2.Id));
        await ArrangeDb.SaveChangesAsync();

        var res = await new GetMarketplaceListingsQuery.Handler(ActDb, Mapper, Mock.Of<IDateTime>()).Handle(new GetMarketplaceListingsQuery
        {
            Page = 1,
            PageSize = 0,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        Assert.That(res.Data, Is.Not.Null);
        Assert.That(res.Data!.TotalCount, Is.EqualTo(2));
        Assert.That(res.Data.Items.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task ShouldReturnTotalCountBeforePagination()
    {
        User user = new();
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        ArrangeDb.MarketplaceListings.AddRange(
            MarketplaceListingFactory.CreateListing(user.Id),
            MarketplaceListingFactory.CreateListing(user.Id),
            MarketplaceListingFactory.CreateListing(user.Id),
            MarketplaceListingFactory.CreateListing(user.Id),
            MarketplaceListingFactory.CreateListing(user.Id));
        await ArrangeDb.SaveChangesAsync();

        var res = await new GetMarketplaceListingsQuery.Handler(ActDb, Mapper, Mock.Of<IDateTime>()).Handle(new GetMarketplaceListingsQuery
        {
            Page = 2,
            PageSize = 2,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        Assert.That(res.Data, Is.Not.Null);
        Assert.That(res.Data!.TotalCount, Is.EqualTo(5));
        Assert.That(res.Data.Items.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task ShouldFilterByOfferedItemId()
    {
        User user1 = new();
        User user2 = new();
        ArrangeDb.Users.AddRange(user1, user2);

        Item itemA = new() { Id = "item_a" };
        Item itemB = new() { Id = "item_b" };
        ArrangeDb.Items.AddRange(itemA, itemB);
        await ArrangeDb.SaveChangesAsync();

        UserItem userItem1 = new() { UserId = user1.Id, ItemId = itemA.Id };
        UserItem userItem2 = new() { UserId = user2.Id, ItemId = itemB.Id };
        ArrangeDb.UserItems.AddRange(userItem1, userItem2);
        await ArrangeDb.SaveChangesAsync();

        ArrangeDb.MarketplaceListings.AddRange(
            MarketplaceListingFactory.CreateListing(user1.Id, offeredUserItemId: userItem1.Id),
            MarketplaceListingFactory.CreateListing(user2.Id, offeredUserItemId: userItem2.Id));
        await ArrangeDb.SaveChangesAsync();

        var res = await new GetMarketplaceListingsQuery.Handler(ActDb, Mapper, Mock.Of<IDateTime>()).Handle(new GetMarketplaceListingsQuery
        {
            Offered = new GetMarketplaceListingsQuery.SideFilter
            {
                ItemId = itemA.Id,
            },
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        Assert.That(res.Data, Is.Not.Null);
        Assert.That(res.Data!.TotalCount, Is.EqualTo(1));
        Assert.That(res.Data.Items.Count, Is.EqualTo(1));
        Assert.That(res.Data.Items[0].Seller.Id, Is.EqualTo(user1.Id));
    }

    [Test]
    public async Task ShouldFilterByRequestedItemId()
    {
        User user1 = new();
        User user2 = new();
        ArrangeDb.Users.AddRange(user1, user2);

        Item itemA = new() { Id = "item_a" };
        Item itemB = new() { Id = "item_b" };
        ArrangeDb.Items.AddRange(itemA, itemB);
        await ArrangeDb.SaveChangesAsync();

        ArrangeDb.MarketplaceListings.AddRange(
            MarketplaceListingFactory.CreateListing(user1.Id, requestedItemId: itemA.Id),
            MarketplaceListingFactory.CreateListing(user2.Id, requestedItemId: itemB.Id));
        await ArrangeDb.SaveChangesAsync();

        var res = await new GetMarketplaceListingsQuery.Handler(ActDb, Mapper, Mock.Of<IDateTime>()).Handle(new GetMarketplaceListingsQuery
        {
            Requested = new GetMarketplaceListingsQuery.SideFilter
            {
                ItemId = itemA.Id,
            },
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        Assert.That(res.Data, Is.Not.Null);
        Assert.That(res.Data!.TotalCount, Is.EqualTo(1));
        Assert.That(res.Data.Items.Count, Is.EqualTo(1));
        Assert.That(res.Data.Items[0].Seller.Id, Is.EqualTo(user1.Id));
    }

    [Test]
    public async Task ShouldFilterByOfferedItemRanks()
    {
        User user1 = new();
        User user2 = new();
        ArrangeDb.Users.AddRange(user1, user2);

        Item itemRank1 = new() { Id = "rank_1", Rank = 1 };
        Item itemRank3 = new() { Id = "rank_3", Rank = 3 };
        ArrangeDb.Items.AddRange(itemRank1, itemRank3);
        await ArrangeDb.SaveChangesAsync();

        UserItem userItem1 = new() { UserId = user1.Id, ItemId = itemRank1.Id };
        UserItem userItem2 = new() { UserId = user2.Id, ItemId = itemRank3.Id };
        ArrangeDb.UserItems.AddRange(userItem1, userItem2);
        await ArrangeDb.SaveChangesAsync();

        ArrangeDb.MarketplaceListings.AddRange(
            MarketplaceListingFactory.CreateListing(user1.Id, offeredUserItemId: userItem1.Id),
            MarketplaceListingFactory.CreateListing(user2.Id, offeredUserItemId: userItem2.Id));
        await ArrangeDb.SaveChangesAsync();

        var res = await new GetMarketplaceListingsQuery.Handler(ActDb, Mapper, Mock.Of<IDateTime>()).Handle(new GetMarketplaceListingsQuery
        {
            Offered = new GetMarketplaceListingsQuery.SideFilter
            {
                ItemRanks = [3],
            },
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        Assert.That(res.Data, Is.Not.Null);
        Assert.That(res.Data!.TotalCount, Is.EqualTo(1));
        Assert.That(res.Data.Items.Count, Is.EqualTo(1));
        Assert.That(res.Data.Items[0].Seller.Id, Is.EqualTo(user2.Id));
    }

    [Test]
    public async Task ShouldFilterByRequestedItemRanks()
    {
        User user1 = new();
        User user2 = new();
        ArrangeDb.Users.AddRange(user1, user2);
        Item itemRank1 = new() { Id = "rank_1", Rank = 1 };
        Item itemRank3 = new() { Id = "rank_3", Rank = 3 };
        ArrangeDb.Items.AddRange(itemRank1, itemRank3);

        await ArrangeDb.SaveChangesAsync();

        ArrangeDb.MarketplaceListings.AddRange(
            MarketplaceListingFactory.CreateListing(user1.Id, requestedItemId: itemRank1.Id),
            MarketplaceListingFactory.CreateListing(user2.Id, requestedItemId: itemRank3.Id));
        await ArrangeDb.SaveChangesAsync();

        var res = await new GetMarketplaceListingsQuery.Handler(ActDb, Mapper, Mock.Of<IDateTime>()).Handle(new GetMarketplaceListingsQuery
        {
            Requested = new GetMarketplaceListingsQuery.SideFilter
            {
                ItemRanks = [3],
            },
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        Assert.That(res.Data, Is.Not.Null);
        Assert.That(res.Data!.TotalCount, Is.EqualTo(1));
        Assert.That(res.Data.Items.Count, Is.EqualTo(1));
        Assert.That(res.Data.Items[0].Seller.Id, Is.EqualTo(user2.Id));
    }

    [Test]
    public async Task ShouldFilterByOfferedItemType()
    {
        User user1 = new();
        User user2 = new();
        ArrangeDb.Users.AddRange(user1, user2);

        Item shield = new() { Id = "offered_shield", Type = ItemType.Shield };
        Item bow = new() { Id = "offered_bow", Type = ItemType.Bow };
        ArrangeDb.Items.AddRange(shield, bow);
        await ArrangeDb.SaveChangesAsync();

        UserItem userItem1 = new() { UserId = user1.Id, ItemId = shield.Id };
        UserItem userItem2 = new() { UserId = user2.Id, ItemId = bow.Id };
        ArrangeDb.UserItems.AddRange(userItem1, userItem2);
        await ArrangeDb.SaveChangesAsync();

        ArrangeDb.MarketplaceListings.AddRange(
            MarketplaceListingFactory.CreateListing(user1.Id, offeredUserItemId: userItem1.Id),
            MarketplaceListingFactory.CreateListing(user2.Id, offeredUserItemId: userItem2.Id));
        await ArrangeDb.SaveChangesAsync();

        var res = await new GetMarketplaceListingsQuery.Handler(ActDb, Mapper, Mock.Of<IDateTime>()).Handle(new GetMarketplaceListingsQuery
        {
            Offered = new GetMarketplaceListingsQuery.SideFilter
            {
                ItemType = ItemType.Bow,
            },
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        Assert.That(res.Data, Is.Not.Null);
        Assert.That(res.Data!.TotalCount, Is.EqualTo(1));
        Assert.That(res.Data.Items.Count, Is.EqualTo(1));
        Assert.That(res.Data.Items[0].Seller.Id, Is.EqualTo(user2.Id));
    }

    [Test]
    public async Task ShouldFilterByRequestedItemType()
    {
        User user1 = new();
        User user2 = new();
        ArrangeDb.Users.AddRange(user1, user2);
        Item shield = new() { Id = "requested_shield", Type = ItemType.Shield };
        Item bow = new() { Id = "requested_bow", Type = ItemType.Bow };
        ArrangeDb.Items.AddRange(shield, bow);

        await ArrangeDb.SaveChangesAsync();

        ArrangeDb.MarketplaceListings.AddRange(
            MarketplaceListingFactory.CreateListing(user1.Id, requestedItemId: shield.Id),
            MarketplaceListingFactory.CreateListing(user2.Id, requestedItemId: bow.Id));
        await ArrangeDb.SaveChangesAsync();

        var res = await new GetMarketplaceListingsQuery.Handler(ActDb, Mapper, Mock.Of<IDateTime>()).Handle(new GetMarketplaceListingsQuery
        {
            Requested = new GetMarketplaceListingsQuery.SideFilter
            {
                ItemType = ItemType.Bow,
            },
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        Assert.That(res.Data, Is.Not.Null);
        Assert.That(res.Data!.TotalCount, Is.EqualTo(1));
        Assert.That(res.Data.Items.Count, Is.EqualTo(1));
        Assert.That(res.Data.Items[0].Seller.Id, Is.EqualTo(user2.Id));
    }

    [Test]
    public async Task ShouldFilterByOfferedItemRankAndTypeTogether()
    {
        User user1 = new();
        User user2 = new();
        User user3 = new();
        ArrangeDb.Users.AddRange(user1, user2, user3);

        Item rank3Bow = new() { Id = "rank3_bow", Rank = 3, Type = ItemType.Bow };
        Item rank3Shield = new() { Id = "rank3_shield", Rank = 3, Type = ItemType.Shield };
        Item rank2Bow = new() { Id = "rank2_bow", Rank = 2, Type = ItemType.Bow };
        ArrangeDb.Items.AddRange(rank3Bow, rank3Shield, rank2Bow);
        await ArrangeDb.SaveChangesAsync();

        UserItem userItem1 = new() { UserId = user1.Id, ItemId = rank3Bow.Id };
        UserItem userItem2 = new() { UserId = user2.Id, ItemId = rank3Shield.Id };
        UserItem userItem3 = new() { UserId = user3.Id, ItemId = rank2Bow.Id };
        ArrangeDb.UserItems.AddRange(userItem1, userItem2, userItem3);
        await ArrangeDb.SaveChangesAsync();

        ArrangeDb.MarketplaceListings.AddRange(
            MarketplaceListingFactory.CreateListing(user1.Id, offeredUserItemId: userItem1.Id),
            MarketplaceListingFactory.CreateListing(user2.Id, offeredUserItemId: userItem2.Id),
            MarketplaceListingFactory.CreateListing(user3.Id, offeredUserItemId: userItem3.Id));
        await ArrangeDb.SaveChangesAsync();

        var res = await new GetMarketplaceListingsQuery.Handler(ActDb, Mapper, Mock.Of<IDateTime>()).Handle(new GetMarketplaceListingsQuery
        {
            Offered = new GetMarketplaceListingsQuery.SideFilter
            {
                ItemRanks = [3],
                ItemType = ItemType.Bow,
            },
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        Assert.That(res.Data, Is.Not.Null);
        Assert.That(res.Data!.TotalCount, Is.EqualTo(1));
        Assert.That(res.Data.Items.Count, Is.EqualTo(1));
        Assert.That(res.Data.Items[0].Seller.Id, Is.EqualTo(user1.Id));
    }

    [Test]
    public async Task ShouldFilterByRequestedItemRankAndTypeTogether()
    {
        User user1 = new();
        User user2 = new();
        User user3 = new();
        ArrangeDb.Users.AddRange(user1, user2, user3);

        Item rank3Bow = new() { Id = "req_rank3_bow", Rank = 3, Type = ItemType.Bow };
        Item rank3Shield = new() { Id = "req_rank3_shield", Rank = 3, Type = ItemType.Shield };
        Item rank2Bow = new() { Id = "req_rank2_bow", Rank = 2, Type = ItemType.Bow };
        ArrangeDb.Items.AddRange(rank3Bow, rank3Shield, rank2Bow);
        await ArrangeDb.SaveChangesAsync();

        ArrangeDb.MarketplaceListings.AddRange(
            MarketplaceListingFactory.CreateListing(user1.Id, requestedItemId: rank3Bow.Id),
            MarketplaceListingFactory.CreateListing(user2.Id, requestedItemId: rank3Shield.Id),
            MarketplaceListingFactory.CreateListing(user3.Id, requestedItemId: rank2Bow.Id));
        await ArrangeDb.SaveChangesAsync();

        var res = await new GetMarketplaceListingsQuery.Handler(ActDb, Mapper, Mock.Of<IDateTime>()).Handle(new GetMarketplaceListingsQuery
        {
            Requested = new GetMarketplaceListingsQuery.SideFilter
            {
                ItemRanks = [3],
                ItemType = ItemType.Bow,
            },
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        Assert.That(res.Data, Is.Not.Null);
        Assert.That(res.Data!.TotalCount, Is.EqualTo(1));
        Assert.That(res.Data.Items.Count, Is.EqualTo(1));
        Assert.That(res.Data.Items[0].Seller.Id, Is.EqualTo(user1.Id));
    }

    [Test]
    public async Task ShouldClampPageSizeToOneHundredWhenPageSizeGreaterThanOneHundred()
    {
        User user = new();
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        for (int i = 0; i < 120; i++)
        {
            ArrangeDb.MarketplaceListings.Add(MarketplaceListingFactory.CreateListing(user.Id, createdAt: DateTime.UtcNow.AddMinutes(-i)));
        }

        await ArrangeDb.SaveChangesAsync();

        var res = await new GetMarketplaceListingsQuery.Handler(ActDb, Mapper, Mock.Of<IDateTime>()).Handle(new GetMarketplaceListingsQuery
        {
            Page = 1,
            PageSize = 1000,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        Assert.That(res.Data, Is.Not.Null);
        Assert.That(res.Data!.TotalCount, Is.EqualTo(120));
        Assert.That(res.Data.Items.Count, Is.EqualTo(100));
    }

    [Test]
    public async Task ShouldFilterGoldNone()
    {
        User user1 = new();
        User user2 = new();
        ArrangeDb.Users.AddRange(user1, user2);
        await ArrangeDb.SaveChangesAsync();

        ArrangeDb.MarketplaceListings.AddRange(
            MarketplaceListingFactory.CreateListing(user1.Id, requestedGold: 0),
            MarketplaceListingFactory.CreateListing(user2.Id, requestedGold: 100));
        await ArrangeDb.SaveChangesAsync();

        var res = await new GetMarketplaceListingsQuery.Handler(ActDb, Mapper, Mock.Of<IDateTime>()).Handle(new GetMarketplaceListingsQuery
        {
            Requested = new GetMarketplaceListingsQuery.SideFilter
            {
                Gold = new MarketplaceCurrencyFilter { Mode = MarketplaceCurrencyFilterMode.None },
            },
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        Assert.That(res.Data, Is.Not.Null);
        Assert.That(res.Data!.TotalCount, Is.EqualTo(1));
        Assert.That(res.Data.Items.Count, Is.EqualTo(1));
        Assert.That(res.Data.Items[0].Seller.Id, Is.EqualTo(user1.Id));
    }

    [Test]
    public async Task ShouldFilterGoldCustomRange()
    {
        User user1 = new();
        User user2 = new();
        User user3 = new();
        ArrangeDb.Users.AddRange(user1, user2, user3);
        await ArrangeDb.SaveChangesAsync();

        ArrangeDb.MarketplaceListings.AddRange(
            MarketplaceListingFactory.CreateListing(user1.Id, requestedGold: 0),
            MarketplaceListingFactory.CreateListing(user2.Id, requestedGold: 25),
            MarketplaceListingFactory.CreateListing(user3.Id, requestedGold: 200));
        await ArrangeDb.SaveChangesAsync();

        var res = await new GetMarketplaceListingsQuery.Handler(ActDb, Mapper, Mock.Of<IDateTime>()).Handle(new GetMarketplaceListingsQuery
        {
            Requested = new GetMarketplaceListingsQuery.SideFilter
            {
                Gold = new MarketplaceCurrencyFilter { Mode = MarketplaceCurrencyFilterMode.CustomRange, Range = (10, 100) },
            },
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        Assert.That(res.Data, Is.Not.Null);
        Assert.That(res.Data!.TotalCount, Is.EqualTo(1));
        Assert.That(res.Data.Items.Count, Is.EqualTo(1));
        Assert.That(res.Data.Items[0].Seller.Id, Is.EqualTo(user2.Id));
    }

    [Test]
    public async Task ShouldFilterHeirloomPointsNone()
    {
        User user1 = new();
        User user2 = new();
        ArrangeDb.Users.AddRange(user1, user2);
        await ArrangeDb.SaveChangesAsync();

        ArrangeDb.MarketplaceListings.AddRange(
            MarketplaceListingFactory.CreateListing(user1.Id, requestedHeirloomPoints: 0),
            MarketplaceListingFactory.CreateListing(user2.Id, requestedHeirloomPoints: 3));
        await ArrangeDb.SaveChangesAsync();

        var res = await new GetMarketplaceListingsQuery.Handler(ActDb, Mapper, Mock.Of<IDateTime>()).Handle(new GetMarketplaceListingsQuery
        {
            Requested = new GetMarketplaceListingsQuery.SideFilter
            {
                HeirloomPoints = new MarketplaceCurrencyFilter { Mode = MarketplaceCurrencyFilterMode.None },
            },
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        Assert.That(res.Data, Is.Not.Null);
        Assert.That(res.Data!.TotalCount, Is.EqualTo(1));
        Assert.That(res.Data.Items.Count, Is.EqualTo(1));
        Assert.That(res.Data.Items[0].Seller.Id, Is.EqualTo(user1.Id));
    }

    [Test]
    public async Task ShouldFilterHeirloomPointsCustomRange()
    {
        User user1 = new();
        User user2 = new();
        User user3 = new();
        ArrangeDb.Users.AddRange(user1, user2, user3);
        await ArrangeDb.SaveChangesAsync();

        ArrangeDb.MarketplaceListings.AddRange(
            MarketplaceListingFactory.CreateListing(user1.Id, requestedHeirloomPoints: 0),
            MarketplaceListingFactory.CreateListing(user2.Id, requestedHeirloomPoints: 3),
            MarketplaceListingFactory.CreateListing(user3.Id, requestedHeirloomPoints: 10));
        await ArrangeDb.SaveChangesAsync();

        var res = await new GetMarketplaceListingsQuery.Handler(ActDb, Mapper, Mock.Of<IDateTime>()).Handle(new GetMarketplaceListingsQuery
        {
            Requested = new GetMarketplaceListingsQuery.SideFilter
            {
                HeirloomPoints = new MarketplaceCurrencyFilter { Mode = MarketplaceCurrencyFilterMode.CustomRange, Range = (2, 5) },
            },
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        Assert.That(res.Data, Is.Not.Null);
        Assert.That(res.Data!.TotalCount, Is.EqualTo(1));
        Assert.That(res.Data.Items.Count, Is.EqualTo(1));
        Assert.That(res.Data.Items[0].Seller.Id, Is.EqualTo(user2.Id));
    }

    [Test]
    public async Task ShouldNormalizeRangeWhenMinGreaterThanMax()
    {
        User user1 = new();
        User user2 = new();
        User user3 = new();
        ArrangeDb.Users.AddRange(user1, user2, user3);
        await ArrangeDb.SaveChangesAsync();

        ArrangeDb.MarketplaceListings.AddRange(
            MarketplaceListingFactory.CreateListing(user1.Id, requestedGold: 4),
            MarketplaceListingFactory.CreateListing(user2.Id, requestedGold: 7),
            MarketplaceListingFactory.CreateListing(user3.Id, requestedGold: 12));
        await ArrangeDb.SaveChangesAsync();

        var res = await new GetMarketplaceListingsQuery.Handler(ActDb, Mapper, Mock.Of<IDateTime>()).Handle(new GetMarketplaceListingsQuery
        {
            Requested = new GetMarketplaceListingsQuery.SideFilter
            {
                Gold = new MarketplaceCurrencyFilter { Mode = MarketplaceCurrencyFilterMode.CustomRange, Range = (10, 5) },
            },
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        Assert.That(res.Data, Is.Not.Null);
        Assert.That(res.Data!.TotalCount, Is.EqualTo(1));
        Assert.That(res.Data.Items.Count, Is.EqualTo(1));
        Assert.That(res.Data.Items[0].Seller.Id, Is.EqualTo(user2.Id));
    }

    [Test]
    public async Task ShouldClampNegativeRangeBoundsToZero()
    {
        User user1 = new();
        User user2 = new();
        User user3 = new();
        ArrangeDb.Users.AddRange(user1, user2, user3);
        await ArrangeDb.SaveChangesAsync();

        ArrangeDb.MarketplaceListings.AddRange(
            MarketplaceListingFactory.CreateListing(user1.Id, requestedGold: 0),
            MarketplaceListingFactory.CreateListing(user2.Id, requestedGold: 1),
            MarketplaceListingFactory.CreateListing(user3.Id, requestedGold: 6));
        await ArrangeDb.SaveChangesAsync();

        var res = await new GetMarketplaceListingsQuery.Handler(ActDb, Mapper, Mock.Of<IDateTime>()).Handle(new GetMarketplaceListingsQuery
        {
            Requested = new GetMarketplaceListingsQuery.SideFilter
            {
                Gold = new MarketplaceCurrencyFilter { Mode = MarketplaceCurrencyFilterMode.CustomRange, Range = (-10, 5) },
            },
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        Assert.That(res.Data, Is.Not.Null);
        Assert.That(res.Data!.TotalCount, Is.EqualTo(1));
        Assert.That(res.Data.Items.Count, Is.EqualTo(1));
        Assert.That(res.Data.Items[0].Seller.Id, Is.EqualTo(user2.Id));
    }

    [Test]
    public async Task ShouldNotApplyBuyerFilterWhenBuyerNotFound()
    {
        User seller1 = new();
        User seller2 = new();
        ArrangeDb.Users.AddRange(seller1, seller2);
        await ArrangeDb.SaveChangesAsync();

        ArrangeDb.MarketplaceListings.AddRange(
            MarketplaceListingFactory.CreateListing(seller1.Id, requestedGold: 999),
            MarketplaceListingFactory.CreateListing(seller2.Id, requestedGold: 999));
        await ArrangeDb.SaveChangesAsync();

        var res = await new GetMarketplaceListingsQuery.Handler(ActDb, Mapper, Mock.Of<IDateTime>()).Handle(new GetMarketplaceListingsQuery
        {
            BuyerId = 999999,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        Assert.That(res.Data, Is.Not.Null);
        Assert.That(res.Data!.TotalCount, Is.EqualTo(2));
        Assert.That(res.Data.Items.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task ShouldExcludeBuyerOwnListingsInAffordableMode()
    {
        User buyer = new() { Gold = 100, HeirloomPoints = 10 };
        User seller = new();
        ArrangeDb.Users.AddRange(buyer, seller);
        await ArrangeDb.SaveChangesAsync();

        ArrangeDb.MarketplaceListings.AddRange(
            MarketplaceListingFactory.CreateListing(buyer.Id, requestedGold: 1, requestedHeirloomPoints: 1),
            MarketplaceListingFactory.CreateListing(seller.Id, requestedGold: 1, requestedHeirloomPoints: 1));
        await ArrangeDb.SaveChangesAsync();

        var res = await new GetMarketplaceListingsQuery.Handler(ActDb, Mapper, Mock.Of<IDateTime>()).Handle(new GetMarketplaceListingsQuery
        {
            BuyerId = buyer.Id,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        Assert.That(res.Data, Is.Not.Null);
        Assert.That(res.Data!.TotalCount, Is.EqualTo(1));
        Assert.That(res.Data.Items.Count, Is.EqualTo(1));
        Assert.That(res.Data.Items[0].Seller.Id, Is.EqualTo(seller.Id));
    }

    [Test]
    public async Task ShouldReturnOnlyAffordableListingsForBuyerResources()
    {
        User buyer = new() { Gold = 100, HeirloomPoints = 5 };
        User seller1 = new();
        User seller2 = new();
        User seller3 = new();
        ArrangeDb.Users.AddRange(buyer, seller1, seller2, seller3);
        await ArrangeDb.SaveChangesAsync();

        ArrangeDb.MarketplaceListings.AddRange(
            MarketplaceListingFactory.CreateListing(seller1.Id, requestedGold: 60, requestedHeirloomPoints: 3),
            MarketplaceListingFactory.CreateListing(seller2.Id, requestedGold: 101, requestedHeirloomPoints: 3),
            MarketplaceListingFactory.CreateListing(seller3.Id, requestedGold: 60, requestedHeirloomPoints: 6));
        await ArrangeDb.SaveChangesAsync();

        var res = await new GetMarketplaceListingsQuery.Handler(ActDb, Mapper, Mock.Of<IDateTime>()).Handle(new GetMarketplaceListingsQuery
        {
            BuyerId = buyer.Id,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        Assert.That(res.Data, Is.Not.Null);
        Assert.That(res.Data!.TotalCount, Is.EqualTo(1));
        Assert.That(res.Data.Items.Count, Is.EqualTo(1));
        Assert.That(res.Data.Items[0].Seller.Id, Is.EqualTo(seller1.Id));
    }

    [Test]
    public async Task ShouldRequireBuyerOwningRequestedItem()
    {
        User buyer = new() { Gold = 100, HeirloomPoints = 5 };
        User seller = new();
        ArrangeDb.Users.AddRange(buyer, seller);
        ArrangeDb.Items.Add(new Item { Id = "req_item" });
        await ArrangeDb.SaveChangesAsync();

        ArrangeDb.MarketplaceListings.Add(MarketplaceListingFactory.CreateListing(seller.Id, requestedGold: 10, requestedHeirloomPoints: 1, requestedItemId: "req_item"));
        await ArrangeDb.SaveChangesAsync();

        var res = await new GetMarketplaceListingsQuery.Handler(ActDb, Mapper, Mock.Of<IDateTime>()).Handle(new GetMarketplaceListingsQuery
        {
            BuyerId = buyer.Id,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        Assert.That(res.Data, Is.Not.Null);
        Assert.That(res.Data!.TotalCount, Is.EqualTo(0));
        Assert.That(res.Data.Items, Is.Empty);
    }

    [Test]
    public async Task ShouldSkipItemOwnershipCheckWhenRequestedItemIsNull()
    {
        User buyer = new() { Gold = 100, HeirloomPoints = 5 };
        User seller = new();
        ArrangeDb.Users.AddRange(buyer, seller);
        await ArrangeDb.SaveChangesAsync();

        ArrangeDb.MarketplaceListings.Add(MarketplaceListingFactory.CreateListing(seller.Id, requestedGold: 10, requestedHeirloomPoints: 1, requestedItemId: null));
        await ArrangeDb.SaveChangesAsync();

        var res = await new GetMarketplaceListingsQuery.Handler(ActDb, Mapper, Mock.Of<IDateTime>()).Handle(new GetMarketplaceListingsQuery
        {
            BuyerId = buyer.Id,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        Assert.That(res.Data, Is.Not.Null);
        Assert.That(res.Data!.TotalCount, Is.EqualTo(1));
        Assert.That(res.Data.Items.Count, Is.EqualTo(1));
        Assert.That(res.Data.Items[0].Seller.Id, Is.EqualTo(seller.Id));
    }

    [Test]
    public async Task ShouldExcludeListingWhenBuyerRequestedItemIsBroken()
    {
        User buyer = new() { Gold = 100, HeirloomPoints = 5 };
        User seller = new();
        ArrangeDb.Users.AddRange(buyer, seller);
        ArrangeDb.Items.Add(new Item { Id = "req_item_broken" });
        await ArrangeDb.SaveChangesAsync();

        ArrangeDb.UserItems.Add(new UserItem
        {
            UserId = buyer.Id,
            ItemId = "req_item_broken",
            IsBroken = true,
        });
        ArrangeDb.MarketplaceListings.Add(MarketplaceListingFactory.CreateListing(seller.Id, requestedGold: 10, requestedHeirloomPoints: 1, requestedItemId: "req_item_broken"));
        await ArrangeDb.SaveChangesAsync();

        var res = await new GetMarketplaceListingsQuery.Handler(ActDb, Mapper, Mock.Of<IDateTime>()).Handle(new GetMarketplaceListingsQuery
        {
            BuyerId = buyer.Id,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        Assert.That(res.Data, Is.Not.Null);
        Assert.That(res.Data!.TotalCount, Is.EqualTo(0));
    }

    [Test]
    public async Task ShouldExcludeListingWhenBuyerRequestedItemIsPersonal()
    {
        User buyer = new() { Gold = 100, HeirloomPoints = 5 };
        User seller = new();
        ArrangeDb.Users.AddRange(buyer, seller);
        ArrangeDb.Items.Add(new Item { Id = "req_item_personal" });
        await ArrangeDb.SaveChangesAsync();

        UserItem buyerItem = new()
        {
            UserId = buyer.Id,
            ItemId = "req_item_personal",
            IsBroken = false,
        };
        ArrangeDb.UserItems.Add(buyerItem);
        await ArrangeDb.SaveChangesAsync();

        ArrangeDb.PersonalItems.Add(new PersonalItem { UserItemId = buyerItem.Id });
        ArrangeDb.MarketplaceListings.Add(MarketplaceListingFactory.CreateListing(seller.Id, requestedGold: 10, requestedHeirloomPoints: 1, requestedItemId: "req_item_personal"));
        await ArrangeDb.SaveChangesAsync();

        var res = await new GetMarketplaceListingsQuery.Handler(ActDb, Mapper, Mock.Of<IDateTime>()).Handle(new GetMarketplaceListingsQuery
        {
            BuyerId = buyer.Id,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        Assert.That(res.Data, Is.Not.Null);
        Assert.That(res.Data!.TotalCount, Is.EqualTo(0));
    }

    [Test]
    public async Task ShouldExcludeListingWhenBuyerRequestedItemIsClanBorrowed()
    {
        User buyer = new() { Gold = 100, HeirloomPoints = 5 };
        User seller = new();
        ArrangeDb.Users.AddRange(buyer, seller);
        ArrangeDb.Items.Add(new Item { Id = "req_item_borrowed" });
        await ArrangeDb.SaveChangesAsync();

        UserItem buyerItem = new()
        {
            UserId = buyer.Id,
            ItemId = "req_item_borrowed",
            IsBroken = false,
        };
        ArrangeDb.UserItems.Add(buyerItem);
        await ArrangeDb.SaveChangesAsync();

        ArrangeDb.ClanArmoryBorrowedItems.Add(new ClanArmoryBorrowedItem
        {
            UserItemId = buyerItem.Id,
            BorrowerUserId = buyer.Id,
            BorrowerClanId = 1,
        });
        ArrangeDb.MarketplaceListings.Add(MarketplaceListingFactory.CreateListing(seller.Id, requestedGold: 10, requestedHeirloomPoints: 1, requestedItemId: "req_item_borrowed"));
        await ArrangeDb.SaveChangesAsync();

        var res = await new GetMarketplaceListingsQuery.Handler(ActDb, Mapper, Mock.Of<IDateTime>()).Handle(new GetMarketplaceListingsQuery
        {
            BuyerId = buyer.Id,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        Assert.That(res.Data, Is.Not.Null);
        Assert.That(res.Data!.TotalCount, Is.EqualTo(0));
    }
}
