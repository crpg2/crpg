namespace Crpg.Application.Parties.Models;

public record ItemStackUpdate
{
    public string ItemId { get; init; } = default!;
    public int Count { get; init; }
}
