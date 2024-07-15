using Crpg.Application.Clans.Queries;
using Crpg.Application.Common.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Clans.Armory;
public class GetClanArmoryQueryTest : TestBase
{
    private static readonly IClanService ClanService = Mock.Of<IClanService>();

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

        var handler = new GetClanArmoryQuery.Handler(ActDb, Mapper, ClanService);
        var result = await handler.Handle(new GetClanArmoryQuery
        {
            UserId = user.Id,
            ClanId = user.ClanMembership!.ClanId,
        }, CancellationToken.None);

        var items = result.Data!;
        var item = items.First();

        Assert.That(result.Errors, Is.Null);
        Assert.That(items.Count, Is.EqualTo(count));
        Assert.That(item.UserItem, Is.Not.Null);
        Assert.That(item.UserItem!.Item, Is.Not.Null);
        Assert.That(item.BorrowedItem, Is.Not.Null);
    }
}
