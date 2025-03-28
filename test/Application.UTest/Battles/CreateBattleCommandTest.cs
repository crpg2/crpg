using Crpg.Application.Battles.Commands;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Parties;
using Crpg.Domain.Entities.Users;
using Crpg.Sdk.Abstractions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace Crpg.Application.UTest.Battles;

public class CreateBattleCommandTest : TestBase
{
    [Test]
    public async Task ShouldReturnErrorIfAttackerPartyNotFound()
    {
        CreateBattleCommand.Handler handler = new(ActDb, Mapper, Mock.Of<IDateTime>());
        var res = await handler.Handle(new()
        {
            ScheduledFor = new DateTime(2025, 1, 1),
            Region = Region.Eu,
            AttackerId = 1,
            DefenderId = 2,
            AttackerTroops = 1,
            DefenderTroops = 1,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.PartyNotFound));
    }

    [Test]
    public async Task ShouldReturnErrorIfDefenderPartyNotFound()
    {
        CreateBattleCommand.Handler handler = new(ActDb, Mapper, Mock.Of<IDateTime>());

        Party attackerParty = new()
        {
            User = new User(),
            Status = PartyStatus.Idle,
        };

        ArrangeDb.Parties.Add(attackerParty);
        await ArrangeDb.SaveChangesAsync();

        var res = await handler.Handle(new()
        {
            ScheduledFor = new DateTime(2025, 1, 1),
            Region = Region.Eu,
            AttackerId = attackerParty.Id,
            DefenderId = 5,
            AttackerTroops = 1,
            DefenderTroops = 1,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.PartyNotFound));
    }

    [Test]
    public async Task ShouldReturnErrorIfAttackerPartyInBattle()
    {
        CreateBattleCommand.Handler handler = new(ActDb, Mapper, Mock.Of<IDateTime>());

        Party attackerParty = new()
        {
            User = new User(),
            Status = PartyStatus.InBattle,
        };

        Party defenderParty = new()
        {
            User = new User(),
            Status = PartyStatus.Idle,
        };

        ArrangeDb.Parties.AddRange(attackerParty, defenderParty);
        await ArrangeDb.SaveChangesAsync();

        var res = await handler.Handle(new()
        {
            ScheduledFor = new DateTime(2025, 1, 1),
            Region = Region.Eu,
            AttackerId = attackerParty.Id,
            DefenderId = defenderParty.Id,
            AttackerTroops = 1,
            DefenderTroops = 1,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.PartyInBattle));
    }

    [Test]
    public async Task ShouldReturnErrorIfDefenderPartyInBattle()
    {
        CreateBattleCommand.Handler handler = new(ActDb, Mapper, Mock.Of<IDateTime>());

        Party attackerParty = new()
        {
            User = new User(),
            Status = PartyStatus.Idle,
        };

        Party defenderParty = new()
        {
            User = new User(),
            Status = PartyStatus.InBattle,
        };

        ArrangeDb.Parties.AddRange(attackerParty, defenderParty);
        await ArrangeDb.SaveChangesAsync();

        var res = await handler.Handle(new()
        {
            ScheduledFor = new DateTime(2025, 1, 1),
            Region = Region.Eu,
            AttackerId = attackerParty.Id,
            DefenderId = defenderParty.Id,
            AttackerTroops = 1,
            DefenderTroops = 1,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.PartyInBattle));
    }

    [Test]
    public async Task ShouldCreateBattle()
    {
        CreateBattleCommand.Handler handler = new(ActDb, Mapper, Mock.Of<IDateTime>());

        Party attackerParty = new()
        {
            User = new User(),
            Status = PartyStatus.Idle,
        };

        Party defenderParty = new()
        {
            User = new User(),
            Status = PartyStatus.Idle,
        };

        ArrangeDb.Parties.AddRange(attackerParty, defenderParty);
        await ArrangeDb.SaveChangesAsync();

        var res = await handler.Handle(new()
        {
            ScheduledFor = new DateTime(2025, 1, 1),
            Region = Region.Eu,
            AttackerId = attackerParty.Id,
            DefenderId = defenderParty.Id,
            AttackerTroops = 100,
            DefenderTroops = 100,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        Assert.That(res.Data!.Attacker.Party!.Id, Is.EqualTo(attackerParty.Id));
        Assert.That(res.Data!.AttackerTotalTroops, Is.EqualTo(100));
        Assert.That(res.Data!.Defender!.Party!.Id, Is.EqualTo(defenderParty.Id));
        Assert.That(res.Data!.DefenderTotalTroops, Is.EqualTo(100));
        Assert.That(res.Data!.ScheduledFor, Is.EqualTo(new DateTime(2025, 1, 1)));
        Assert.That(res.Data!.Region, Is.EqualTo(Region.Eu));
    }
}
