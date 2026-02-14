namespace Crpg.Domain.Entities.Parties;

/// <summary>
/// TODO:.
/// </summary>
public class PartyTransferOffer
{
    public int Id { get; set; }
    public int PartyId { get; set; }
    public int TargetPartyId { get; set; }
    public PartyTransferOfferStatus Status { get; set; }
    public int Gold { get; set; }
    public float Troops { get; set; }
    public List<PartyTransferOfferItem> Items { get; set; } = [];

    /// <summary>See <see cref="TargetPartyId"/>.</summary>
    public Party? TargetParty { get; set; }

    public Party? Party { get; set; }
}
