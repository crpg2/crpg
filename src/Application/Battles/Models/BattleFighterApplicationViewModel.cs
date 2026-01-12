using Crpg.Application.Common.Mappings;
using Crpg.Application.Parties.Models;
using Crpg.Domain.Entities.Battles;

namespace Crpg.Application.Battles.Models;

public record BattleFighterApplicationViewModel : IMapFrom<BattleFighterApplication>
{
    public int Id { get; init; }
    public PartyPublicViewModel Party { get; init; } = default!;
    public BattleSide Side { get; init; }
    public BattleFighterApplicationStatus Status { get; init; }
}
