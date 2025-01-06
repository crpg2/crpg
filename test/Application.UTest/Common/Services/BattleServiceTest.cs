using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.UTest.Battles;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Parties;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Crpg.Application.UTest.Common.Services;

public class BattleServiceTest : TestBase
{
    private readonly BattleService _battleService = new();
    [Test]
    public async Task GetBattleFighterShouldReturnErrorIfUserNotFound()
    {
        var res = await _battleService.GetBattleFighter(ActDb, 1, 2, CancellationToken.None);
        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.UserNotFound));
    }

    [Test]
    public async Task GetBattleFighterShouldReturnErrorIfBattleNotFound()
    {
        User user = new();
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        var res = await _battleService.GetBattleFighter(ActDb, user.Id, 2, CancellationToken.None);
        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.BattleNotFound));
    }

    [Test]
    public async Task GetBattleFighterShouldReturnErrorIfUserNotAFighter()
    {
        User user = new();
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        Battle battle = new();
        ArrangeDb.Battles.Add(battle);
        await ArrangeDb.SaveChangesAsync();

        var res = await _battleService.GetBattleFighter(ActDb, user.Id, battle.Id, CancellationToken.None);
        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.FighterNotFound));
    }

    [Test]
    public async Task GetBattleFighterShouldNotReturnErrorIfUserIsAFighter()
    {
        User user = new();
        ArrangeDb.Users.Add(user);
        Party party = new() { User = user };
        ArrangeDb.Parties.Add(party);
        Battle battle = new() { Fighters = new() { new BattleFighter { Party = party, Side = BattleSide.Attacker, Commander = true } } };
        ArrangeDb.Battles.Add(battle);
        await ArrangeDb.SaveChangesAsync();

        var res = await _battleService.GetBattleFighter(ActDb, user.Id, 2, CancellationToken.None);
        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.FighterNotFound));
    }

    [Test]
    public async Task GetBattleMercenaryShouldReturnErrorIfUserNotFound()
    {
        var res = await _battleService.GetBattleMercenary(ActDb, 1, 2, CancellationToken.None);
        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.UserNotFound));
    }

    [Test]
    public async Task GetBattleMercenaryShouldReturnErrorIfBattleNotFound()
    {
        User user = new();
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        var res = await _battleService.GetBattleMercenary(ActDb, user.Id, 2, CancellationToken.None);
        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.BattleNotFound));
    }

    [Test]
    public async Task GetBattleMercenaryShouldReturnErrorIfUserNotAMercenary()
    {
        User user = new();
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        Battle battle = new();
        ArrangeDb.Battles.Add(battle);
        await ArrangeDb.SaveChangesAsync();

        var res = await _battleService.GetBattleMercenary(ActDb, user.Id, battle.Id, CancellationToken.None);
        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.MercenaryNotFound));
    }

    [Test]
    public async Task GetBattleMercenaryShouldNotReturnErrorIfUserIsAMercenary()
    {
        User user = new();
        ArrangeDb.Users.Add(user);
        Character character = new() { User = user };
        ArrangeDb.Characters.Add(character);
        Battle battle = new() { Mercenaries = new() { new BattleMercenary { Character = character, Side = BattleSide.Attacker} } };
        ArrangeDb.Battles.Add(battle);
        await ArrangeDb.SaveChangesAsync();

        var res = await _battleService.GetBattleFighter(ActDb, user.Id, battle.Id, CancellationToken.None);
        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.FighterNotFound));
    }
}
