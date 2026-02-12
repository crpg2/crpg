namespace Crpg.Application.Parties.Models;

public record PartyTransferOfferUpdate
{
    public int Gold { get; init; }
    public float Troops { get; init; }
    public ItemStackUpdate[] Items { get; init; } = default!;
}
