using Crpg.Application.Battles.Commands;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Battles;

public class RemoveBattleMercenaryApplicationCommandTest : TestBase
{
    private IBattleService BattleService { get; } = new BattleService();

    [Test]
    public async Task ShouldReturnErrorIfBattleNotFound()
    {
        RemoveBattleMercenaryApplicationCommand.Handler handler = new(ActDb, BattleService);
        var res = await handler.Handle(new RemoveBattleMercenaryApplicationCommand
        {
            UserId = 99,
            BattleId = 99,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.BattleNotFound));
    }

    [Test]
    public async Task ShouldReturnErrorIfBattleNotInHiringPhase()
    {
        Battle battle = new() { Phase = BattlePhase.Live };
        ActDb.Battles.Add(battle);
        await ActDb.SaveChangesAsync();

        RemoveBattleMercenaryApplicationCommand.Handler handler = new(ActDb, BattleService);
        var res = await handler.Handle(new RemoveBattleMercenaryApplicationCommand
        {
            UserId = 1,
            BattleId = battle.Id,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.BattleInvalidPhase));
    }

    [Test]
    public async Task ShouldRemoveBattleMercenaryApplication()
    {
        User user = new();
        Character character = new() { User = user };
        Battle battle = new() { Phase = BattlePhase.Hiring };
        BattleMercenaryApplication application = new()
        {
            Battle = battle,
            Side = BattleSide.Attacker,
            Character = character,
        };

        ActDb.Users.Add(user);
        ActDb.Characters.Add(character);
        ActDb.Battles.Add(battle);
        ActDb.BattleMercenaryApplications.Add(application);
        await ActDb.SaveChangesAsync();

        RemoveBattleMercenaryApplicationCommand.Handler handler = new(ActDb, BattleService);
        var res = await handler.Handle(new RemoveBattleMercenaryApplicationCommand
        {
            UserId = user.Id,
            BattleId = battle.Id,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null, "Command should succeed without errors");
        Assert.That(await ActDb.BattleMercenaryApplications.FindAsync(application.Id),
            Is.Null, "Mercenary application should be deleted");
    }
}
