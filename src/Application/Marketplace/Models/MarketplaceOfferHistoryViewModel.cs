using Crpg.Application.Users.Models;

namespace Crpg.Application.Marketplace.Models;

public record MarketplaceOfferHistoryViewModel
{
    public int Id { get; init; }
    public UserPublicViewModel Seller { get; init; } = default!;
    public UserPublicViewModel Buyer { get; init; } = default!;
    public int GoldFee { get; init; }
    public DateTime AcceptedAt { get; init; }
    public MarketplaceOfferAssetViewModel Offer { get; init; } = new();
    public MarketplaceOfferAssetViewModel Request { get; init; } = new();
}
