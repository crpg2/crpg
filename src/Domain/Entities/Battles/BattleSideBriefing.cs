using Crpg.Domain.Common;

namespace Crpg.Domain.Entities.Battles;

public class BattleSideBriefing : AuditableEntity
{
    public int Id { get; set; }

    public int BattleId { get; set; }

    public BattleSide Side { get; set; }

    /// <summary>
    /// Free-form text: Discord, equipment, requirements, etc.
    /// </summary>
    public string Note { get; set; } = string.Empty;

    public Battle? Battle { get; set; }
}
