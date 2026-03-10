using AutoMapper;
using Crpg.Application.Battles.Models;
using Crpg.Application.Battles.Queries;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Settlements;
using Crpg.Domain.Entities.Terrains;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Battles;

public class GetBattleQueryTest : TestBase
{
    [Test]
    public async Task ShouldReturnErrorIfBattleNotFound()
    {
        GetBattleQuery.Handler handler = new(ActDb, Mapper, Mock.Of<IBattleService>());
        var res = await handler.Handle(new GetBattleQuery
        {
            BattleId = 99,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.BattleNotFound));
    }

    // TODO: FIXME: TEMP
    // [Test]
    // public async Task ShouldReturnErrorIfBattleInPreparation()
    // {
    //     Battle battle = new() { Phase = BattlePhase.Preparation };
    //     ArrangeDb.Battles.Add(battle);
    //     await ArrangeDb.SaveChangesAsync();

    // GetBattleQuery.Handler handler = new(ActDb, Mapper, Mock.Of<IBattleService>());
    //     var res = await handler.Handle(new GetBattleQuery
    //     {
    //         BattleId = battle.Id,
    //     }, CancellationToken.None);

    // Assert.That(res.Errors, Is.Not.Null);
    //     Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.BattleInvalidPhase));
    // }

    [Test]
    public async Task ShouldGetTheBattle()
    {
        Battle battle = new()
        {
            Region = Region.Na,
            Phase = BattlePhase.Hiring,
        };
        ArrangeDb.Battles.Add(battle);
        await ArrangeDb.SaveChangesAsync();

        Mock<IBattleService> battleService = new();
        battleService.Setup(s => s.MapBattleToDetailedViewModel(
            It.IsAny<IMapper>(),
            It.IsAny<Battle>(),
            It.IsAny<int>(),
            It.IsAny<Settlement>(),
            It.IsAny<Terrain>())).Returns(new BattleDetailedViewModel()
            {
                Id = battle.Id,
            });

        GetBattleQuery.Handler handler = new(ActDb, Mapper, battleService.Object);
        var res = await handler.Handle(new GetBattleQuery
        {
            BattleId = battle.Id,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        Assert.That(res.Data!.Id, Is.EqualTo(battle.Id));
    }
}
