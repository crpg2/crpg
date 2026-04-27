using Crpg.Domain.Entities.GameEvents;

namespace Crpg.Application.Games.Models;

public record GameEventViewModel
{
    public int? UserId { get; init; }
    public GameEventType Type { get; init; }
    public Dictionary<GameEventField, string>? EventData { get; init; }
}
