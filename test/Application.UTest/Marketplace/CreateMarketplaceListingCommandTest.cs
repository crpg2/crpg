using Crpg.Application.Common;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Marketplace.Commands;
using Crpg.Application.Marketplace.Models;
using Crpg.Domain.Entities.ActivityLogs;
using Crpg.Domain.Entities.Clans;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Users;
using Crpg.Sdk.Abstractions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Marketplace;

public class CreateMarketplaceListingCommandTest : TestBase
{
    private static readonly Constants MarketplaceConstants = new()
    {
        MarketplaceActiveListingLimit = 10,
        MarketplaceListingFeePerDay = 0,
        MarketplaceListingDurationDays = 1,
        MarketplaceGoldFeePercent = 0,
    };

    [Test]
    public async Task ShouldReturnErrorWhenSellerDoesNotExist()
    {
        var result = await new CreateMarketplaceListingCommand.Handler(ActDb, Mapper, MarketplaceConstants, Mock.Of<IActivityLogService>(), Mock.Of<IDateTime>()).Handle(new CreateMarketplaceListingCommand
        {
            UserId = 99,
            Offer = new MarketplaceListingAssetInput { Gold = 100 },
            Request = new MarketplaceListingAssetInput { HeirloomPoints = 5 },
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Not.Null);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.UserNotFound));
    }

    [Test]
    public async Task ShouldReturnErrorWhenActiveListingLimitIsReached()
    {
        User seller = new() { Gold = 1000, HeirloomPoints = 1000 };
        ArrangeDb.Users.Add(seller);
        await ArrangeDb.SaveChangesAsync();

        ArrangeDb.MarketplaceListings.AddRange(Enumerable.Range(0, MarketplaceConstants.MarketplaceActiveListingLimit)
            .Select(_ => MarketplaceListingFactory.CreateListing(seller.Id, offeredGold: 10, requestedGold: 20)));
        await ArrangeDb.SaveChangesAsync();

        var result = await new CreateMarketplaceListingCommand.Handler(ActDb, Mapper, MarketplaceConstants, Mock.Of<IActivityLogService>(), Mock.Of<IDateTime>()).Handle(new CreateMarketplaceListingCommand
        {
            UserId = seller.Id,
            Offer = new MarketplaceListingAssetInput { Gold = 100 },
            Request = new MarketplaceListingAssetInput { HeirloomPoints = 5 },
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Not.Null);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.MarketplaceListingLimitReached));
    }

    [Test]
    public async Task ShouldReturnErrorWhenGoldIsBothOfferedAndRequested()
    {
        User seller = new() { Gold = 1000, HeirloomPoints = 1000 };
        ArrangeDb.Users.Add(seller);
        await ArrangeDb.SaveChangesAsync();

        var result = await new CreateMarketplaceListingCommand.Handler(ActDb, Mapper, MarketplaceConstants, Mock.Of<IActivityLogService>(), Mock.Of<IDateTime>()).Handle(new CreateMarketplaceListingCommand
        {
            UserId = seller.Id,
            Offer = new MarketplaceListingAssetInput { Gold = 100 },
            Request = new MarketplaceListingAssetInput { Gold = 50 },
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Not.Null);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.MarketplaceListingInvalidAsset));
        Assert.That(result.Errors[0].Detail, Is.EqualTo("Gold cannot be both offered and requested in the same listing"));
    }

    [Test]
    public async Task ShouldReturnErrorWhenHeirloomPointsAreBothOfferedAndRequested()
    {
        User seller = new() { Gold = 1000, HeirloomPoints = 1000 };
        ArrangeDb.Users.Add(seller);
        await ArrangeDb.SaveChangesAsync();

        var result = await new CreateMarketplaceListingCommand.Handler(ActDb, Mapper, MarketplaceConstants, Mock.Of<IActivityLogService>(), Mock.Of<IDateTime>()).Handle(new CreateMarketplaceListingCommand
        {
            UserId = seller.Id,
            Offer = new MarketplaceListingAssetInput { HeirloomPoints = 10 },
            Request = new MarketplaceListingAssetInput { HeirloomPoints = 5 },
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Not.Null);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.MarketplaceListingInvalidAsset));
        Assert.That(result.Errors[0].Detail, Is.EqualTo("Heirloom points cannot be both offered and requested in the same listing"));
    }

    [Test]
    public async Task ShouldReturnErrorWhenOfferedAssetIsEmpty()
    {
        User seller = new() { Gold = 1000, HeirloomPoints = 1000 };
        ArrangeDb.Users.Add(seller);
        await ArrangeDb.SaveChangesAsync();

        var result = await new CreateMarketplaceListingCommand.Handler(ActDb, Mapper, MarketplaceConstants, Mock.Of<IActivityLogService>(), Mock.Of<IDateTime>()).Handle(new CreateMarketplaceListingCommand
        {
            UserId = seller.Id,
            Offer = new MarketplaceListingAssetInput { Gold = 0, HeirloomPoints = 0 },
            Request = new MarketplaceListingAssetInput { Gold = 100 },
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Not.Null);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.MarketplaceListingInvalidAsset));
    }

    [Test]
    public async Task ShouldReturnErrorWhenUserItemDoesNotBelongToSeller()
    {
        User seller = new() { Gold = 1000, HeirloomPoints = 1000 };
        ArrangeDb.Users.Add(seller);
        await ArrangeDb.SaveChangesAsync();

        var result = await new CreateMarketplaceListingCommand.Handler(ActDb, Mapper, MarketplaceConstants, Mock.Of<IActivityLogService>(), Mock.Of<IDateTime>()).Handle(new CreateMarketplaceListingCommand
        {
            UserId = seller.Id,
            Offer = new MarketplaceListingAssetInput { UserItemId = 999 },
            Request = new MarketplaceListingAssetInput { Gold = 100 },
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Not.Null);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.UserItemNotFound));
    }

    [Test]
    public async Task ShouldReturnErrorWhenUserItemIsBroken()
    {
        UserItem brokenItem = new() { IsBroken = true, Item = new Item { Id = "item_a" } };
        User seller = new() { Gold = 1000, HeirloomPoints = 1000, Items = { brokenItem } };
        ArrangeDb.Users.Add(seller);
        await ArrangeDb.SaveChangesAsync();

        var result = await new CreateMarketplaceListingCommand.Handler(ActDb, Mapper, MarketplaceConstants, Mock.Of<IActivityLogService>(), Mock.Of<IDateTime>()).Handle(new CreateMarketplaceListingCommand
        {
            UserId = seller.Id,
            Offer = new MarketplaceListingAssetInput { UserItemId = brokenItem.Id },
            Request = new MarketplaceListingAssetInput { Gold = 100 },
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Not.Null);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.MarketplaceListingInvalidAsset));
        Assert.That(result.Errors[0].Detail, Does.Contain("broken"));
    }

    [Test]
    public async Task ShouldReturnErrorWhenUserItemIsPersonal()
    {
        UserItem personalItem = new() { Item = new Item { Id = "item_b" }, PersonalItem = new PersonalItem() };
        User seller = new() { Gold = 1000, HeirloomPoints = 1000, Items = { personalItem } };
        ArrangeDb.Users.Add(seller);
        await ArrangeDb.SaveChangesAsync();

        var result = await new CreateMarketplaceListingCommand.Handler(ActDb, Mapper, MarketplaceConstants, Mock.Of<IActivityLogService>(), Mock.Of<IDateTime>()).Handle(new CreateMarketplaceListingCommand
        {
            UserId = seller.Id,
            Offer = new MarketplaceListingAssetInput { UserItemId = personalItem.Id },
            Request = new MarketplaceListingAssetInput { Gold = 100 },
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Not.Null);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.MarketplaceListingInvalidAsset));
        Assert.That(result.Errors[0].Detail, Does.Contain("personal item"));
    }

    [Test]
    public async Task ShouldReturnErrorWhenUserItemIsInClanArmory()
    {
        UserItem armoryItem = new() { Item = new Item { Id = "item_c" }, ClanArmoryItem = new ClanArmoryItem() };
        User seller = new() { Gold = 1000, HeirloomPoints = 1000, Items = { armoryItem } };
        ArrangeDb.Users.Add(seller);
        await ArrangeDb.SaveChangesAsync();

        var result = await new CreateMarketplaceListingCommand.Handler(ActDb, Mapper, MarketplaceConstants, Mock.Of<IActivityLogService>(), Mock.Of<IDateTime>()).Handle(new CreateMarketplaceListingCommand
        {
            UserId = seller.Id,
            Offer = new MarketplaceListingAssetInput { UserItemId = armoryItem.Id },
            Request = new MarketplaceListingAssetInput { Gold = 100 },
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Not.Null);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.MarketplaceListingInvalidAsset));
        Assert.That(result.Errors[0].Detail, Does.Contain("clan armory"));
    }

    [Test]
    public async Task ShouldReturnErrorWhenUserItemIsBorrowedFromClanArmory()
    {
        UserItem borrowedItem = new() { Item = new Item { Id = "item_d" }, ClanArmoryBorrowedItem = new ClanArmoryBorrowedItem() };
        User seller = new() { Gold = 1000, HeirloomPoints = 1000, Items = { borrowedItem } };
        ArrangeDb.Users.Add(seller);
        await ArrangeDb.SaveChangesAsync();

        var result = await new CreateMarketplaceListingCommand.Handler(ActDb, Mapper, MarketplaceConstants, Mock.Of<IActivityLogService>(), Mock.Of<IDateTime>()).Handle(new CreateMarketplaceListingCommand
        {
            UserId = seller.Id,
            Offer = new MarketplaceListingAssetInput { UserItemId = borrowedItem.Id },
            Request = new MarketplaceListingAssetInput { Gold = 100 },
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Not.Null);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.MarketplaceListingInvalidAsset));
        Assert.That(result.Errors[0].Detail, Does.Contain("borrowed"));
    }

    [Test]
    public async Task ShouldReturnErrorWhenRequestedAssetIsEmpty()
    {
        User seller = new() { Gold = 1000, HeirloomPoints = 1000 };
        ArrangeDb.Users.Add(seller);
        await ArrangeDb.SaveChangesAsync();

        var result = await new CreateMarketplaceListingCommand.Handler(ActDb, Mapper, MarketplaceConstants, Mock.Of<IActivityLogService>(), Mock.Of<IDateTime>()).Handle(new CreateMarketplaceListingCommand
        {
            UserId = seller.Id,
            Offer = new MarketplaceListingAssetInput { Gold = 100 },
            Request = new MarketplaceListingAssetInput { Gold = 0, HeirloomPoints = 0 },
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Not.Null);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.MarketplaceListingInvalidAsset));
    }

    [Test]
    public async Task ShouldReturnErrorWhenRequestedItemDoesNotExist()
    {
        User seller = new() { Gold = 1000, HeirloomPoints = 1000 };
        ArrangeDb.Users.Add(seller);
        await ArrangeDb.SaveChangesAsync();

        var result = await new CreateMarketplaceListingCommand.Handler(ActDb, Mapper, MarketplaceConstants, Mock.Of<IActivityLogService>(), Mock.Of<IDateTime>()).Handle(new CreateMarketplaceListingCommand
        {
            UserId = seller.Id,
            Offer = new MarketplaceListingAssetInput { Gold = 100 },
            Request = new MarketplaceListingAssetInput { ItemId = "nonexistent_item" },
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Not.Null);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.ItemNotFound));
    }

    [Test]
    public async Task ShouldReturnErrorWhenNotEnoughGold()
    {
        var constants = new Constants { MarketplaceActiveListingLimit = 10, MarketplaceListingFeePerDay = 100, MarketplaceListingDurationDays = 3, MarketplaceGoldFeePercent = 0 };
        // listingFee = 100 * 3 = 300, reservedGold = 50 => total = 350
        User seller = new() { Gold = 349, HeirloomPoints = 1000 };
        ArrangeDb.Users.Add(seller);
        await ArrangeDb.SaveChangesAsync();

        var result = await new CreateMarketplaceListingCommand.Handler(ActDb, Mapper, constants, Mock.Of<IActivityLogService>(), Mock.Of<IDateTime>()).Handle(new CreateMarketplaceListingCommand
        {
            UserId = seller.Id,
            Offer = new MarketplaceListingAssetInput { Gold = 50 },
            Request = new MarketplaceListingAssetInput { HeirloomPoints = 5 },
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Not.Null);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.NotEnoughGold));
    }

    [Test]
    public async Task ShouldReturnErrorWhenNotEnoughHeirloomPoints()
    {
        User seller = new() { Gold = 1000, HeirloomPoints = 4 };
        ArrangeDb.Users.Add(seller);
        await ArrangeDb.SaveChangesAsync();

        var result = await new CreateMarketplaceListingCommand.Handler(ActDb, Mapper, MarketplaceConstants, Mock.Of<IActivityLogService>(), Mock.Of<IDateTime>()).Handle(new CreateMarketplaceListingCommand
        {
            UserId = seller.Id,
            Offer = new MarketplaceListingAssetInput { HeirloomPoints = 5 },
            Request = new MarketplaceListingAssetInput { Gold = 100 },
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Not.Null);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.NotEnoughHeirloomPoints));
    }

    [Test]
    public async Task ShouldCreateActivityLogWhenOfferIsCreated()
    {
        var constants = new Constants
        {
            MarketplaceActiveListingLimit = 10,
            MarketplaceListingFeePerDay = 10,
            MarketplaceListingDurationDays = 7,
            MarketplaceGoldFeePercent = 5,
        };
        // listingFee = 70, goldFee = floor((100 + 0) * 5 / 100) = 5
        User seller = new() { Gold = 10000, HeirloomPoints = 10 };
        ArrangeDb.Users.Add(seller);
        await ArrangeDb.SaveChangesAsync();

        var result = await new CreateMarketplaceListingCommand.Handler(ActDb, Mapper, constants, new ActivityLogService(new MetadataService()), Mock.Of<IDateTime>()).Handle(new CreateMarketplaceListingCommand
        {
            UserId = seller.Id,
            Offer = new MarketplaceListingAssetInput { Gold = 100 },
            Request = new MarketplaceListingAssetInput { HeirloomPoints = 5 },
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Null);

        ActivityLog? activityLog = await AssertDb.ActivityLogs
            .Include(l => l.Metadata)
            .SingleOrDefaultAsync(l => l.UserId == seller.Id && l.Type == ActivityLogType.MarketplaceListingCreated);

        Assert.That(activityLog, Is.Not.Null);
        Assert.That(activityLog!.Metadata.Single(m => m.Key == "listingId").Value, Is.Not.Empty);
        Assert.That(activityLog.Metadata.Single(m => m.Key == "listingFee").Value, Is.EqualTo("70"));
        Assert.That(activityLog.Metadata.Single(m => m.Key == "goldFee").Value, Is.EqualTo("5"));
        Assert.That(activityLog.Metadata.Single(m => m.Key == "offer").Value, Does.Contain("\"gold\":100"));
        Assert.That(activityLog.Metadata.Single(m => m.Key == "request").Value, Does.Contain("\"heirloomPoints\":5"));
    }

    [Test]
    public async Task ShouldDeductGoldAndHeirloomPointsFromSellerOnSuccess()
    {
        var constants = new Constants
        {
            MarketplaceActiveListingLimit = 10,
            MarketplaceListingFeePerDay = 10,
            MarketplaceListingDurationDays = 7,
            MarketplaceGoldFeePercent = 10,
        };
        // listingFee = 70, goldFee = floor((0 + 100) * 10 / 100) = 10, reservedGold = 0 + 10 = 10
        // total gold deducted = 70 + 10 = 80, heirloomPoints deducted = 3
        User seller = new() { Gold = 1000, HeirloomPoints = 10 };
        ArrangeDb.Users.Add(seller);
        await ArrangeDb.SaveChangesAsync();

        var result = await new CreateMarketplaceListingCommand.Handler(ActDb, Mapper, constants, new ActivityLogService(new MetadataService()), Mock.Of<IDateTime>()).Handle(new CreateMarketplaceListingCommand
        {
            UserId = seller.Id,
            Offer = new MarketplaceListingAssetInput { HeirloomPoints = 3 },
            Request = new MarketplaceListingAssetInput { Gold = 100 },
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Null);

        User? updatedSeller = await AssertDb.Users.FindAsync(seller.Id);
        Assert.That(updatedSeller!.Gold, Is.EqualTo(1000 - 70 - 10));
        Assert.That(updatedSeller.HeirloomPoints, Is.EqualTo(10 - 3));
    }
}
