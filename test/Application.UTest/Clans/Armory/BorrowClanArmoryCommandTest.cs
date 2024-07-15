using Crpg.Application.Clans.Commands.Armory;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Items;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Clans.Armory;
public class BorrowClanArmoryCommandTest : TestBase
{
    private static readonly Mock<IActivityLogService> ActivityLogService = new() { DefaultValue = DefaultValue.Mock };
    private static readonly Mock<IUserNotificationService> UserNotificationService = new() { DefaultValue = DefaultValue.Mock };
    private static readonly IClanService ClanService = new ClanService(ActivityLogService.Object, UserNotificationService.Object);

    [Test]
    public async Task ShouldBorrow()
    {
        await ClanArmoryTestHelper.CommonSetUp(ArrangeDb);
        await ClanArmoryTestHelper.AddItems(ArrangeDb, "user0");
        await ArrangeDb.SaveChangesAsync();

        var user = await ActDb.Users
            .Include(u => u.ClanMembership)
            .FirstAsync(u => u.Name == "user1");

        var clan = await ActDb.Clans
            .Include(c => c.Members).ThenInclude(cm => cm.ArmoryItems)
            .FirstAsync(c => c.Id == user.ClanMembership!.ClanId);

        var item = clan.Members.First(cm => cm.ArmoryItems.Count > 0).ArmoryItems.First();

        var handler = new BorrowItemFromClanArmoryCommand.Handler(ActDb, Mapper, ClanService);
        var result = await handler.Handle(new BorrowItemFromClanArmoryCommand
        {
            UserItemId = item.UserItemId,
            UserId = user.Id,
            ClanId = clan.Id,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Null);

        user = await AssertDb.Users
            .Include(u => u.ClanMembership!).ThenInclude(cm => cm.ArmoryBorrowedItems)
            .FirstAsync(u => u.Id == user.Id);

        Assert.That(user.ClanMembership!.ArmoryBorrowedItems.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task ShouldNotBorrowItemInUse()
    {
        await ClanArmoryTestHelper.CommonSetUp(ArrangeDb);
        await ClanArmoryTestHelper.AddItems(ArrangeDb, "user0");
        await ClanArmoryTestHelper.BorrowItems(ArrangeDb, "user1");
        await ArrangeDb.SaveChangesAsync();

        var user = await ActDb.Users
            .Include(u => u.ClanMembership)
            .FirstAsync(u => u.Name == "user2");

        var clan = await ActDb.Clans
            .Include(c => c.Members).ThenInclude(cm => cm.ArmoryItems)
            .FirstAsync(c => c.Id == user.ClanMembership!.ClanId);

        var item = clan.Members.First(cm => cm.ArmoryItems.Count > 0).ArmoryItems.First();

        var handler = new BorrowItemFromClanArmoryCommand.Handler(ActDb, Mapper, ClanService);
        var result = await handler.Handle(new BorrowItemFromClanArmoryCommand
        {
            UserItemId = item.UserItemId,
            UserId = user.Id,
            ClanId = clan.Id,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Not.Null);

        user = await AssertDb.Users
            .Include(u => u.ClanMembership!).ThenInclude(cm => cm.ArmoryBorrowedItems)
            .FirstAsync(u => u.Id == user.Id);

        Assert.That(user.ClanMembership!.ArmoryBorrowedItems.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task ShouldNotBorrowWithWrongClan()
    {
        await ClanArmoryTestHelper.CommonSetUp(ArrangeDb);
        await ClanArmoryTestHelper.AddItems(ArrangeDb, "user0");
        await ArrangeDb.SaveChangesAsync();

        var user = await ActDb.Users
            .Include(u => u.ClanMembership)
            .FirstAsync(u => u.Name == "user1");

        var clan = await ActDb.Clans
            .Include(c => c.Members).ThenInclude(ci => ci.ArmoryItems)
            .FirstAsync(c => c.Id == user.ClanMembership!.ClanId);

        var item = clan.Members.First(cm => cm.ArmoryItems.Count > 0).ArmoryItems.First();

        var handler = new BorrowItemFromClanArmoryCommand.Handler(ActDb, Mapper, ClanService);
        var result = await handler.Handle(new BorrowItemFromClanArmoryCommand
        {
            UserItemId = item.UserItemId,
            UserId = user.Id,
            ClanId = clan.Id + 1,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Not.Null);

        user = await AssertDb.Users
             .Include(u => u.ClanMembership!).ThenInclude(cm => cm.ArmoryBorrowedItems)
             .FirstAsync(u => u.Id == user.Id);

        Assert.That(user.ClanMembership!.ArmoryBorrowedItems.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task ShouldNotBorrowExistingItem()
    {
        await ClanArmoryTestHelper.CommonSetUp(ArrangeDb);
        await ClanArmoryTestHelper.AddItems(ArrangeDb, "user0");
        await ArrangeDb.SaveChangesAsync();

        var user = await ArrangeDb.Users
            .Include(u => u.Items)
            .Include(u => u.ClanMembership)
            .FirstAsync(u => u.Name == "user1");

        var clan = await ArrangeDb.Clans
            .Include(c => c.Members).ThenInclude(cm => cm.ArmoryItems).ThenInclude(ci => ci.UserItem)
            .FirstAsync(c => c.Id == user.ClanMembership!.ClanId);

        var item = clan.Members.First(cm => cm.ArmoryItems.Count > 0).ArmoryItems.First().UserItem!;
        user.Items.Add(new UserItem { ItemId = item.ItemId });
        await ArrangeDb.SaveChangesAsync();

        user = await ActDb.Users
            .Include(u => u.ClanMembership)
            .FirstAsync(u => u.Id == user.Id);

        var handler = new BorrowItemFromClanArmoryCommand.Handler(ActDb, Mapper, ClanService);
        var result = await handler.Handle(new BorrowItemFromClanArmoryCommand
        {
            UserItemId = item.Id,
            UserId = user.Id,
            ClanId = clan.Id,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Not.Empty);

        user = await AssertDb.Users
             .Include(u => u.ClanMembership!).ThenInclude(cm => cm.ArmoryBorrowedItems)
             .FirstAsync(u => u.Id == user.Id);

        Assert.That(user.ClanMembership!.ArmoryBorrowedItems.Count, Is.EqualTo(0));
    }
}
