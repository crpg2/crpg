using Crpg.Application.Users.Models;

namespace Crpg.Application.Marketplace.Models;

public record MarketplaceListingHistoryViewModel
{
    public int Id { get; init; }
    public UserPublicViewModel Seller { get; init; } = default!;
    public UserPublicViewModel Buyer { get; init; } = default!;
    public int GoldFee { get; init; }
    public DateTime AcceptedAt { get; init; }
    public MarketplaceListingAssetViewModel Offer { get; init; } = new();
    public MarketplaceListingAssetViewModel Request { get; init; } = new();
}
