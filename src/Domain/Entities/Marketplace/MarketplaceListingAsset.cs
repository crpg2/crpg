using Crpg.Domain.Entities.Items;

namespace Crpg.Domain.Entities.Marketplace;

public class MarketplaceListingAsset
{
    public int Id { get; set; }
    public int MarketplaceListingId { get; set; }

    /// <summary>
    /// A listing must contain exactly 2 assets: one with <see cref="Side"/> == <see cref="MarketplaceListingAssetSide.Offered"/> and another with <see cref="Side"/> == <see cref="MarketplaceListingAssetSide.Requested"/>.
    /// </summary>
    public MarketplaceListingAssetSide Side { get; set; }
    public int Gold { get; set; }
    public int HeirloomPoints { get; set; }

    /// <summary>
    /// The <see cref="UserItem"/> that the seller is offering for sale.
    /// Can not be null when <see cref="Side"/> == <see cref="MarketplaceListingAssetSide.Offered"/>.
    /// </summary>
    public int? UserItemId { get; set; }

    /// <summary>
    /// The <see cref="Item"/> that the seller wants in trade. Can be any item available in the shop, and can also have an upgrade level.
    /// Can not be null when <see cref="Side"/> == <see cref="MarketplaceListingAssetSide.Requested"/>.
    /// </summary>
    public string? ItemId { get; set; }

    /// <summary>See <see cref="MarketplaceListingId"/>.</summary>
    public MarketplaceListing? MarketplaceListing { get; set; }

    /// <summary>See <see cref="UserItemId"/>.</summary>
    public UserItem? UserItem { get; set; }

    /// <summary>See <see cref="ItemId"/>.</summary>
    public Item? Item { get; set; }
}
