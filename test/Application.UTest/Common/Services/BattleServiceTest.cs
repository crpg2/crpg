using AutoMapper;
using Crpg.Application.Battles.Models;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Parties;
using Crpg.Domain.Entities.Users;
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

        var res = await _battleService.GetBattleFighter(ActDb, user.Id, battle.Id, CancellationToken.None);
        Assert.That(res.Errors, Is.Null);
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
        Battle battle = new() { Mercenaries = new() { new BattleMercenary { Character = character, Side = BattleSide.Attacker } } };
        ArrangeDb.Battles.Add(battle);
        await ArrangeDb.SaveChangesAsync();

        var res = await _battleService.GetBattleMercenary(ActDb, user.Id, battle.Id, CancellationToken.None);
        Assert.That(res.Errors, Is.Null);
    }

    [Test]
    public void MapToBattleDetailedViewModel_ShouldMapCorrectly()
    {
        var attackerUser = new User();
        var defenderUser = new User();

        var attackerParty = new Party { User = attackerUser, Troops = 20 };
        var defenderParty = new Party { User = defenderUser, Troops = 15 };

        var attackerCommander = new BattleFighter
        {
            Side = BattleSide.Attacker,
            Commander = true,
            Party = attackerParty,
        };

        var defenderCommander = new BattleFighter
        {
            Side = BattleSide.Defender,
            Commander = true,
            Party = defenderParty,
        };

        var otherFighter = new BattleFighter
        {
            Side = BattleSide.Attacker,
            Commander = false,
            Party = new Party { Troops = 10 },
        };

        var battle = new Battle
        {
            Id = 1,
            Phase = BattlePhase.Scheduled,
            Fighters = new List<BattleFighter> { attackerCommander, defenderCommander, otherFighter },
            CreatedAt = DateTime.UtcNow,
            ScheduledFor = DateTime.UtcNow.AddHours(2),
        };

        var vm = _battleService.MapToBattleDetailedViewModel(Mapper, battle);

        Assert.That(vm.Id, Is.EqualTo(1));
        Assert.That(vm.AttackerTotalTroops, Is.EqualTo(30)); // 20 + 10
        Assert.That(vm.DefenderTotalTroops, Is.EqualTo(15));
        Assert.That(vm.Attacker.Commander, Is.True);
        Assert.That(vm.Defender?.Commander, Is.True);
        Assert.That(vm.Type, Is.EqualTo(BattleType.Battle));
    }
}
