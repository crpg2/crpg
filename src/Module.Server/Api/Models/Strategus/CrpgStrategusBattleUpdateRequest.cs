using Crpg.Module.Api.Models.Characters;

namespace Crpg.Module.Api.Models.Strategus;

// Copy of Crpg.Application.Games.Models.Strategus.StrategusBattleUpdate
internal class CrpgStrategusBattleUpdateRequest
{
    public CrpgStrategusBattleUpdate Update { get; set; } = default!;
}
