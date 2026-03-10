namespace Crpg.Module.Api.Models.Campaign;

internal class CrpgSettlementCreation
{
    public string Name { get; set; } = null!;
    public CrpgSettlementType Type { get; set; }
    public CrpgCulture Culture { get; set; }
    public Point Position { get; set; } = null!;
    public string Scene { get; set; } = null!;
}

// Copy of Crpg.Domain.Entities.Settlements.SettlementType.
internal enum CrpgSettlementType
{
    Village,
    Castle,
    Town,
}
