using Crpg.Application.Items.Queries;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Items;

public class UserItemPresetsQueriesTest : TestBase
{
    [Test]
    public async Task GetAndListAreScopedByUser()
    {
        var user1 = ArrangeDb.Users.Add(new User()).Entity;
        var user2 = ArrangeDb.Users.Add(new User()).Entity;
        ArrangeDb.UserItemPresets.AddRange(
            new UserItemPreset
            {
                UserId = user1.Id,
                Name = "u1",
            },
            new UserItemPreset
            {
                UserId = user2.Id,
                Name = "u2",
            });
        await ArrangeDb.SaveChangesAsync();

        var listResult = await new GetUserItemPresetsQuery.Handler(ActDb, Mapper)
            .Handle(new GetUserItemPresetsQuery { UserId = user1.Id }, CancellationToken.None);

        Assert.That(listResult.Errors, Is.Null);
        Assert.That(listResult.Data!.Count, Is.EqualTo(1));
        Assert.That(listResult.Data[0].Name, Is.EqualTo("u1"));
    }
}
