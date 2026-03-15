using Crpg.Domain.Common;
using Crpg.Domain.Entities.Users;

namespace Crpg.Domain.Entities.Marketplace;

public class MarketplaceOffer : AuditableEntity
{
    public int Id { get; set; }
    public int SellerUserId { get; set; }
    public int? BuyerUserId { get; set; }
    public MarketplaceOfferStatus Status { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? ClosedAt { get; set; }

    public User? SellerUser { get; set; }
    public User? BuyerUser { get; set; }
    public List<MarketplaceOfferAsset> Assets { get; set; } = [];
}
