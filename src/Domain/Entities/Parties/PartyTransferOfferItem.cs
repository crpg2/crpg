using Crpg.Domain.Entities.Items;

namespace Crpg.Domain.Entities.Parties;

/// <summary>
/// Item in a party transfer offer.
/// </summary>
public class PartyTransferOfferItem
{
    public int PartyTransferOfferId { get; set; }
    public string ItemId { get; set; } = string.Empty;
    public int Count { get; set; }

    public PartyTransferOffer? PartyTransferOffer { get; set; }
    public Item? Item { get; set; }
}
