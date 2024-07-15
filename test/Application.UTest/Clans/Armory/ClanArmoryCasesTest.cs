using Crpg.Application.Common.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Clans.Armory;

public class ClanArmoryCasesTest : TestBase
{
    private static readonly Mock<IActivityLogService> ActivityLogService = new() { DefaultValue = DefaultValue.Mock };
    private static readonly Mock<IUserNotificationService> UserNotificationService = new() { DefaultValue = DefaultValue.Mock };
    private static readonly IClanService ClanService = new ClanService(ActivityLogService.Object, UserNotificationService.Object);

    [Test]
    public async Task ShouldCascadeOnClanLeave()
    {
        await ClanArmoryTestHelper.CommonSetUp(ArrangeDb);
        await ClanArmoryTestHelper.AddItems(ArrangeDb, "user0");
        await ClanArmoryTestHelper.BorrowItems(ArrangeDb, "user1");

        var user = await ActDb.Users
            .Include(u => u.ClanMembership)
            .Where(u => u.Name == "user0")
            .FirstAsync();

        var result = await ClanService.LeaveClan(ActDb, user.ClanMembership!, CancellationToken.None);
        Assert.That(result.Errors, Is.Null.Or.Empty);
        await ActDb.SaveChangesAsync();

        Assert.That(AssertDb.ClanArmoryBorrowedItems.Count(), Is.EqualTo(0));
        Assert.That(AssertDb.ClanArmoryItems.Count(), Is.EqualTo(0));

        user = await AssertDb.Users
            .Include(u => u.ClanMembership!).ThenInclude(cm => cm.ArmoryBorrowedItems)
            .Include(u => u.ClanMembership!).ThenInclude(cm => cm.ArmoryItems)
            .Where(u => u.Name == "user0")
            .FirstAsync();

        Assert.That(user.ClanMembership, Is.Null);

        user = await AssertDb.Users
            .Include(u => u.ClanMembership!).ThenInclude(cm => cm.ArmoryItems)
            .Include(u => u.ClanMembership!).ThenInclude(cm => cm.ArmoryBorrowedItems)
            .Include(u => u.Items)
            .Where(u => u.Name == "user1")
            .FirstAsync();

        Assert.That(user.ClanMembership, Is.Not.Null);
        Assert.That(user.ClanMembership!.ArmoryItems.Count, Is.EqualTo(0));
        Assert.That(user.ClanMembership!.ArmoryBorrowedItems.Count, Is.EqualTo(0));
    }
}
