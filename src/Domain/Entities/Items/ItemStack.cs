using Crpg.Domain.Common;
using Crpg.Domain.Entities.Parties;
using Crpg.Domain.Entities.Settlements;

namespace Crpg.Domain.Entities.Items;

/// <summary>
/// A stack of items owned by a <see cref="Party"/>, <see cref="Settlement"/>,
/// or included in a <see cref="PartyTransferOffer"/>.
/// Exactly one of <see cref="PartyId"/>, <see cref="SettlementId"/>,
/// or <see cref="PartyTransferOfferId"/> must be set.
/// </summary>
public class ItemStack : AuditableEntity
{
    public int Id { get; set; }
    public string ItemId { get; set; } = string.Empty;
    public int Count { get; set; }

    public int? PartyId { get; set; }
    public int? SettlementId { get; set; }
    public int? PartyTransferOfferId { get; set; }

    public Item? Item { get; set; }
    public Party? Party { get; set; }
    public Settlement? Settlement { get; set; }
    public PartyTransferOffer? PartyTransferOffer { get; set; }
}
