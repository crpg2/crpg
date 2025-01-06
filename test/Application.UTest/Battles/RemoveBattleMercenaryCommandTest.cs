using Crpg.Application.Battles.Commands;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Parties;
using Crpg.Domain.Entities.Users;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Battles;

public class RemoveBattleMercenaryCommandTest : TestBase
{
    private IBattleService BattleService { get; } = new BattleService();
    [Test]
    public async Task ShouldReturnErrorIfBattleNotFound()
    {
        RemoveBattleMercenaryCommand.Handler handler = new(ActDb, BattleService);
        var res = await handler.Handle(new RemoveBattleMercenaryCommand
        {
            UserId = 99,
            BattleId = 99,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.BattleNotFound));
    }

    [Test]
    public async Task ShouldReturnErrorIfBattleMercenaryNotFound()
    {
        Battle battle = new();
        ActDb.Battles.Add(battle);
        await ActDb.SaveChangesAsync();

        RemoveBattleMercenaryCommand.Handler handler = new(ActDb, BattleService);
        var res = await handler.Handle(new RemoveBattleMercenaryCommand
        {
            UserId = 99,
            BattleId = battle.Id,
            RemovedMercenaryId = 1,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.MercenaryNotFound));
    }

    [Test]
    public async Task ShouldReturnErrorIfUserNotBattleFighter()
    {
        User user = new();
        Party party = new() { User = user };
        BattleMercenary mercenary = new() { Side = BattleSide.Attacker };
        Battle battle = new() { Mercenaries = { mercenary } };
        ActDb.Users.Add(user);
        ActDb.Battles.Add(battle);
        await ActDb.SaveChangesAsync();

        RemoveBattleMercenaryCommand.Handler handler = new(ActDb, BattleService);
        var res = await handler.Handle(new RemoveBattleMercenaryCommand
        {
            UserId = user.Id,
            BattleId = battle.Id,
            RemovedMercenaryId = mercenary.Id,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.FighterNotFound));
    }

    [Test]
    public async Task ShouldNotReturnErrorIfBattleMercenaryFound()
    {
        User user = new();
        Party party = new() { User = user };
        BattleFighter battleFighter = new() { Party = party, Commander = true, Side = BattleSide.Attacker};
        BattleMercenary mercenary = new() { Side = BattleSide.Attacker };
        Battle battle = new() { Fighters = { battleFighter }, Mercenaries = { mercenary } };
        ActDb.Battles.Add(battle);
        await ActDb.SaveChangesAsync();

        RemoveBattleMercenaryCommand.Handler handler = new(ActDb, BattleService);
        var res = await handler.Handle(new RemoveBattleMercenaryCommand
        {
            UserId = user.Id,
            BattleId = battle.Id,
            RemovedMercenaryId = mercenary.Id,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
    }
}
