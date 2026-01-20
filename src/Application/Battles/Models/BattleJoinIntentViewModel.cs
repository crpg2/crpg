using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities.Battles;

namespace Crpg.Application.Battles.Models;

public record BattleJoinIntentViewModel : IMapFrom<BattleJoinIntent>
{
    public int BattleId { get; init; }
    public BattleSide Side { get; init; }
}
