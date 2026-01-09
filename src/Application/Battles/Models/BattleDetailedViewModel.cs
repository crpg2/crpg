using System.Text.Json.Serialization;
using Crpg.Application.Settlements.Models;
using Crpg.Application.Terrains.Models;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Settlements;
using Crpg.Domain.Entities.Terrains;
using NetTopologySuite.Geometries;

namespace Crpg.Application.Battles.Models;

public record BattleDetailedViewModel
{
    public int Id { get; init; }
    public Region Region { get; set; }
    public Point Position { get; set; } = default!;
    public TerrainViewModel Terrain { get; set; } = default!;
    public SettlementPublicViewModel NearestSettlement { get; set; } = default!;
    public BattlePhase Phase { get; set; }
    public BattleType Type { get; init; }
    public DateTime CreatedAt { get; set; }
    [JsonRequired]
    public DateTime? ScheduledFor { get; set; }
    public BattleSideDetailedViewModel Attacker { get; init; } = default!;
    public BattleSideDetailedViewModel Defender { get; init; } = default!;
}
