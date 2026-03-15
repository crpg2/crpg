using Crpg.Domain.Entities.Marketplace;

namespace Crpg.Application.Marketplace.Models;

public record MarketplaceOfferAssetInput
{
    public MarketplaceAssetType Type { get; init; }
    public int Amount { get; init; }
    public int? UserItemId { get; init; }
    public string? ItemId { get; init; }
}
