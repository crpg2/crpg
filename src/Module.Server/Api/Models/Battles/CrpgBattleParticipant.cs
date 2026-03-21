using Crpg.Module.Api.Models.Users;

namespace Crpg.Module.Api.Models.Battles;

internal class CrpgBattleParticipant
{
    public int Id { get; set; }
    public CrpgBattleSide Side { get; set; }
    public CrpgUser User { get; set; } = null!;
}
