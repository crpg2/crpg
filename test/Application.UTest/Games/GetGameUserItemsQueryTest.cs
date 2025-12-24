using Crpg.Application.Games.Queries;
using Crpg.Application.Items.Queries;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Games;

public class GetGameUserItemsQueryTest : TestBase
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

        var result = await new GetGameUserItemsQuery.Handler(ActDb, Mapper).Handle(
            new GetGameUserItemsQuery { UserId = user.Entity.Id }, CancellationToken.None);

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

        var result = await new GetGameUserItemsQuery.Handler(ActDb, Mapper).Handle(
            new GetGameUserItemsQuery { UserId = user.Entity.Id }, CancellationToken.None);

        Assert.That(result.Data!.Count, Is.EqualTo(3));
    }
}
