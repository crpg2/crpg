using System.Text.Json.Serialization;
using Crpg.Application.Settlements.Models;

namespace Crpg.Application.Battles.Models;

public record BattleSideDetailedViewModel
{
    public BattleFighterViewModel Commander { get; init; } = default!;
    [JsonRequired]
    public SettlementPublicViewModel? Settlement { get; init; }
    public int TotalTroops { get; init; }
    [JsonRequired]
    public BattleMercenaryApplicationViewModel? MercenaryApplication { get; init; }
    public int TotalParticipantSlots { get; init; }
    public BattleSideBriefingViewModel Briefing { get; init; } = default!;
}
