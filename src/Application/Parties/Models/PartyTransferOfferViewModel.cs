using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities.Parties;

namespace Crpg.Application.Parties.Models;

public record PartyTransferOfferViewModel : IMapFrom<PartyTransferOffer>
{
    public int Id { get; set; }
    public PartyVisibleViewModel Party { get; init; } = default!;
    public PartyVisibleViewModel TargetParty { get; init; } = default!;
    public PartyTransferOfferStatus Status { get; init; }
    public int Gold { get; set; }
    public float Troops { get; set; }
    public List<ItemStack> Items { get; set; } = [];
}
