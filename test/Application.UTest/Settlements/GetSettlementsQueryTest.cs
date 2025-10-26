using Crpg.Application.Settlements.Queries;
using Crpg.Domain.Entities.Parties;
using Crpg.Domain.Entities.Settlements;
using Crpg.Domain.Entities.Users;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace Crpg.Application.UTest.Settlements;

public class GetSettlementsQueryTest : TestBase
{
    [Test]
    public async Task ShouldReturnAllSettlements()
    {
        Settlement[] settlements =
        {
            new()
            {
                Name = "abc",
                Type = SettlementType.Village,
                Position = new Point(5, 6),
                Scene = "battania_village",
            },
            new()
            {
                Name = "def",
                Type = SettlementType.Castle,
                Position = new Point(7, 8),
                Scene = "sturgia_castle",
            },
        };
        ArrangeDb.Settlements.AddRange(settlements);
        await ArrangeDb.SaveChangesAsync();

        GetSettlementsQuery.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new GetSettlementsQuery(), CancellationToken.None);
        var settlementViews = res.Data!;
        Assert.That(settlementViews, Is.Not.Null);
        Assert.That(settlementViews.Count, Is.EqualTo(2));

        Assert.That(settlementViews[0].Name, Is.EqualTo("abc"));
        Assert.That(settlementViews[1].Name, Is.EqualTo("def"));
        Assert.That(settlementViews[0].Owner, Is.Null);
    }

    [Test]
    public async Task SettlementWithOwnerMapsCorrectly()
    {
        User user = new();
        ArrangeDb.Users.Add(user);
        Party party = new() { User = user };
        ArrangeDb.Parties.Add(party);
        Settlement settlement =
            new()
            {
                Name = "abc",
                Type = SettlementType.Village,
                Position = new Point(5, 6),
                Scene = "battania_village",
                Owner = party,
            };
        ArrangeDb.Settlements.Add(settlement);
        await ArrangeDb.SaveChangesAsync();

        GetSettlementsQuery.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new GetSettlementsQuery(), CancellationToken.None);
        var settlementViews = res.Data!;
        Assert.That(settlementViews, Is.Not.Null);
        Assert.That(settlementViews.Count, Is.EqualTo(1));
        Assert.That(settlementViews[0].Owner, Is.Not.Null);
        Assert.That(settlementViews[0].Owner!.Id, Is.EqualTo(party.User.Id));
    }
}
