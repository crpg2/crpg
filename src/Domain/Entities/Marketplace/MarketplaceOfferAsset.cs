using Crpg.Domain.Entities.Items;

namespace Crpg.Domain.Entities.Marketplace;

public class MarketplaceOfferAsset
{
    public int Id { get; set; }
    public int MarketplaceOfferId { get; set; }

    /// <summary>
    /// An offer must contain exactly 2 assets: one with <see cref="Side"/> == <see cref="MarketplaceOfferAssetSide.Offered"/> and another with <see cref="Side"/> == <see cref="MarketplaceOfferAssetSide.Requested"/>.
    /// </summary>
    public MarketplaceOfferAssetSide Side { get; set; }
    public int Gold { get; set; }
    public int HeirloomPoints { get; set; }

    /// <summary>
    /// Not null when <see cref="Side"/> == <see cref="MarketplaceOfferAssetSide.Offered"/> and the seller is listing a specific owned item for sale.
    /// </summary>
    public int? UserItemId { get; set; }

    /// <summary>
    /// Not null when <see cref="Side"/> == <see cref="MarketplaceOfferAssetSide.Requested"/>.
    /// </summary>
    public string? ItemId { get; set; }

    /// <summary>See <see cref="MarketplaceOfferId"/>.</summary>
    public MarketplaceOffer? MarketplaceOffer { get; set; }

    /// <summary>See <see cref="UserItemId"/>.</summary>
    public UserItem? UserItem { get; set; }

    /// <summary>See <see cref="ItemId"/>.</summary>
    public Item? Item { get; set; }
}
