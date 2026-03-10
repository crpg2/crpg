using Crpg.Domain.Common;
using NetTopologySuite.Geometries;

namespace Crpg.Domain.Entities.Battles;

public class Battle : AuditableEntity
{
    public int Id { get; set; }
    public BattlePhase Phase { get; set; }
    public Region Region { get; set; }
    public Point Position { get; set; } = Point.Empty;

    /// <summary>The date the battle takes place. Null if the battle is not yet scheduled.</summary>
    public DateTime? ScheduledFor { get; set; }

    public List<BattleFighter> Fighters { get; set; } = [];
    public List<BattleFighterApplication> FighterApplications { get; set; } = [];
    public List<BattleParticipant> Participants { get; set; } = [];
    public List<BattleMercenaryApplication> MercenaryApplications { get; set; } = [];
    public List<BattleSideBriefing> SideBriefings { get; set; } = [];
}
