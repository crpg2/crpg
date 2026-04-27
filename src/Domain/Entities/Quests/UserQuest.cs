using Crpg.Domain.Common;
using Crpg.Domain.Entities.Users;

namespace Crpg.Domain.Entities.Quests;

public class UserQuest : AuditableEntity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int QuestDefinitionId { get; set; }
    public bool IsRewardClaimed { get; set; }
    public DateTime ExpiresAt { get; set; }

    /// <summary>Used for optimistic concurrency.</summary>
    public uint Version { get; set; }

    public User? User { get; set; }
    public QuestDefinition? QuestDefinition { get; set; }
}
