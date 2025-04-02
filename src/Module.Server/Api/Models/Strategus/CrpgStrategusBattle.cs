using Crpg.Module.Api.Models.Strategus;

namespace Crpg.Module.Api.Models.Users;

// Copy of Crpg.Application.Battles.Models.BattleDetailedViewModel
internal class CrpgStrategusBattle
{
    public int Id { get; set; }
    public List<CrpgStrategusFighter> Fighters { get; set; } = default!;
    public List<CrpgStrategusMercenary> Mercenaries { get; set; } = default!;
    public int AttackerTotalTroops { get; set; }
    public int DefenderTotalTroops { get; set; }
    public BattlePhase Phase { get; set; }
    public DateTime? ScheduledFor { get; set; }

    public CrpgStrategusFighter? GetFighterByUser(CrpgUser user)
    {
        return Fighters.FirstOrDefault(f => f.Party?.User == user /*|| f.Settlement?.Owner.User == user*/);
    }

    public CrpgStrategusMercenary? GetMercenaryByUser(CrpgUser user)
    {
        return Mercenaries.FirstOrDefault(m => m.User == user);
    }
}
