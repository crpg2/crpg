using Crpg.Application.Battles.Queries;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Battles;

public class GetBattleParticipantsQueryTest : TestBase
{
    [Test]
    public async Task ShouldReturnErrorIfBattleNotFound()
    {
        GetBattleParticipantsQuery.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new GetBattleParticipantsQuery
        {
            BattleId = 99,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.BattleNotFound));
    }

    [Test]
    public async Task ShouldReturnErrorIfBattleIsInPreparation()
    {
        Battle battle = new() { Phase = BattlePhase.Preparation };
        ArrangeDb.Battles.Add(battle);
        await ArrangeDb.SaveChangesAsync();

        GetBattleParticipantsQuery.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new GetBattleParticipantsQuery
        {
            BattleId = battle.Id,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.BattleInvalidPhase));
    }

    [Test]
    public async Task ShouldOnlyReturnBothSidesDuringOtherPhases()
    {
        Battle battle = new()
        {
            Phase = BattlePhase.Hiring,
            Participants =
            {
                new()
                {
                    Character = new Character { User = new User() },
                    Side = BattleSide.Attacker,
                },
                new()
                {
                    Character = new Character { User = new User() },
                    Side = BattleSide.Defender,
                },
            },
        };

        Battle battle2 = new()
        {
            Phase = BattlePhase.Hiring,
            Participants =
            {
                new()
                {
                    Character = new Character { User = new User() },
                    Side = BattleSide.Attacker,
                },
                new()
                {
                    Character = new Character { User = new User() },
                    Side = BattleSide.Defender,
                },
            },
        };
        ArrangeDb.Battles.AddRange([battle, battle2]);
        await ArrangeDb.SaveChangesAsync();

        GetBattleParticipantsQuery.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new GetBattleParticipantsQuery
        {
            BattleId = battle.Id,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        var mercenaries = res.Data!;
        Assert.That(mercenaries.Count, Is.EqualTo(2));
    }
}
