using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities.Marketplace;

namespace Crpg.Application.Marketplace.Models;

public record MarketplaceOfferViewModel : IMapFrom<MarketplaceOffer>
{
    public int Id { get; init; }
    public int SellerUserId { get; init; }
    public int? BuyerUserId { get; init; }
    public MarketplaceOfferStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public DateTime ExpiresAt { get; init; }
    public DateTime? ClosedAt { get; init; }
    public List<MarketplaceOfferAssetViewModel> Assets { get; init; } = [];
}
