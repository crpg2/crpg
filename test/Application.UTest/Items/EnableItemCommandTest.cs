using Crpg.Application.Common;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Items.Commands;
using Crpg.Application.Marketplace.Services;
using Crpg.Domain.Entities.ActivityLogs;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Notifications;
using Crpg.Domain.Entities.Users;
using Crpg.Sdk.Abstractions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using static Crpg.Application.UTest.Marketplace.MarketplaceOfferFactory;

namespace Crpg.Application.UTest.Items;

public class EnableItemCommandTest : TestBase
{
    [Test]
    public async Task ShouldReturnErrorIfItemIsNotFound()
    {
        User user = new();
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        var result = await new EnableItemCommand.Handler(ActDb, new MarketplaceService(), new ActivityLogService(new MetadataService()), Mock.Of<IItemService>(), new UserNotificationService(new MetadataService())).Handle(new EnableItemCommand
        {
            BaseItemId = "a",
            Enable = true,
            UserId = user.Id,
        }, CancellationToken.None);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.ItemNotFound));
    }

    [Test]
    public async Task ShouldEnableItem()
    {
        Item item = new() { Id = "a", BaseId = "a", Enabled = false };
        ArrangeDb.Items.Add(item);
        User user = new();
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        var result = await new EnableItemCommand.Handler(ActDb, new MarketplaceService(), new ActivityLogService(new MetadataService()), Mock.Of<IItemService>(), new UserNotificationService(new MetadataService())).Handle(new EnableItemCommand
        {
            BaseItemId = item.Id,
            Enable = true,
            UserId = user.Id,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Null);
        var dbItem = AssertDb.Items.First(i => i.Id == "a");
        Assert.That(dbItem.Enabled, Is.True);
    }

    [Test]
    public async Task ShouldDisableItem()
    {
        Item item = new() { Id = "a", BaseId = "a", Enabled = false };
        ArrangeDb.Items.Add(item);

        User user0 = new();
        UserItem user0Item = new() { Item = item };
        user0.Items.Add(user0Item);
        Character user0Character = new()
        {
            EquippedItems =
            {
                new EquippedItem { UserItem = user0Item },
            },
        };
        user0.Characters.Add(user0Character);

        User user1 = new();
        UserItem user1Item = new() { Item = item };
        user1.Items.Add(user1Item);
        Character user1Character = new()
        {
            EquippedItems =
            {
                new EquippedItem { Slot = ItemSlot.Head, UserItem = user1Item },
                new EquippedItem { Slot = ItemSlot.Body, UserItem = new UserItem { Item = new Item { Id = "b" } } },
            },
        };
        user1.Characters.Add(user1Character);
        ArrangeDb.Users.AddRange(user0, user1);
        await ArrangeDb.SaveChangesAsync();

        var result = await new EnableItemCommand.Handler(ActDb, new MarketplaceService(), new ActivityLogService(new MetadataService()), Mock.Of<IItemService>(), new UserNotificationService(new MetadataService())).Handle(new EnableItemCommand
        {
            BaseItemId = item.Id,
            Enable = false,
            UserId = user0.Id,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Null);
        var dbItem = AssertDb.Items.First(i => i.Id == "a");
        Assert.That(dbItem.Enabled, Is.False);

        var equippedItems = await AssertDb.EquippedItems.Include(ei => ei.UserItem).ToArrayAsync();
        Assert.That(equippedItems.Length, Is.EqualTo(1));
        Assert.That(equippedItems[0].UserItem!.ItemId, Is.EqualTo("b"));
    }

    [Test]
    public async Task ShouldCancelOffersWithItemOffered()
    {
        Item item = new() { Id = "a", BaseId = "a", Enabled = true };
        ArrangeDb.Items.Add(item);

        User seller = new() { Gold = 100 };
        UserItem sellerItem = new() { Item = item };
        seller.Items.Add(sellerItem);
        ArrangeDb.Users.Add(seller);
        await ArrangeDb.SaveChangesAsync();

        var offer = CreateOffer(seller.Id, goldFee: 10, offeredUserItemId: sellerItem.Id, requestedGold: 50);
        ArrangeDb.MarketplaceOffers.Add(offer);
        await ArrangeDb.SaveChangesAsync();

        var result = await new EnableItemCommand.Handler(ActDb, new MarketplaceService(), new ActivityLogService(new MetadataService()), Mock.Of<IItemService>(), new UserNotificationService(new MetadataService())).Handle(new EnableItemCommand
        {
            BaseItemId = item.Id,
            Enable = false,
            UserId = seller.Id,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Null);
        Assert.That(AssertDb.MarketplaceOffers.Count(), Is.EqualTo(0));

        var notification = await AssertDb.UserNotifications
            .Include(n => n.Metadata)
            .SingleOrDefaultAsync(n => n.UserId == seller.Id && n.Type == NotificationType.MarketplaceOfferInvalidated);
        Assert.That(notification, Is.Not.Null);

        var activityLog = await AssertDb.ActivityLogs
            .Include(l => l.Metadata)
            .SingleOrDefaultAsync(l => l.UserId == seller.Id && l.Type == ActivityLogType.MarketplaceOfferInvalidated);
        Assert.That(activityLog, Is.Not.Null);
    }

    [Test]
    public async Task ShouldCancelOffersWithItemRequested()
    {
        Item item = new() { Id = "a", BaseId = "a", Enabled = true };
        ArrangeDb.Items.Add(item);

        User seller = new() { Gold = 100 };
        ArrangeDb.Users.Add(seller);
        await ArrangeDb.SaveChangesAsync();

        var offer = CreateOffer(seller.Id, goldFee: 5, offeredGold: 80, requestedItemId: item.Id);
        ArrangeDb.MarketplaceOffers.Add(offer);
        await ArrangeDb.SaveChangesAsync();

        var result = await new EnableItemCommand.Handler(ActDb, new MarketplaceService(), new ActivityLogService(new MetadataService()), Mock.Of<IItemService>(), new UserNotificationService(new MetadataService())).Handle(new EnableItemCommand
        {
            BaseItemId = item.Id,
            Enable = false,
            UserId = seller.Id,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Null);
        Assert.That(AssertDb.MarketplaceOffers.Count(), Is.EqualTo(0));

        var notification = await AssertDb.UserNotifications
            .Include(n => n.Metadata)
            .SingleOrDefaultAsync(n => n.UserId == seller.Id && n.Type == NotificationType.MarketplaceOfferInvalidated);
        Assert.That(notification, Is.Not.Null);

        var activityLog = await AssertDb.ActivityLogs
            .Include(l => l.Metadata)
            .SingleOrDefaultAsync(l => l.UserId == seller.Id && l.Type == ActivityLogType.MarketplaceOfferInvalidated);
        Assert.That(activityLog, Is.Not.Null);
    }

    [Test]
    public async Task ShouldRefundUsersItemsWhenDisabling()
    {
        Item itemRank0 = new() { Id = "a_rank0", BaseId = "a", Enabled = true, Price = 100, Rank = 0 };
        Item itemRank1 = new() { Id = "a_rank1", BaseId = "a", Enabled = true, Price = 200, Rank = 1 };
        ArrangeDb.Items.AddRange(itemRank0, itemRank1);

        User user = new();
        ArrangeDb.Users.Add(user);
        UserItem userItemRank0 = new() { User = user, Item = itemRank0 };
        UserItem userItemRank1 = new() { User = user, Item = itemRank1 };
        ArrangeDb.UserItems.AddRange(userItemRank0, userItemRank1);
        await ArrangeDb.SaveChangesAsync();

        var result = await new EnableItemCommand.Handler(ActDb, new MarketplaceService(), new ActivityLogService(new MetadataService()), new ItemService(Mock.Of<IDateTime>(), new Constants()), new UserNotificationService(new MetadataService())).Handle(new EnableItemCommand
        {
            BaseItemId = "a",
            Enable = false,
            UserId = user.Id,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Null);
        Assert.That(AssertDb.UserItems.Any(), Is.False);
        user = AssertDb.Users.First(u => u.Id == user.Id);
        Assert.That(user.Gold, Is.EqualTo(300)); // 100 + 200
        Assert.That(user.HeirloomPoints, Is.EqualTo(1)); // only rank1 contributes
    }
}
