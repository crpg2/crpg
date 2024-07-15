using Crpg.Application.Clans.Commands.Armory;
using Crpg.Application.Common.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Clans.Armory;
public class RemoveClanArmoryCommandTest : TestBase
{
    private static readonly Mock<IActivityLogService> ActivityLogService = new() { DefaultValue = DefaultValue.Mock };
    private static readonly Mock<IUserNotificationService> UserNotificationService = new() { DefaultValue = DefaultValue.Mock };
    private static readonly IClanService ClanService = new ClanService(ActivityLogService.Object, UserNotificationService.Object);

    [Test]
    public async Task ShouldRemove()
    {
        await ClanArmoryTestHelper.CommonSetUp(ArrangeDb);
        await ClanArmoryTestHelper.AddItems(ArrangeDb, "user0");
        await ArrangeDb.SaveChangesAsync();

        var user = await ActDb.Users
            .Include(u => u.Items).ThenInclude(ui => ui.ClanArmoryItem)
            .Include(u => u.ClanMembership).
            FirstAsync(u => u.Name == "user0");

        var handler = new RemoveItemFromClanArmoryCommand.Handler(ActDb, ClanService);
        var result = await handler.Handle(new RemoveItemFromClanArmoryCommand
        {
            UserItemId = user.Items.First(ui => ui.ClanArmoryItem != null).Id,
            UserId = user.Id,
            ClanId = user.ClanMembership!.ClanId,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Null);

        user = await AssertDb.Users
            .Include(u => u.ClanMembership!).ThenInclude(cm => cm.ArmoryItems)
            .FirstAsync(u => u.Id == user.Id);
        Assert.That(user.ClanMembership!.ArmoryItems.Count, Is.EqualTo(0));

        Assert.That(AssertDb.ClanArmoryItems.Count(), Is.EqualTo(0));
    }

    [Test]
    public async Task ShouldNotRemoveWithWrongUser()
    {
        await ClanArmoryTestHelper.CommonSetUp(ArrangeDb);
        await ClanArmoryTestHelper.AddItems(ArrangeDb, "user0");
        await ArrangeDb.SaveChangesAsync();

        var user = await ActDb.Users
            .Include(u => u.ClanMembership!)
            .FirstAsync(u => u.Name == "user1");

        var clan = ActDb.Clans
            .Include(c => c.Members).ThenInclude(cm => cm.ArmoryItems)
            .First(c => c.Id == user.ClanMembership!.ClanId);

        var items = clan.Members.SelectMany(cm => cm.ArmoryItems);
        int expectedCount = items.Count();

        var handler = new RemoveItemFromClanArmoryCommand.Handler(ActDb, ClanService);
        var result = await handler.Handle(new RemoveItemFromClanArmoryCommand
        {
            UserItemId = items.First().UserItemId,
            UserId = user.Id,
            ClanId = clan.Id,
        }, CancellationToken.None);
        Assert.That(result.Errors, Is.Not.Empty);

        Assert.That(AssertDb.ClanArmoryItems.Count(), Is.EqualTo(expectedCount));
    }

    [Test]
    public async Task ShouldNotRemoveNotExisting()
    {
        await ClanArmoryTestHelper.CommonSetUp(ArrangeDb);
        await ClanArmoryTestHelper.AddItems(ArrangeDb, "user0");
        await ArrangeDb.SaveChangesAsync();

        var user = await ActDb.Users
            .Include(u => u.Items).ThenInclude(ui => ui.ClanArmoryItem)
            .Include(u => u.ClanMembership)
            .FirstAsync(u => u.Name == "user0");

        var handler = new RemoveItemFromClanArmoryCommand.Handler(ActDb, ClanService);
        var result = await handler.Handle(new RemoveItemFromClanArmoryCommand
        {
            UserItemId = user.Items.First(ui => ui.ClanArmoryItem == null).Id,
            UserId = user.Id,
            ClanId = user.ClanMembership!.ClanId,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Not.Empty);

        Assert.That(AssertDb.ClanArmoryItems.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task ShouldRemoveBorrowed()
    {
        await ClanArmoryTestHelper.CommonSetUp(ArrangeDb);
        await ClanArmoryTestHelper.AddItems(ArrangeDb, "user0");
        await ClanArmoryTestHelper.BorrowItems(ArrangeDb, "user1");
        await ArrangeDb.SaveChangesAsync();

        var user = await ActDb.Users
            .Include(u => u.ClanMembership!).ThenInclude(cm => cm.ArmoryItems).ThenInclude(ci => ci.BorrowedItem)
            .FirstAsync(u => u.Name == "user0");

        var item = user.ClanMembership!.ArmoryItems.First(ci => ci.BorrowedItem != null);

        var handler = new RemoveItemFromClanArmoryCommand.Handler(ActDb, ClanService);
        var result = await handler.Handle(new RemoveItemFromClanArmoryCommand
        {
            UserItemId = item.UserItemId,
            UserId = user.Id,
            ClanId = user.ClanMembership!.ClanId,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Null);

        user = await AssertDb.Users
            .Include(u => u.ClanMembership!).ThenInclude(cm => cm.ArmoryBorrowedItems)
            .Where(u => u.Name == "user1")
            .FirstAsync();
        Assert.That(user.ClanMembership!.ArmoryBorrowedItems.Count, Is.EqualTo(0));

        Assert.That(AssertDb.ClanArmoryBorrowedItems.Count(), Is.EqualTo(0));
    }
}
