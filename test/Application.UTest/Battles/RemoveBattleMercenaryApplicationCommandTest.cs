using Crpg.Application.Battles.Commands;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Parties;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Battles;

public class RemoveBattleMercenaryApplicationCommandTest : TestBase
{
    [Test]
    public async Task ShouldReturnErrorIfPartyNotFound()
    {
        RemoveBattleMercenaryApplicationCommand.Handler handler = new(ActDb);
        var res = await handler.Handle(new RemoveBattleMercenaryApplicationCommand
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

        RemoveBattleMercenaryApplicationCommand.Handler handler = new(ActDb);
        var res = await handler.Handle(new RemoveBattleMercenaryApplicationCommand
        {
            PartyId = party.Id,
            BattleId = 99,
            // Character = character,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.BattleNotFound));
    }

    [Test]
    public async Task ShouldReturnErrorIfBattleNotInHiringPhase()
    {
        Party party = new() { User = new User() };
        Battle battle = new() { Phase = BattlePhase.Live };
        ActDb.Battles.Add(battle);
        ActDb.Parties.Add(party);
        await ActDb.SaveChangesAsync();

        RemoveBattleMercenaryApplicationCommand.Handler handler = new(ActDb);
        var res = await handler.Handle(new RemoveBattleMercenaryApplicationCommand
        {
            PartyId = party.Id,
            BattleId = battle.Id,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.BattleInvalidPhase));
    }

    [Test]
    public async Task ShouldRemoveBattleMercenaryApplication()
    {
        User user = new();
        Party party = new() { User = user };
        Character character = new() { User = user };
        Battle battle = new() { Phase = BattlePhase.Hiring };
        BattleMercenaryApplication application = new()
        {
            Battle = battle,
            Side = BattleSide.Attacker,
            Character = character,
        };

        ActDb.Characters.Add(character);
        ActDb.Battles.Add(battle);
        ActDb.Users.Add(user);
        ActDb.Parties.Add(party);
        ActDb.BattleMercenaryApplications.Add(application);
        await ActDb.SaveChangesAsync();

        RemoveBattleMercenaryApplicationCommand.Handler handler = new(ActDb);
        var res = await handler.Handle(new RemoveBattleMercenaryApplicationCommand
        {
            PartyId = user.Id,
            BattleId = battle.Id,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null, "Command should succeed without errors");
        Assert.That(await ActDb.BattleMercenaryApplications.FindAsync(application.Id),
            Is.Null, "Mercenary application should be deleted");
    }
}
