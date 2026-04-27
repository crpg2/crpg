namespace Crpg.Application.Marketplace.Models;

public record MarketplaceListingAssetInput
{
    public int? Gold { get; init; }
    public int? HeirloomPoints { get; init; }
    public int? UserItemId { get; init; }
    public string? ItemId { get; init; }
}
