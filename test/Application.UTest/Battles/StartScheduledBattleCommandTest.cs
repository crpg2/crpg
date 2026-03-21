using Crpg.Application.Battles.Commands;
using Crpg.Application.Battles.Models;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Parties;
using Crpg.Domain.Entities.Restrictions;
using Crpg.Domain.Entities.Settlements;
using Crpg.Domain.Entities.Users;
using Crpg.Sdk.Abstractions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Battles;

public class StartScheduledBattleCommandTest : TestBase
{
    [Test]
    public async Task ShouldReturnNullWhenNoBattleAvailable()
    {
        Mock<IDateTime> dt = new();
        dt.Setup(d => d.UtcNow).Returns(new DateTime(2024, 1, 1));

        var handler = new StartScheduledBattleCommand.Handler(ActDb, Mapper, dt.Object);
        var result = await handler.Handle(new StartScheduledBattleCommand
        {
            Region = Region.Eu,
            Instance = "server-1",
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Null);
        Assert.That(result.Data, Is.Null);
    }

    [Test]
    public async Task ShouldClaimScheduledBattlePastItsTime()
    {
        Battle battle = new()
        {
            Phase = BattlePhase.Scheduled,
            Region = Region.Eu,
            ScheduledFor = new DateTime(2024, 1, 1, 10, 0, 0),
        };
        ArrangeDb.Battles.Add(battle);
        await ArrangeDb.SaveChangesAsync();

        Mock<IDateTime> dt = new();
        dt.Setup(d => d.UtcNow).Returns(new DateTime(2024, 1, 1, 12, 0, 0));

        var handler = new StartScheduledBattleCommand.Handler(ActDb, Mapper, dt.Object);
        var result = await handler.Handle(new StartScheduledBattleCommand
        {
            Region = Region.Eu,
            Instance = "server-1",
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Null);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.Id, Is.EqualTo(battle.Id));

        var dbBattle = await AssertDb.Battles.FirstAsync();
        Assert.That(dbBattle.Phase, Is.EqualTo(BattlePhase.Live));
        Assert.That(dbBattle.GameServer, Is.EqualTo("server-1"));
    }

    [Test]
    public async Task ShouldNotClaimBattleNotYetPastScheduledTime()
    {
        ArrangeDb.Battles.Add(new Battle
        {
            Phase = BattlePhase.Scheduled,
            Region = Region.Eu,
            ScheduledFor = new DateTime(2024, 1, 2),
        });
        await ArrangeDb.SaveChangesAsync();

        Mock<IDateTime> dt = new();
        dt.Setup(d => d.UtcNow).Returns(new DateTime(2024, 1, 1));

        var handler = new StartScheduledBattleCommand.Handler(ActDb, Mapper, dt.Object);
        var result = await handler.Handle(new StartScheduledBattleCommand
        {
            Region = Region.Eu,
            Instance = "server-1",
        }, CancellationToken.None);

        Assert.That(result.Data, Is.Null);

        var dbBattle = await AssertDb.Battles.FirstAsync();
        Assert.That(dbBattle.Phase, Is.EqualTo(BattlePhase.Scheduled));
    }

    [Test]
    public async Task ShouldNotClaimBattleFromDifferentRegion()
    {
        ArrangeDb.Battles.Add(new Battle
        {
            Phase = BattlePhase.Scheduled,
            Region = Region.Na,
            ScheduledFor = new DateTime(2024, 1, 1),
        });
        await ArrangeDb.SaveChangesAsync();

        Mock<IDateTime> dt = new();
        dt.Setup(d => d.UtcNow).Returns(new DateTime(2024, 1, 2));

        var handler = new StartScheduledBattleCommand.Handler(ActDb, Mapper, dt.Object);
        var result = await handler.Handle(new StartScheduledBattleCommand
        {
            Region = Region.Eu,
            Instance = "server-1",
        }, CancellationToken.None);

        Assert.That(result.Data, Is.Null);
    }

    [Test]
    public async Task ShouldResumeLiveBattleForSameInstance()
    {
        ArrangeDb.Battles.Add(new Battle
        {
            Phase = BattlePhase.Live,
            Region = Region.Eu,
            GameServer = "server-1",
        });
        await ArrangeDb.SaveChangesAsync();

        Mock<IDateTime> dt = new();
        dt.Setup(d => d.UtcNow).Returns(new DateTime(2024, 1, 1));

        var handler = new StartScheduledBattleCommand.Handler(ActDb, Mapper, dt.Object);
        var result = await handler.Handle(new StartScheduledBattleCommand
        {
            Region = Region.Eu,
            Instance = "server-1",
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Null);
        Assert.That(result.Data, Is.Not.Null);

        // Should still be Live, not re-claimed.
        var dbBattle = await AssertDb.Battles.FirstAsync();
        Assert.That(dbBattle.Phase, Is.EqualTo(BattlePhase.Live));
        Assert.That(dbBattle.GameServer, Is.EqualTo("server-1"));
    }

    [Test]
    public async Task ShouldNotResumeLiveBattleFromDifferentInstance()
    {
        ArrangeDb.Battles.Add(new Battle
        {
            Phase = BattlePhase.Live,
            Region = Region.Eu,
            GameServer = "server-2",
        });
        // Also add a scheduled battle that could be claimed.
        ArrangeDb.Battles.Add(new Battle
        {
            Phase = BattlePhase.Scheduled,
            Region = Region.Eu,
            ScheduledFor = new DateTime(2024, 1, 1),
        });
        await ArrangeDb.SaveChangesAsync();

        Mock<IDateTime> dt = new();
        dt.Setup(d => d.UtcNow).Returns(new DateTime(2024, 1, 2));

        var handler = new StartScheduledBattleCommand.Handler(ActDb, Mapper, dt.Object);
        var result = await handler.Handle(new StartScheduledBattleCommand
        {
            Region = Region.Eu,
            Instance = "server-1",
        }, CancellationToken.None);

        // Should claim the scheduled battle, not resume the other instance's live battle.
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.Id, Is.Not.EqualTo(1)); // not the live battle

        var battles = await AssertDb.Battles.OrderBy(b => b.Id).ToArrayAsync();
        Assert.That(battles[0].Phase, Is.EqualTo(BattlePhase.Live));
        Assert.That(battles[0].GameServer, Is.EqualTo("server-2"));
        Assert.That(battles[1].Phase, Is.EqualTo(BattlePhase.Live));
        Assert.That(battles[1].GameServer, Is.EqualTo("server-1"));
    }

    [Test]
    public async Task ShouldReturnFightersAndParticipantsData()
    {
        User user = new() { Name = "TestUser", Platform = Platform.Steam, PlatformUserId = "123" };
        Character character = new() { User = user, Name = "Warrior", Level = 20, Class = CharacterClass.Infantry };
        Party party = new()
        {
            Troops = 10,
            User = new User(),
            Items = new List<ItemStack>
            {
                new() { ItemId = "item_sword_1", Count = 5 },
                new() { ItemId = "item_shield_2", Count = 3 },
            },
        };
        Settlement settlement = new()
        {
            Name = "Castle",
            Troops = 50,
            Owner = new Party { User = new User(), Troops = 1 },
            Items = new List<ItemStack>
            {
                new() { ItemId = "item_armor_1", Count = 2 },
            },
        };
        Battle battle = new()
        {
            Phase = BattlePhase.Scheduled,
            Region = Region.Eu,
            ScheduledFor = new DateTime(2024, 1, 1),
            Fighters =
            {
                new BattleFighter { Side = BattleSide.Attacker, Party = party },
                new BattleFighter { Side = BattleSide.Defender, Settlement = settlement },
            },
            Participants =
            {
                new BattleParticipant
                {
                    Side = BattleSide.Attacker,
                    Type = BattleParticipantType.Party,
                    Character = character,
                    CaptainFighter = new BattleFighter { Side = BattleSide.Attacker },
                },
            },
        };
        ArrangeDb.Battles.Add(battle);
        await ArrangeDb.SaveChangesAsync();

        Mock<IDateTime> dt = new();
        dt.Setup(d => d.UtcNow).Returns(new DateTime(2024, 1, 2));

        var handler = new StartScheduledBattleCommand.Handler(ActDb, Mapper, dt.Object);
        var result = await handler.Handle(new StartScheduledBattleCommand
        {
            Region = Region.Eu,
            Instance = "server-1",
        }, CancellationToken.None);

        // Party fighter.
        Assert.That(result.Data!.Fighters, Has.Count.EqualTo(2));
        var partyFighter = result.Data.Fighters.First(f => f.Party != null);
        Assert.That(partyFighter.Party!.Items.Select(i => i.ItemId), Is.EquivalentTo(new[] { "item_sword_1", "item_shield_2" }));
        Assert.That(partyFighter.Party!.Items.Select(i => i.Count), Is.EquivalentTo(new[] { 5, 3 }));
        Assert.That(partyFighter.Settlement, Is.Null);

        // Settlement fighter.
        var settlementFighter = result.Data.Fighters.First(f => f.Settlement != null);
        Assert.That(settlementFighter.Settlement!.Items.Select(i => i.ItemId), Is.EquivalentTo(new[] { "item_armor_1" }));
        Assert.That(settlementFighter.Settlement!.Items.Select(i => i.Count), Is.EquivalentTo(new[] { 2 }));
        Assert.That(settlementFighter.Party, Is.Null);

        // Participant.
        Assert.That(result.Data.Participants, Has.Count.EqualTo(1));
        var participant = result.Data.Participants[0];
        Assert.That(participant.Side, Is.EqualTo(BattleSide.Attacker));
        Assert.That(participant.User, Is.Not.Null);
        Assert.That(participant.User.Name, Is.EqualTo("TestUser"));
    }

    [Test]
    public async Task ShouldPreferResumingOverClaimingNew()
    {
        ArrangeDb.Battles.Add(new Battle
        {
            Phase = BattlePhase.Live,
            Region = Region.Eu,
            GameServer = "server-1",
        });
        ArrangeDb.Battles.Add(new Battle
        {
            Phase = BattlePhase.Scheduled,
            Region = Region.Eu,
            ScheduledFor = new DateTime(2024, 1, 1),
        });
        await ArrangeDb.SaveChangesAsync();

        Mock<IDateTime> dt = new();
        dt.Setup(d => d.UtcNow).Returns(new DateTime(2024, 1, 2));

        var handler = new StartScheduledBattleCommand.Handler(ActDb, Mapper, dt.Object);
        var result = await handler.Handle(new StartScheduledBattleCommand
        {
            Region = Region.Eu,
            Instance = "server-1",
        }, CancellationToken.None);

        // Should resume the live battle, not claim the scheduled one.
        var battles = await AssertDb.Battles.OrderBy(b => b.Id).ToArrayAsync();
        Assert.That(result.Data!.Id, Is.EqualTo(battles[0].Id));
        Assert.That(battles[1].Phase, Is.EqualTo(BattlePhase.Scheduled));
    }
}
