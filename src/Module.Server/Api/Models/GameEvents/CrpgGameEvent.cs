namespace Crpg.Module.Api.Models.GameEvents;

internal class CrpgGameEvent
{
    public int? UserId { get; set; }
    public CrpgGameEventType Type { get; set; }
    public Dictionary<CrpgGameEventField, string>? EventData { get; set; }
}
