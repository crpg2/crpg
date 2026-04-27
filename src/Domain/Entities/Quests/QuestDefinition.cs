using Crpg.Domain.Common;
using Crpg.Domain.Entities.GameEvents;

namespace Crpg.Domain.Entities.Quests;

public class QuestDefinition : AuditableEntity
{
    public int Id { get; set; }
    public QuestType Type { get; set; }
    public GameEventType EventType { get; set; }
    public Dictionary<string, string>[]? EventFiltersJson { get; set; }
    public QuestAggregationType AggregationType { get; set; }
    public GameEventField? AggregationField { get; set; }
    public int RequiredValue { get; set; }
    public int RewardGold { get; set; }
    public int RewardExperience { get; set; }
    public bool IsActive { get; set; }
}

public enum QuestAggregationType
{
    Count,
    Sum,
}
