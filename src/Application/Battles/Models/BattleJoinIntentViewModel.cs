using Crpg.Domain.Entities.Battles;

namespace Crpg.Application.Battles.Models;

public record BattleJoinIntentViewModel
{
    public BattleSide Side { get; init; }
}
