namespace Crpg.Module.Api.Models.Battles;

/// <summary>
/// A claimed campaign battle returned by the API.
/// </summary>
internal class CrpgBattle
{
    public int Id { get; set; }
    public CrpgRegion Region { get; set; }
    public IList<CrpgBattleFighter> Fighters { get; set; } = [];
    public IList<CrpgBattleParticipant> Participants { get; set; } = [];
    public IList<CrpgBattleSideBriefing> SideBriefings { get; set; } = [];
}
