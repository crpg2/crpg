using System.Text.Json.Serialization;
using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities.GameEvents;
using Crpg.Domain.Entities.Quests;

namespace Crpg.Application.Quests.Models;

public record QuestDefinitionViewModel : IMapFrom<QuestDefinition>
{
    public int Id { get; init; }
    public QuestType Type { get; set; }
    public GameEventType EventType { get; init; }
    public QuestAggregationType AggregationType { get; init; }

    [JsonRequired]
    public GameEventField? AggregationField { get; init; }

    public Dictionary<string, string>[] EventFiltersJson { get; set; } = [];

    public int RequiredValue { get; init; }
    public int RewardGold { get; init; }
    public int RewardExperience { get; init; }
}
