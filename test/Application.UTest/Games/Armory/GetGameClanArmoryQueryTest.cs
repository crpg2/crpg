using Crpg.Application.Clans.Queries;
using Crpg.Application.Common.Services;
using Crpg.Application.Games.Queries;
using Crpg.Application.UTest.Clans.Armory;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Games.Armory;
public class GetGameClanArmoryQueryTest : TestBase
{
    private static readonly Mock<IActivityLogService> ActivityLogService = new() { DefaultValue = DefaultValue.Mock };
    private static readonly Mock<IUserNotificationService> UserNotificationsService = new() { DefaultValue = DefaultValue.Mock };
    private IClanService ClanService { get; } = new ClanService(ActivityLogService.Object, UserNotificationsService.Object);

    [Test]
    public async Task ShouldGetClanArmoryItems()
    {
        await ClanArmoryTestHelper.CommonSetUp(ArrangeDb);
        int count = 2;
        await ClanArmoryTestHelper.AddItems(ArrangeDb, "user0", count);
        await ClanArmoryTestHelper.BorrowItems(ArrangeDb, "user1", count);
        await ArrangeDb.SaveChangesAsync();

        var user = await ActDb.Users
            .Include(u => u.ClanMembership)
            .FirstAsync(u => u.Name == "user1");

        var handler = new GetGameClanArmoryQuery.Handler(ActDb, Mapper, ClanService);
        var result = await handler.Handle(new GetGameClanArmoryQuery
        {
            UserId = user.Id,
            ClanId = user.ClanMembership!.ClanId,
        }, CancellationToken.None);

        var items = result.Data!;
        var item = items.First();

        Assert.That(result.Errors, Is.Null);
        Assert.That(items.Count, Is.EqualTo(count));
        Assert.That(item.UserItem, Is.Not.Null);
        Assert.That(item.UserItem!.ItemId, Is.Not.Null);
        Assert.That(item.BorrowedItem, Is.Not.Null);
    }
}
