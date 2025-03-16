using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Battles;

namespace Crpg.GameServerManager.Api.Models;
public record CrpgBattle
{
    public int Id { get; init; }
    public Region Region { get; init; }
    public BattlePhase Phase { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ScheduledFor { get; set; }
    public string? Instance { get; set; }
}
