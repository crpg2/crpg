using Crpg.Domain.Common;
using Crpg.Domain.Entities.Servers;
using Crpg.Domain.Entities.Users;

namespace Crpg.Domain.Entities.GameEvents;

public class GameEvent : AuditableEntity
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string Instance { get; set; } = string.Empty;
    public GameMode GameMode { get; set; }
    public GameEventType Type { get; set; }
    public Dictionary<GameEventField, string>? EventData { get; set; }
    public User? User { get; set; }
}
