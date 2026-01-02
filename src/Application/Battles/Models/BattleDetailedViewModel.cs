using System.Text.Json.Serialization;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Battles;
using NetTopologySuite.Geometries;

namespace Crpg.Application.Battles.Models;

public record BattleDetailedViewModel
{
    public int Id { get; init; }
    public Region Region { get; set; }
    public Point Position { get; set; } = default!;
    public BattlePhase Phase { get; set; }
    public BattleType Type { get; init; }
    public DateTime CreatedAt { get; set; }

    [JsonRequired]
    public DateTime? ScheduledFor { get; set; }

    [JsonRequired]
    public BattleSideDetailedViewModel Attacker { get; init; } = default!;

    [JsonRequired]
    public BattleSideDetailedViewModel Defender { get; init; } = default!;
}
