using Crpg.Application.Items.Queries;
using Crpg.Domain.Entities.Clans;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Marketplace;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Items;

public class GetUserItemsQueryTest : TestBase
{
    [Test]
    public async Task Basic()
    {
        var user = ArrangeDb.Users.Add(new User
        {
            Items =
            {
                new() { Item = new Item { Id = "1", Enabled = true } },
                new() { Item = new Item { Id = "2", Enabled = true } },
                new() { Item = new Item { Id = "3", Enabled = false } },
            },
        });
        await ArrangeDb.SaveChangesAsync();

        var result = await new GetUserItemsQuery.Handler(ActDb, Mapper).Handle(
            new GetUserItemsQuery { UserId = user.Entity.Id }, CancellationToken.None);

        Assert.That(result.Data!.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task PersonalItems()
    {
        var user = ArrangeDb.Users.Add(new User
        {
            Items =
            {
                new() { Item = new Item { Id = "1", Enabled = true } },
                new() { Item = new Item { Id = "2", Enabled = true } },
                new() { Item = new Item { Id = "3", Enabled = false, }, PersonalItem = new() },
            },
        });
        await ArrangeDb.SaveChangesAsync();

        var result = await new GetUserItemsQuery.Handler(ActDb, Mapper).Handle(
            new GetUserItemsQuery { UserId = user.Entity.Id }, CancellationToken.None);

        Assert.That(result.Data!.Count, Is.EqualTo(3));
    }

    [Test]
    public async Task ClanArmoryBorrowedItems()
    {
        var borrower = ArrangeDb.Users.Add(new User { Id = 100 }).Entity;
        var lender = ArrangeDb.Users.Add(new User
        {
            Id = 101,
            Items =
            {
                new()
                {
                    Item = new Item { Id = "1", Enabled = true },
                    ClanArmoryBorrowedItem = new ClanArmoryBorrowedItem { BorrowerUserId = borrower.Id },
                },
            },
        });
        await ArrangeDb.SaveChangesAsync();

        var result = await new GetUserItemsQuery.Handler(ActDb, Mapper).Handle(
            new GetUserItemsQuery { UserId = borrower.Id }, CancellationToken.None);

        Assert.That(result.Data!.Count, Is.EqualTo(1));
        Assert.That(result.Data.Any(i => i.Id == lender.Entity.Items[0].Id), Is.True);
    }

    [Test]
    public async Task ClanArmoryBorrowedItemsFromAnotherBorrower()
    {
        var borrower = ArrangeDb.Users.Add(new User { Id = 100 }).Entity;
        var anotherUser = ArrangeDb.Users.Add(new User { Id = 101 }).Entity;
        ArrangeDb.Users.Add(new User
        {
            Id = 102,
            Items =
            {
                new()
                {
                    Item = new Item { Id = "1", Enabled = true },
                    ClanArmoryBorrowedItem = new ClanArmoryBorrowedItem { BorrowerUserId = borrower.Id },
                },
            },
        });
        await ArrangeDb.SaveChangesAsync();

        var result = await new GetUserItemsQuery.Handler(ActDb, Mapper).Handle(
            new GetUserItemsQuery { UserId = anotherUser.Id }, CancellationToken.None);

        Assert.That(result.Data, Is.Empty);
    }

    [Test]
    public async Task DisabledClanArmoryBorrowedItems()
    {
        var borrower = ArrangeDb.Users.Add(new User { Id = 100 }).Entity;
        ArrangeDb.Users.Add(new User
        {
            Id = 101,
            Items =
            {
                new()
                {
                    Item = new Item { Id = "1", Enabled = false },
                    ClanArmoryBorrowedItem = new ClanArmoryBorrowedItem { BorrowerUserId = borrower.Id },
                },
            },
        });
        await ArrangeDb.SaveChangesAsync();

        var result = await new GetUserItemsQuery.Handler(ActDb, Mapper).Handle(
            new GetUserItemsQuery { UserId = borrower.Id }, CancellationToken.None);

        Assert.That(result.Data, Is.Empty);
    }

    [Test]
    public async Task IsListedOnMarketplaceWhenOfferedAsset()
    {
        var item = new Item { Id = "1", Enabled = true };
        var userItem = new UserItem { Item = item };
        var user = ArrangeDb.Users.Add(new User { Items = { userItem } });
        await ArrangeDb.SaveChangesAsync();

        ArrangeDb.MarketplaceOffers.Add(new MarketplaceOffer
        {
            SellerId = user.Entity.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            Assets =
            [
                new MarketplaceOfferAsset { Side = MarketplaceOfferAssetSide.Offered, UserItemId = userItem.Id },
                new MarketplaceOfferAsset { Side = MarketplaceOfferAssetSide.Requested, Gold = 100 },
            ],
        });
        await ArrangeDb.SaveChangesAsync();

        var result = await new GetUserItemsQuery.Handler(ActDb, Mapper).Handle(
            new GetUserItemsQuery { UserId = user.Entity.Id }, CancellationToken.None);

        Assert.That(result.Data!.Single().IsListedOnMarketplace, Is.True);
    }

    [Test]
    public async Task IsNotListedOnMarketplaceWhenNoOfferedAsset()
    {
        var user = ArrangeDb.Users.Add(new User
        {
            Items = { new() { Item = new Item { Id = "1", Enabled = true } } },
        });
        await ArrangeDb.SaveChangesAsync();

        var result = await new GetUserItemsQuery.Handler(ActDb, Mapper).Handle(
            new GetUserItemsQuery { UserId = user.Entity.Id }, CancellationToken.None);

        Assert.That(result.Data!.Single().IsListedOnMarketplace, Is.False);
    }
}
