using Crpg.Application.Clans.Commands.Armory;
using Crpg.Application.Common.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Clans.Armory;
public class ReturnClanArmoryCommandTest : TestBase
{
    private static readonly Mock<IActivityLogService> ActivityLogService = new() { DefaultValue = DefaultValue.Mock };
    private static readonly Mock<IUserNotificationService> UserNotificationService = new() { DefaultValue = DefaultValue.Mock };
    private static readonly IClanService ClanService = new ClanService(ActivityLogService.Object, UserNotificationService.Object);

    [Test]
    public async Task ShouldReturn()
    {
        await ClanArmoryTestHelper.CommonSetUp(ArrangeDb);
        await ClanArmoryTestHelper.AddItems(ArrangeDb, "user0");
        await ClanArmoryTestHelper.BorrowItems(ArrangeDb, "user1");
        await ArrangeDb.SaveChangesAsync();

        var user = await ActDb.Users
            .Include(u => u.ClanMembership!).ThenInclude(cm => cm.ArmoryBorrowedItems)
            .FirstAsync(u => u.Name == "user1");

        var item = user.ClanMembership!.ArmoryBorrowedItems.First();

        var handler = new ReturnItemToClanArmoryCommand.Handler(ActDb, ClanService);
        var result = await handler.Handle(new ReturnItemToClanArmoryCommand
        {
            UserItemId = item.UserItemId,
            UserId = user.Id,
            ClanId = user.ClanMembership!.ClanId,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Null);

        user = await AssertDb.Users
            .Include(u => u.ClanMembership!).ThenInclude(cm => cm.ArmoryBorrowedItems)
            .Include(u => u.Items)
            .FirstAsync(u => u.Id == user.Id);

        Assert.That(user.ClanMembership!.ArmoryBorrowedItems.Count, Is.EqualTo(0));
        Assert.That(AssertDb.ClanArmoryBorrowedItems.Count(), Is.EqualTo(0));
    }

    [Test]
    public async Task ShouldNotReturnWithWrongUser()
    {
        await ClanArmoryTestHelper.CommonSetUp(ArrangeDb);
        await ClanArmoryTestHelper.AddItems(ArrangeDb, "user0");
        await ClanArmoryTestHelper.BorrowItems(ArrangeDb, "user1");
        await ArrangeDb.SaveChangesAsync();

        var user = await ActDb.Users
            .Include(u => u.ClanMembership)
            .Include(u => u.Items).ThenInclude(ui => ui.ClanArmoryBorrowedItem)
            .FirstAsync(u => u.Name == "user0");

        var item = user.Items.First(ui => ui.ClanArmoryBorrowedItem != null);

        var handler = new ReturnItemToClanArmoryCommand.Handler(ActDb, ClanService);
        var result = await handler.Handle(new ReturnItemToClanArmoryCommand
        {
            UserItemId = item.Id,
            UserId = user.Id,
            ClanId = user.ClanMembership!.ClanId,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Not.Empty);

        user = await AssertDb.Users
            .Include(u => u.ClanMembership!).ThenInclude(cm => cm.ArmoryBorrowedItems)
            .FirstAsync(u => u.Name == "user1");

        Assert.That(AssertDb.ClanArmoryBorrowedItems.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task ShouldNotReturnNotExisting()
    {
        await ClanArmoryTestHelper.CommonSetUp(ArrangeDb);
        await ClanArmoryTestHelper.AddItems(ArrangeDb, "user0");
        await ClanArmoryTestHelper.BorrowItems(ArrangeDb, "user1");
        await ArrangeDb.SaveChangesAsync();

        var user = await ActDb.Users
            .Include(u => u.ClanMembership)
            .Include(u => u.Items).ThenInclude(ui => ui.ClanArmoryItem)
            .FirstAsync(u => u.Name == "user1");

        var handler = new ReturnItemToClanArmoryCommand.Handler(ActDb, ClanService);
        var result = await handler.Handle(new ReturnItemToClanArmoryCommand
        {
            UserItemId = user.Items.First(ui => ui.ClanArmoryItem == null).Id,
            UserId = user.Id,
            ClanId = user.ClanMembership!.ClanId,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Not.Empty);

        user = await AssertDb.Users
            .Include(u => u.ClanMembership!).ThenInclude(cm => cm.ArmoryBorrowedItems)
            .FirstAsync(u => u.Id == user.Id);

        Assert.That(user.ClanMembership!.ArmoryBorrowedItems.Count, Is.EqualTo(1));
        Assert.That(AssertDb.ClanArmoryBorrowedItems.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task ShouldNotReturnNotBorrowed()
    {
        await ClanArmoryTestHelper.CommonSetUp(ArrangeDb);
        await ClanArmoryTestHelper.AddItems(ArrangeDb, "user0");
        await ClanArmoryTestHelper.AddItems(ArrangeDb, "user1");
        await ClanArmoryTestHelper.BorrowItems(ArrangeDb, "user1");
        await ArrangeDb.SaveChangesAsync();

        var user = await ActDb.Users
            .Include(u => u.ClanMembership)
            .Include(u => u.Items).ThenInclude(ui => ui.ClanArmoryItem)
            .FirstAsync(u => u.Name == "user1");

        var item = user.Items.First(ui => ui.ClanArmoryItem != null);

        var handler = new ReturnItemToClanArmoryCommand.Handler(ActDb, ClanService);
        var result = await handler.Handle(new ReturnItemToClanArmoryCommand
        {
            UserItemId = item.Id,
            UserId = user.Id,
            ClanId = user.ClanMembership!.ClanId,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Not.Empty);

        user = await AssertDb.Users
            .Include(u => u.ClanMembership!).ThenInclude(cm => cm.ArmoryBorrowedItems)
            .FirstAsync(u => u.Id == user.Id);

        Assert.That(user.ClanMembership!.ArmoryBorrowedItems.Count, Is.EqualTo(1));
        Assert.That(AssertDb.ClanArmoryBorrowedItems.Count(), Is.EqualTo(1));
    }
}
