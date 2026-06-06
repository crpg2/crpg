using Crpg.Domain.Entities.GameEvents;

namespace Crpg.Application.Games.Models;

public record GameEventCreate
{
    public int? UserId { get; init; }
    public GameEventType Type { get; init; }
    public Dictionary<GameEventField, string>? EventData { get; init; }
}
