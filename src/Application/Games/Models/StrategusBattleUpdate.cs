using Crpg.Application.Characters.Models;
using Crpg.Domain.Entities.Battles;

namespace Crpg.Application.Games.Models;

public record StrategusBattleUpdate
{
    public string Instance { get; init; } = string.Empty;
    public int BattleId { get; init; }
    public int AttackerTickets { get; init; }
    public int DefenderTickets { get; init; }
    public BattleSide? Winner { get; init; }
}
