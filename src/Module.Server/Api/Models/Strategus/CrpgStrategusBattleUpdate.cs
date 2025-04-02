using Crpg.Module.Api.Models.Characters;

namespace Crpg.Module.Api.Models.Strategus;

// Copy of Crpg.Application.Games.Models.Strategus.StrategusBattleUpdate
internal class CrpgStrategusBattleUpdate
{
    public string Instance { get; set; } = string.Empty;
    public int BattleId { get; set; }
    public int AttackerTickets { get; set; }
    public int DefenderTickets { get; set; }
    public BattleSide? Winner { get; set; }
}
