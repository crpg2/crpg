namespace Crpg.Module.Api.Models.GameEvents;


// Copy of Crpg.Application.Games.Commands.CreateGameEventsCommand
internal class CrpgGameEventsCreateRequest
{
    public string Instance { get; set; } = string.Empty;
    public CrpgGameMode GameMode { get; set; }
    public IList<CrpgGameEvent> Events { get; set; } = Array.Empty<CrpgGameEvent>();
}
