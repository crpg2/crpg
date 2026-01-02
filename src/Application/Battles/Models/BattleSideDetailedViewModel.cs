using System.Text.Json.Serialization;
using Crpg.Domain.Entities.Battles;

namespace Crpg.Application.Battles.Models;

public record BattleSideDetailedViewModel
{
    public BattleFighterViewModel Fighter { get; init; } = default!;
    public int TotalTroops { get; init; }

    [JsonRequired]
    public BattleMercenaryApplicationStatus? ApplicationStatus { get; init; }

    [JsonRequired]
    public BattleSideBriefingViewModel? Briefing { get; init; }
}
