using Crpg.Application.Battles.Commands;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Parties;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Battles;

public class RemoveBattleParticipantCommandTest : TestBase
{
    [Test]
    public async Task ShouldReturnErrorIfPartyNotFound()
    {
        RemoveBattleParticipantCommand.Handler handler = new(ActDb);
        var res = await handler.Handle(new RemoveBattleParticipantCommand
        {
            PartyId = 99,
            BattleId = 99,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.PartyNotFound));
    }

    [Test]
    public async Task ShouldReturnErrorIfBattleNotFound()
    {
        Party party = new() { User = new User() };
        ActDb.Parties.Add(party);
        await ActDb.SaveChangesAsync();

        RemoveBattleParticipantCommand.Handler handler = new(ActDb);
        var res = await handler.Handle(new RemoveBattleParticipantCommand
        {
            PartyId = party.Id,
            BattleId = 99,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.BattleNotFound));
    }

    [Test]
    public async Task ShouldReturnErrorIfBattleNotInHiringPhase()
    {
        Party party = new() { User = new User() };
        Battle battle = new();
        ActDb.Battles.Add(battle);
        ActDb.Parties.Add(party);
        await ActDb.SaveChangesAsync();

        RemoveBattleParticipantCommand.Handler handler = new(ActDb);
        var res = await handler.Handle(new RemoveBattleParticipantCommand
        {
            PartyId = party.Id,
            BattleId = battle.Id,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.BattleInvalidPhase));
    }

    [Test]
    public async Task ShouldReturnErrorIfBattleParticipantNotFound()
    {
        Party party = new() { User = new User() };
        Battle battle = new() { Phase = BattlePhase.Hiring };
        ActDb.Battles.Add(battle);
        ActDb.Parties.Add(party);
        await ActDb.SaveChangesAsync();

        RemoveBattleParticipantCommand.Handler handler = new(ActDb);
        var res = await handler.Handle(new RemoveBattleParticipantCommand
        {
            PartyId = party.Id,
            BattleId = battle.Id,
            RemovedParticipantId = 1,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.BattleParticipantNotFound));
    }

    // TODO: FIXME:
    // [Test]
    // public async Task ShouldReturnErrorIfUserNotBattleFighterOrSelf()
    // {
    //     User user = new();
    //     Party party = new() { User = user };
    //     BattleMercenary mercenary = new() { Side = BattleSide.Attacker };
    //     Battle battle = new() { Mercenaries = { mercenary } };
    //     ActDb.Users.Add(user);
    //     ActDb.Battles.Add(battle);
    //     await ActDb.SaveChangesAsync();

    //     RemoveBattleMercenaryCommand.Handler handler = new(ActDb, BattleService);
    //     var res = await handler.Handle(new RemoveBattleMercenaryCommand
    //     {
    //         UserId = user.Id,
    //         BattleId = battle.Id,
    //         RemovedMercenaryId = mercenary.Id,
    //     }, CancellationToken.None);

    //     Assert.That(res.Errors, Is.Not.Null);
    //     Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.FighterNotFound));
    // }

    // TODO: FIXME:
    // [Test]
    // public async Task ShouldRemoveBattleMercenaryIfUserNotBattleFighter()
    // {
    //     User user = new();
    //     Party party = new() { User = user };
    //     BattleFighter battleFighter = new() { Party = party, Commander = true, Side = BattleSide.Attacker };
    //     BattleMercenary mercenary = new() { Side = BattleSide.Attacker };
    //     Battle battle = new() { Fighters = { battleFighter }, Mercenaries = { mercenary } };
    //     ActDb.Battles.Add(battle);
    //     await ActDb.SaveChangesAsync();

    //     RemoveBattleMercenaryCommand.Handler handler = new(ActDb, BattleService);
    //     var res = await handler.Handle(new RemoveBattleMercenaryCommand
    //     {
    //         UserId = user.Id,
    //         BattleId = battle.Id,
    //         RemovedMercenaryId = mercenary.Id,
    //     }, CancellationToken.None);

    //     Assert.That(res.Errors, Is.Null);
    // }

    // TODO: FIXME:
    // [Test]
    // public async Task ShouldRemoveBattleParticipantIfUserSelf()
    // { }

    // TODO: FIXME:
    // [Test]
    // public async Task ShouldRemoveBattleParticipantIfBattleParticipantTypeEqualsParty()
    // { }
}
