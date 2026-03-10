using AutoMapper;
using Crpg.Application.Battles.Models;
using Crpg.Application.Battles.Queries;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Parties;
using Crpg.Domain.Entities.Settlements;
using Crpg.Domain.Entities.Terrains;
using Crpg.Domain.Entities.Users;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Battles;

public class GetBattlesQueryTest : TestBase
{
    [Test]
    public async Task ShouldGetBattlesMatchingThePhases()
    {
        Battle[] battles =
        [
            new()
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
            },
            new()
            {
                Region = Region.Na,
                Phase = BattlePhase.Live,
                Fighters =
                {
                    new BattleFighter
                    {
                        Side = BattleSide.Attacker,
                        Commander = true,
                        Party = new Party { Troops = 100.5f, User = new User() },
                    },
                    new BattleFighter
                    {
                        Side = BattleSide.Defender,
                        Commander = true,
                        Settlement = new Settlement
                        {
                            Name = "toto",
                            Troops = 12,
                        },
                    },
                    new BattleFighter
                    {
                        Side = BattleSide.Defender,
                        Commander = false,
                        Party = new Party { Troops = 35.6f, User = new User() },
                    },
                },
            },
            new() { Region = Region.Na, Phase = BattlePhase.Preparation },
            new() { Region = Region.Eu, Phase = BattlePhase.Hiring },
            new() { Region = Region.As, Phase = BattlePhase.Live },
            new() { Region = Region.Na, Phase = BattlePhase.End },
        ];
        ArrangeDb.Battles.AddRange(battles);
        await ArrangeDb.SaveChangesAsync();

        Mock<IBattleService> battleService = new();
        battleService.Setup(s => s.MapBattleToDetailedViewModel(
            It.IsAny<IMapper>(),
            It.IsAny<Battle>(),
            It.IsAny<int>(),
            It.IsAny<Settlement>(),
            It.IsAny<Terrain>())).Returns<IMapper, Battle, int, Settlement, Terrain>((mapper, battle, userId, settlement, terrain) => new BattleDetailedViewModel()
            {
                Id = battle.Id,
                Region = battle.Region,
                Phase = battle.Phase,
            });

        GetBattlesQuery.Handler handler = new(ActDb, Mapper, battleService.Object);
        var res = await handler.Handle(new GetBattlesQuery
        {
            Region = Region.Na,
            Phases = [BattlePhase.Hiring, BattlePhase.Live],
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);

        var battlesVm = res.Data!;
        Assert.That(battlesVm.Count, Is.EqualTo(2));

        Assert.That(battlesVm[0].Region, Is.EqualTo(Region.Na));
        Assert.That(battlesVm[0].Phase, Is.EqualTo(BattlePhase.Hiring));

        Assert.That(battlesVm[1].Region, Is.EqualTo(Region.Na));
        Assert.That(battlesVm[1].Phase, Is.EqualTo(BattlePhase.Live));
    }
}
