using Crpg.Domain.Common;

namespace Crpg.Domain.Entities.Quests;

public class WeeklyQuestAssignment : AuditableEntity
{
    public int Id { get; set; }
    public int QuestDefinitionId { get; set; }
    public DateTime AssignedAt { get; set; }
    public DateTime ExpiresAt { get; set; }

    public QuestDefinition? QuestDefinition { get; set; }
}
