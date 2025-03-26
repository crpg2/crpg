using Crpg.Application.Battles.Commands;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Parties;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace Crpg.Application.UTest.Battles;

public class ScheduleBattleCommandTest : TestBase
{
    [Test]
    public async Task ShouldReturnErrorIfPartyNotFound()
    {
        ScheduleBattleCommand.Handler handler = new(ActDb, Mapper, Mock.Of<IBattleScheduler>());
        var res = await handler.Handle(new()
        {
            PartyId = 1,
            BattleId = 2,
            Hour = 3,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.PartyNotFound));
    }

    [Test]
    public async Task ShouldReturnErrorIfBattleNotFound()
    {
        Party party = new() { User = new User() };
        ArrangeDb.Parties.Add(party);
        await ArrangeDb.SaveChangesAsync();

        ScheduleBattleCommand.Handler handler = new(ActDb, Mapper, Mock.Of<IBattleScheduler>());
        var res = await handler.Handle(new()
        {
            PartyId = party.Id,
            BattleId = 2,
            Hour = 3,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.BattleNotFound));
    }

    [Test]
    public async Task ShouldReturnErrorIfFighterNotCommander()
    {
        Party party = new()
        {
            Status = PartyStatus.Idle,
            Position = new Point(1, 2),
            User = new User(),
        };
        ArrangeDb.Parties.Add(party);
        Battle battle = new()
        {
            Fighters = new()
            {
                new()
                {
                    Side = BattleSide.Attacker,
                    Party = party,
                    Commander = false,
                },
            },
            Phase = BattlePhase.Preparation,
        };
        ArrangeDb.Battles.Add(battle);
        await ArrangeDb.SaveChangesAsync();

        ScheduleBattleCommand.Handler handler = new(ActDb, Mapper, Mock.Of<IBattleScheduler>());
        var res = await handler.Handle(new()
        {
            PartyId = party.Id,
            BattleId = battle.Id,
            Hour = 3,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.FighterNotACommander));
    }

    [Test]
    public async Task ShouldReturnErrorIfBattleNotInPreparation()
    {
        Party party = new()
        {
            Status = PartyStatus.Idle,
            Position = new Point(1, 2),
            User = new User(),
        };
        ArrangeDb.Parties.Add(party);
        Battle battle = new()
        {
            Fighters = new()
            {
                new()
                {
                    Side = BattleSide.Attacker,
                    Party = party,
                    Commander = true,
                },
            },
            Phase = BattlePhase.Hiring,
        };
        ArrangeDb.Battles.Add(battle);
        await ArrangeDb.SaveChangesAsync();

        ScheduleBattleCommand.Handler handler = new(ActDb, Mapper, Mock.Of<IBattleScheduler>());
        var res = await handler.Handle(new()
        {
            PartyId = party.Id,
            BattleId = battle.Id,
            Hour = 3,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.BattleInvalidPhase));
    }
}
