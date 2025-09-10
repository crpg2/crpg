using Crpg.Application.Battles.Queries;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Parties;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Battles;

public class GetBattleQueryTest : TestBase
{
    [Test]
    public async Task ShouldReturnErrorIfBattleNotFound()
    {
        GetBattleQuery.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new GetBattleQuery
        {
            BattleId = 99,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.BattleNotFound));
    }

    //[Test]
    //public async Task ShouldReturnErrorIfBattleInPreparation()
    //{
    //    Battle battle = new() { Phase = BattlePhase.Preparation };
    //    ArrangeDb.Battles.Add(battle);
    //    await ArrangeDb.SaveChangesAsync();

    //    GetBattleQuery.Handler handler = new(ActDb, Mapper);
    //    var res = await handler.Handle(new GetBattleQuery
    //    {
    //        BattleId = battle.Id,
    //    }, CancellationToken.None);

    //    Assert.That(res.Errors, Is.Not.Null);
    //    Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.BattleInvalidPhase));
    //}

    [Test]
    public async Task ShouldGetTheBattle()
    {
        Battle battle = new()
        {
            Region = Region.Na,
            Phase = BattlePhase.Hiring,
            Fighters =
            {
                new BattleFighter
                {
                    Side = BattleSide.Attacker,
                    Commander = true,
                    Party = new Party { Troops = 20.9f, User = new User() },
                },
                new BattleFighter
                {
                    Side = BattleSide.Attacker,
                    Commander = false,
                    Party = new Party { Troops = 15.8f, User = new User() },
                },
                new BattleFighter
                {
                    Side = BattleSide.Defender,
                    Commander = false,
                    Party = new Party { Troops = 35.7f, User = new User() },
                },
                new BattleFighter
                {
                    Side = BattleSide.Defender,
                    Commander = true,
                    Party = new Party { Troops = 10.6f, User = new User() },
                },
            },
        };

        ArrangeDb.Battles.Add(battle);
        await ArrangeDb.SaveChangesAsync();

        GetBattleQuery.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new GetBattleQuery
        {
            BattleId = battle.Id,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        Assert.That(res.Data!.Id, Is.EqualTo(battle.Id));
    }
}
