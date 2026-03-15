using Crpg.Domain.Entities.Items;

namespace Crpg.Domain.Entities.Marketplace;

public class MarketplaceOfferAsset
{
    public int Id { get; set; }
    public int MarketplaceOfferId { get; set; }
    public MarketplaceOfferAssetSide Side { get; set; }
    public MarketplaceAssetType Type { get; set; }
    public int Amount { get; set; }
    public string? ItemId { get; set; }
    public int? UserItemId { get; set; }

    public MarketplaceOffer? MarketplaceOffer { get; set; }
    public UserItem? UserItem { get; set; }
}
