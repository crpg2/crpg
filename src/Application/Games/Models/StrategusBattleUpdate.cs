using Crpg.Application.Characters.Models;
using Crpg.Domain.Entities.Battles;

namespace Crpg.Application.Games.Models;

public record StrategusBattleUpdate
{
    public int AttackersLost { get; init; }
    public int DefendersLost { get; init; }
    public BattleSide? Winner { get; init; }
}
