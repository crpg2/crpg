using Crpg.Application.Quests.Services;
using Crpg.Domain.Entities.GameEvents;
using Crpg.Domain.Entities.Quests;
using NUnit.Framework;

namespace Crpg.Application.UTest.Quests.Services;

public class QuestEvaluationServiceTest : TestBase
{
    [Test]
    public async Task ComputeCurrentValueShouldCountOnlyMatchingEvents()
    {
        UserQuest userQuest = new()
        {
            UserId = 10,
            CreatedAt = new DateTime(2026, 04, 10, 15, 00, 00, DateTimeKind.Utc),
            QuestDefinition = new QuestDefinition
            {
                EventType = GameEventType.Kill,
                AggregationType = QuestAggregationType.Count,
            },
        };

        ArrangeDb.GameEvents.AddRange(
            new GameEvent { UserId = 10, Type = GameEventType.Kill, CreatedAt = new DateTime(2026, 04, 10, 00, 00, 00, DateTimeKind.Utc) },
            new GameEvent { UserId = 10, Type = GameEventType.Kill, CreatedAt = new DateTime(2026, 04, 11, 12, 00, 00, DateTimeKind.Utc) },
            new GameEvent { UserId = 10, Type = GameEventType.Block, CreatedAt = new DateTime(2026, 04, 11, 12, 00, 00, DateTimeKind.Utc) },
            new GameEvent { UserId = 11, Type = GameEventType.Kill, CreatedAt = new DateTime(2026, 04, 11, 12, 00, 00, DateTimeKind.Utc) },
            new GameEvent { UserId = 10, Type = GameEventType.Kill, CreatedAt = new DateTime(2026, 04, 09, 23, 59, 59, DateTimeKind.Utc) });
        await ArrangeDb.SaveChangesAsync();

        QuestEvaluationService service = new(ActDb);

        var values = await service.ComputeCurrentValuesAsync([userQuest]);

        Assert.That(values[userQuest.Id], Is.EqualTo(2));
    }

    [Test]
    public async Task ComputeCurrentValueShouldApplyEventFilters()
    {
        UserQuest userQuest = new()
        {
            UserId = 10,
            CreatedAt = new DateTime(2026, 04, 10, 08, 00, 00, DateTimeKind.Utc),
            QuestDefinition = new QuestDefinition
            {
                EventType = GameEventType.Hit,
                AggregationType = QuestAggregationType.Count,
                EventFiltersJson =
                [
                    new Dictionary<string, string>
                    {
                        ["WeaponClass"] = "Sword",
                        ["TargetType"] = "Player",
                    },
                    new Dictionary<string, string>
                    {
                        ["HitType"] = "Headshot",
                    },
                ],
            },
        };

        ArrangeDb.GameEvents.AddRange(
            new GameEvent
            {
                UserId = 10,
                Type = GameEventType.Hit,
                CreatedAt = new DateTime(2026, 04, 10, 09, 00, 00, DateTimeKind.Utc),
                EventData = new Dictionary<GameEventField, string>
                {
                    [GameEventField.WeaponClass] = "Sword",
                    [GameEventField.TargetType] = "Player",
                },
            },
            new GameEvent
            {
                UserId = 10,
                Type = GameEventType.Hit,
                CreatedAt = new DateTime(2026, 04, 10, 10, 00, 00, DateTimeKind.Utc),
                EventData = new Dictionary<GameEventField, string>
                {
                    [GameEventField.HitType] = "Headshot",
                },
            },
            new GameEvent
            {
                UserId = 10,
                Type = GameEventType.Hit,
                CreatedAt = new DateTime(2026, 04, 10, 11, 00, 00, DateTimeKind.Utc),
                EventData = new Dictionary<GameEventField, string>
                {
                    [GameEventField.WeaponClass] = "Sword",
                },
            },
            new GameEvent
            {
                UserId = 10,
                Type = GameEventType.Hit,
                CreatedAt = new DateTime(2026, 04, 10, 12, 00, 00, DateTimeKind.Utc),
                EventData = null,
            });
        await ArrangeDb.SaveChangesAsync();

        QuestEvaluationService service = new(ActDb);

        var values = await service.ComputeCurrentValuesAsync([userQuest]);

        Assert.That(values[userQuest.Id], Is.EqualTo(2));
    }

    [Test]
    public async Task ComputeCurrentValueShouldSumOnlyParseableValues()
    {
        UserQuest userQuest = new()
        {
            UserId = 10,
            CreatedAt = new DateTime(2026, 04, 10, 08, 00, 00, DateTimeKind.Utc),
            QuestDefinition = new QuestDefinition
            {
                EventType = GameEventType.Hit,
                AggregationType = QuestAggregationType.Sum,
                AggregationField = GameEventField.Damage,
            },
        };

        ArrangeDb.GameEvents.AddRange(
            new GameEvent
            {
                UserId = 10,
                Type = GameEventType.Hit,
                CreatedAt = new DateTime(2026, 04, 10, 09, 00, 00, DateTimeKind.Utc),
                EventData = new Dictionary<GameEventField, string>
                {
                    [GameEventField.Damage] = "12",
                },
            },
            new GameEvent
            {
                UserId = 10,
                Type = GameEventType.Hit,
                CreatedAt = new DateTime(2026, 04, 10, 10, 00, 00, DateTimeKind.Utc),
                EventData = new Dictionary<GameEventField, string>
                {
                    [GameEventField.Damage] = "not-an-int",
                },
            },
            new GameEvent
            {
                UserId = 10,
                Type = GameEventType.Hit,
                CreatedAt = new DateTime(2026, 04, 10, 11, 00, 00, DateTimeKind.Utc),
                EventData = new Dictionary<GameEventField, string>
                {
                    [GameEventField.TargetType] = "Player",
                },
            });
        await ArrangeDb.SaveChangesAsync();

        QuestEvaluationService service = new(ActDb);

        var values = await service.ComputeCurrentValuesAsync([userQuest]);

        Assert.That(values[userQuest.Id], Is.EqualTo(12));
    }

    [Test]
    public async Task ComputeCurrentValueShouldReturnZeroWhenSumFieldIsNull()
    {
        UserQuest userQuest = new()
        {
            UserId = 10,
            CreatedAt = new DateTime(2026, 04, 10, 08, 00, 00, DateTimeKind.Utc),
            QuestDefinition = new QuestDefinition
            {
                EventType = GameEventType.Hit,
                AggregationType = QuestAggregationType.Sum,
                AggregationField = null,
            },
        };

        ArrangeDb.GameEvents.Add(new GameEvent
        {
            UserId = 10,
            Type = GameEventType.Hit,
            CreatedAt = new DateTime(2026, 04, 10, 09, 00, 00, DateTimeKind.Utc),
            EventData = new Dictionary<GameEventField, string>
            {
                [GameEventField.Damage] = "50",
            },
        });
        await ArrangeDb.SaveChangesAsync();

        QuestEvaluationService service = new(ActDb);

        var values = await service.ComputeCurrentValuesAsync([userQuest]);

        Assert.That(values[userQuest.Id], Is.EqualTo(0));
    }
}
